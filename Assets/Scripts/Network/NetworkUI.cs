using System.Collections;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviourPunCallbacks
{
    public InputField hostNameInputField;
    public InputField clientNameInputField;
    public InputField joinCodeInputField; // This will act as the room name input for joining
    public Button hostButton;
    public Button clientButton;
    public TMP_Text statusText;
    public LobbyManager lobbyManager;
    public GameObject joinPanel;
    public GameObject lobbyPanel;
    public GameManager gameManager;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // Add listeners to buttons
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        string playerName = hostNameInputField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

        // Store the player name in PlayerPrefs
        PhotonNetwork.NickName = playerName;

        // Create room options and settings
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };

        // Create a room with a unique name
        string roomName = "Room_" + Random.Range(1000, 9999); // You can modify this to use more meaningful room names
        PhotonNetwork.CreateRoom(roomName, roomOptions);

        statusText.text = "Creating room...";
        Debug.Log("Creating room...");
    }

    private void StartClient()
    {
        string playerName = clientNameInputField.text;
        string roomName = joinCodeInputField.text; // Use as room name for joining

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "Please enter a room name.";
            return;
        }

        // Store the player name in PlayerPrefs
        PhotonNetwork.NickName = playerName;

        PhotonNetwork.JoinRoom(roomName);
        statusText.text = "Joining room...";
        Debug.Log("Joining room...");
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        statusText.text = "Room created successfully!";
        Debug.Log("Room created successfully.");

        lobbyManager.UpdateLobbyUI();
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        gameManager.InitializeGameManager();
        lobbyManager.EnableStartGameButton();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        statusText.text = $"Failed to create room: {message}";
        Debug.LogError($"Failed to create room: {message}");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        statusText.text = "Joined room successfully!";
        Debug.Log("Joined room successfully.");

        gameManager.InitializeGameManager();

        lobbyManager.UpdateLobbyUI();
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        statusText.text = $"Failed to join room: {message}";
        Debug.LogError($"Failed to join room: {message}");
    }
}
