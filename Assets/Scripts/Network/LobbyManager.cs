using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public TMP_Text[] playerTexts; // Text fields for player slots
    public Button startGameButton;
    public TMP_Text roomCodeText; // Room code display
    public GameObject joinPanel;
    public GameObject lobbyPanel;

    public Button maleButton;
    public Button femaleButton;

    private List<ulong> connectedPlayers = new List<ulong>(); // Store connected player IDs

    private void Start()
    {
        maleButton.onClick.AddListener(() => SetGender("Male"));
        femaleButton.onClick.AddListener(() => SetGender("Female"));

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        UpdateLobbyUI();
    }

    public void SetGender(string gender)
    {
        PlayerPrefs.SetString("PlayerGender", gender);
        PlayerPrefs.Save();
        Debug.Log($"Gender set to: {gender}");

        // Apply new color immediately if the player is already in the game
        PlayerController localPlayer = FindObjectOfType<PlayerController>();
        if (localPlayer != null)
        {
            localPlayer.ApplyGenderColor();
        }
    }

    public void EnableStartGameButton()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = true;
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");

        if (!connectedPlayers.Contains(clientId))
        {
            connectedPlayers.Add(clientId);
        }

        UpdateLobbyUI();
        SwitchToLobbyPanel();

        if (connectedPlayers.Count >= 0)
        {
           StartGame();
        }
    }

    void StartGame()
    {
        if (NetworkManager.Singleton.IsHost) 
        {
            Debug.Log("Starting game...");
            SceneTransitionManager.Instance.TransitionToScene("SplitPath");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");

        if (connectedPlayers.Contains(clientId))
        {
            connectedPlayers.Remove(clientId);
        }

        UpdateLobbyUI();
    }

    public void UpdateLobbyUI()
    {
        for (int i = 0; i < playerTexts.Length; i++)
        {
            if (i < connectedPlayers.Count)
            {
                playerTexts[i].text = $"Player {i + 1}"; // Display as Player 1, Player 2, etc.
            }
            else
            {
                playerTexts[i].text = "Waiting for player...";
            }
        }
    }

    public void DisplayRoomCode(string roomCode)
    {
        roomCodeText.text = $"Room Code: {roomCode}";
    }

    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Start Game clicked. Loading GameScene.");
            SceneTransitionManager.Instance.TransitionToScene("SplitPath");
        }
    }

    private void SwitchToLobbyPanel()
    {
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
}
