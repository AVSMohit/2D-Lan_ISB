using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public TMP_Text[] playerTexts; // Array to hold player Text UI elements
    public Button startGameButton;
    public TMP_Text roomCodeText; // Text to display the room code
    public GameObject joinPanel;
    public GameObject lobbyPanel;

    private NetworkList<FixedString32Bytes> playerNames;

    private void Awake()
    {
        playerNames = new NetworkList<FixedString32Bytes>();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        playerNames.OnListChanged += OnPlayerNamesChanged;

        UpdateLobbyUI();
    }

    public void EnableStartGameButton()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Enabling Start Game button for host.");
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = true;
            startGameButton.onClick.AddListener(OnStartGameClicked);
            Debug.Log("Host joined. Start Game button should be interactable and visible.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");

        // If this is the host, notify clients to set their name,
        // but skip the host if it's acting as room creator.
        if (NetworkManager.Singleton.IsHost)
        {
            if (!(RoomSettings.IsRoomCreator && clientId == NetworkManager.Singleton.LocalClientId))
            {
                Debug.Log("Host detected OnClientConnected.");
                NotifyClientToSetNameClientRpc(clientId);
            }
        }

        UpdateLobbyUI();
        SwitchToLobbyPanel();
    }

    [ClientRpc]
    private void NotifyClientToSetNameClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // If this client is the host and is set as room creator, do not set a name.
            if (NetworkManager.Singleton.IsHost && RoomSettings.IsRoomCreator)
            {
                return;
            }

            string playerName = PlayerPrefs.GetString("PlayerName", $"Player {clientId}");
            SetPlayerNameServerRpc(playerName);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        playerNames.Add(playerName);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");
        if (NetworkManager.Singleton.IsHost)
        {
            RemovePlayerNameServerRpc(clientId);
        }

        UpdateLobbyUI();
    }

    private void OnPlayerNamesChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        UpdateLobbyUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerNameServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if ((int)clientId < playerNames.Count)
        {
            playerNames.RemoveAt((int)clientId);
        }
    }

    public void UpdateLobbyUI()
    {
        // Use the count from the replicated NetworkList instead of ConnectedClients.
        int playerCount = playerNames.Count;

        for (int i = 0; i < playerTexts.Length; i++)
        {
            if (i < playerCount)
            {
                // Display generic labels (e.g., "Player 1", "Player 2", etc.)
                playerTexts[i].text = "Player " + (i + 1).ToString();
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
            GlobaGameManager.Instance.StartGame();
        }
        else
        {
            Debug.Log("Start Game button should not be clickable for clients.");
        }
    }

    private void SwitchToLobbyPanel()
    {
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
}
