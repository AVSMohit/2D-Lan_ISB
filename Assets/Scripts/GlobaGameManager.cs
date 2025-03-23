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
    public static GlobaGameManager Instance { get; private set; }

    float totalGameTime = 30f * 60f;
    NetworkVariable<float> remainingTime = new NetworkVariable<float>(0f);
    NetworkVariable<int> score = new NetworkVariable<int>(10000);

    int resetCount = 0;
    bool gameStarted = false;
    bool isPaused = true;

    public TMP_Text timerText;
    public TMP_Text resetCountText;
    public TMP_Text scoreText;
    string lobbyNumber = "Lobby_1";

    public Canvas gameCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (IsServer)
        {
            remainingTime.Value = totalGameTime;
        }

        remainingTime.OnValueChanged += (oldValue, newValue) => UpdateTimerText();
        score.OnValueChanged += (oldValue, newValue) => UpdateScoreText();

        UpdateTimerText();
        UpdateScoreText();
    }

    void Update()
    {
        UpdateTimerText();
        if (IsServer && !isPaused)
        {
            if (gameStarted)
            {
                remainingTime.Value -= Time.deltaTime;
                UpdateScoreBasedOnTime();

                if (remainingTime.Value <= 0)
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
        score.Value = Mathf.Max(0, (int)(10000 * (remainingTime.Value / totalGameTime)));
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.Value.ToString();
    }

    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime.Value / 60);
        int seconds = Mathf.FloorToInt(remainingTime.Value % 60);

        timerText.text = ("Time : " + string.Format("{0:00}:{1:00}", minutes, seconds));
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PauseTimer();
        StartCoroutine(ResumeGameAfterSceneLoad());
    }

    public void StartGame()
    {
        if (IsServer)
        {
            remainingTime.Value = totalGameTime;
        }

        gameStarted = true;
        isPaused = false;
        UpdateTimerText();
        Debug.Log("Game started.");
    }

    private IEnumerator ResumeGameAfterSceneLoad()
    {
        yield return new WaitForSeconds(1f);
        ResumeTimer();
    }

    public void PauseTimer()
    {
        isPaused = true;
        gameCanvas.gameObject.SetActive(false);
    }

    public void ResumeTimer()
    {
        if (!gameStarted)
        {
            StartGame();
        }

        if (gameStarted)
        {
            isPaused = false;
            UpdateTimerText();
        }

        gameCanvas.gameObject.SetActive(true);
    }

    public void EndGame()
    {
        gameStarted = false;

        Analytics.CustomEvent("Game_End", new Dictionary<string, object>
        {
            { "lobbyNumber", lobbyNumber },
            { "remainingTime", remainingTime.Value },
            { "finalScore", score },
            { "resetCount", resetCount }
        });

        Debug.Log("Game has ended. Data sent to Unity Analytics.");
    }
}
