using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GlobaGameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GlobaGameManager Instance { get; private set; }

    float totalGameTime = 30f * 60f;
    private float remainingTime;
    private int score = 10000;
    private int resetCount = 0;

    private bool gameStarted = false;
    private bool isPaused = true;

    public TMP_Text timerText;
    public TMP_Text resetCountText;
    public TMP_Text scoreText;

    private Dictionary<int, string> playerNames = new Dictionary<int, string>();
    private string lobbyNumber = "Lobby_1";

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

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            remainingTime = totalGameTime;
        }

        UpdateTimerText();
        UpdateScoreText();
    }

    private void InitializeUIComponents()
    {
        if (gameCanvas == null)
        {
            gameCanvas = GetComponentInChildren<Canvas>();
        }
        if (timerText == null)
        {
            timerText = gameCanvas.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        }

        if (timerText == null || gameCanvas == null)
        {
            Debug.LogWarning("TimerText or GameCanvas is not properly assigned!");
        }
    }

    private void Update()
    {
        UpdateTimerText();
        if (PhotonNetwork.IsMasterClient && !isPaused)
        {
            if (gameStarted)
            {
                remainingTime -= Time.deltaTime;
                UpdateScoreBasedOnTime();

                if (remainingTime <= 0)
                {
                    EndGame();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                photonView.RPC("RestartScene", RpcTarget.All);
            }
        }
    }

    private void UpdateScoreBasedOnTime()
    {
        score = Mathf.Max(0, (int)(10000 * (remainingTime / totalGameTime)));
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        if (timerText != null)
        {
            timerText.text = "Time : " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    [PunRPC]
    private void RestartScene()
    {
        resetCount++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PauseTimer();
        InitializeUIComponents();
        StartCoroutine(ResumeGameAfterSceneLoad());
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            remainingTime = totalGameTime;
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
        if (gameCanvas != null)
        {
            gameCanvas.gameObject.SetActive(false);
        }
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

        if (gameCanvas != null)
        {
            gameCanvas.gameObject.SetActive(true);
        }

        Debug.Log("Game resumed.");
    }

    public void EndGame()
    {
        gameStarted = false;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("PlayerName", out object playerName))
            {
                playerNames[player.ActorNumber] = playerName.ToString();
            }
        }

        Analytics.CustomEvent("Game_End", new Dictionary<string, object>
        {
            { "lobbyNumber", lobbyNumber },
            { "remainingTime", remainingTime },
            { "finalScore", score },
            { "resetCount", resetCount },
            { "players", string.Join(", ", playerNames.Values) }
        });

        Debug.Log("Game has ended. Data sent to Unity Analytics.");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(remainingTime);
            stream.SendNext(score);
            stream.SendNext(gameStarted);
            stream.SendNext(isPaused);
        }
        else
        {
            remainingTime = (float)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
            gameStarted = (bool)stream.ReceiveNext();
            isPaused = (bool)stream.ReceiveNext();
        }
    }
}
