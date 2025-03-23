using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Collections;

public class GlobaGameManager : NetworkBehaviour
{
    public static GlobaGameManager Instance {  get; private set; }

    float totalGameTime = 30f * 60f;
    NetworkVariable<float> remainingTime = new NetworkVariable<float>(0f);

    NetworkVariable<int> score = new NetworkVariable<int>(10000);  // Changed to NetworkVariable

    int resetCount = 0;

    bool gameStarted = false;
    bool isPaused = true;


    public TMP_Text timerText;
    public TMP_Text resetCountText;
    public TMP_Text scoreText;

    Dictionary<ulong,string>playerNames = new Dictionary<ulong,string>();
    string lobbyNumber = "Lobby_1";

    public Canvas gameCanvas;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            remainingTime.Value = totalGameTime;
        }

        // Register callbacks for the NetworkVariables to update clients when the values change
        remainingTime.OnValueChanged += (oldValue, newValue) => UpdateTimerText();
        score.OnValueChanged += (oldValue, newValue) => UpdateScoreText();

        UpdateTimerText();
        UpdateScoreText();
    }
    private void InitializeUIComponents()
    {

        if (gameCanvas == null)
        {
            // Try to find the Canvas if not assigned in the Inspector
            gameCanvas = GetComponentInChildren<Canvas>();
        }
        if (timerText == null)
        {
            // Try to find the timerText if not assigned in the Inspector
            timerText = gameCanvas.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        }

        if (timerText == null || gameCanvas == null)
        {
            Debug.LogWarning("TimerText or GameCanvas is not properly assigned!");
        }
    }



    // Update is called once per frame
    void Update()
    {

        UpdateTimerText();
        if (IsServer && !isPaused)
        {

            if(gameStarted)
            {
                remainingTime.Value -=  Time.deltaTime;
                UpdateScoreBasedOnTime();

                if(remainingTime.Value <= 0)
                {
                    EndGame();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartSceneServerRpc();
            }
        }
    }

    void UpdateScoreBasedOnTime()
    {
        score.Value  = Mathf.Max(0,(int) (10000 * (remainingTime.Value/totalGameTime)));
        //scoreText.text = ($"Score : " + score.ToString());
    }
    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.Value.ToString();
    }
    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime.Value / 60);
        int seconds  = Mathf.FloorToInt(remainingTime.Value % 60);

        timerText.text = ("Time : "  + string.Format("{0:00}:{1:00}",minutes,seconds));
    }

    [ServerRpc(RequireOwnership = false)]
    void RestartSceneServerRpc()
    {
        resetCount++;
        RestartSceneClientRpc();
    }

    [ClientRpc]

    void RestartSceneClientRpc()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  // Restart the current scene
        PauseTimer();  // Pause the timer on scene reload
        InitializeUIComponents();  // Reinitialize UI components after scene load

        // Automatically resume the timer and gameplay after the scene loads
        StartCoroutine(ResumeGameAfterSceneLoad());
    }

    public void StartGame()
    {
        if (IsServer)
        {
            remainingTime.Value = totalGameTime;  // Initialize the timer with total game time
        }

        gameStarted = true;
        isPaused = false;  // Start the timer and scoring
        UpdateTimerText();
        Debug.Log("Game started.");
    }
    private IEnumerator ResumeGameAfterSceneLoad()
    {
        yield return new WaitForSeconds(1f);  // Give the scene time to load
        ResumeTimer();  // Resume the timer and gameplay
    }
    public void PauseTimer()
    {
        isPaused = true;
        gameCanvas.gameObject.SetActive(false);
        Debug.Log(gameStarted);
        Debug.Log(isPaused);
    }

    public void ResumeTimer()
    {
        if (!gameStarted)
        {
            StartGame();  // Ensure the game starts properly if not already started
        }

        if (gameStarted)
        {
            isPaused = false;  // Resume the timer only if the game has started
            UpdateTimerText();  // Update the timer to reflect the current time
        }

        gameCanvas.gameObject.SetActive(true);
        Debug.Log("Game resumed.");
    }

    public void EndGame()
    {
        gameStarted = false;

        // Collect player data
        foreach (var player in NetworkManager.Singleton.ConnectedClients)
        {
            if (player.Value != null && player.Value.PlayerObject != null)
            {
                if (player.Value.PlayerObject.TryGetComponent(out PlayerName playerNameComponent))
                {
                    playerNames[player.Key] = playerNameComponent.playerName.Value.ToString();
                }
                else
                {
                    Debug.LogWarning($"PlayerName component not found on player {player.Key}");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerObject not found for player {player.Key}");
            }
        }

        // Send data to Unity Analytics
        Analytics.CustomEvent("Game_End", new Dictionary<string, object>
        {
            { "lobbyNumber", lobbyNumber },
            { "remainingTime", remainingTime.Value },
            { "finalScore", score },
            { "resetCount", resetCount },
            { "players", string.Join(", ", playerNames.Values) }
        });

        Debug.Log("Game has ended. Data sent to Unity Analytics.");
    }
}
