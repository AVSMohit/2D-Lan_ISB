using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text[] playerTexts;
    public Button startGameButton;
    public TMP_Text roomCodeText;
    public GameObject joinPanel;
    public GameObject lobbyPanel;

    [Header("Gender Selection")]
    public Button maleButton;
    public Button femaleButton;

    // Synced player list
    public NetworkList<ulong> connectedPlayers = new NetworkList<ulong>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Full resync on spawn
            connectedPlayers.Clear();

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (!connectedPlayers.Contains(kvp.Key))
                {
                    connectedPlayers.Add(kvp.Key);
                }
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        connectedPlayers.OnListChanged += OnConnectedPlayersChanged;

        maleButton.onClick.AddListener(() => SetGender("Male"));
        femaleButton.onClick.AddListener(() => SetGender("Female"));

        UpdateLobbyUI();
    }

    private void OnConnectedPlayersChanged(NetworkListEvent<ulong> changeEvent)
    {
        UpdateLobbyUI();

        if (IsServer && connectedPlayers.Count >= 2)
        {
            Debug.Log("✅ Room full. Starting game...");
            SceneTransitionManager.Instance.TransitionToScene("SplitPath");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");

        if (IsServer && !connectedPlayers.Contains(clientId))
        {
            connectedPlayers.Add(clientId);
        }

        SwitchToLobbyPanel();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");

        if (IsServer && connectedPlayers.Contains(clientId))
        {
            connectedPlayers.Remove(clientId);
        }
    }

    public void UpdateLobbyUI()
    {
        Debug.Log($"[LobbyManager] Updating UI. Connected players: {connectedPlayers.Count}");

        for (int i = 0; i < playerTexts.Length; i++)
        {
            if (i < connectedPlayers.Count)
            {
                playerTexts[i].text = $"Player {i + 1}";
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

    public void EnableStartGameButton()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = true;
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
    }

    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Start Game clicked. Loading game scene.");
            SceneTransitionManager.Instance.TransitionToScene("SplitPath");
        }
    }

    private void SwitchToLobbyPanel()
    {
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void SetGender(string gender)
    {
        PlayerPrefs.SetString("PlayerGender", gender);
        PlayerPrefs.Save();
        Debug.Log($"Gender set to: {gender}");
    }
}
