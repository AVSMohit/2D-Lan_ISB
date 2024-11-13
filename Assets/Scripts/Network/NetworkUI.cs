using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviourPunCallbacks
{
    public InputField hostNameInputField;
    public InputField clientNameInputField;
    public InputField joinCodeInputField;
    public Button hostButton;
    public Button clientButton;
    public TMP_Text statusText;
    public LobbyManager lobbyManager;
    public GameObject joinPanel;
    public GameObject lobbyPanel;
    public GameManager gameManager;

    private bool isConnectedToMaster = false;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "Connecting to Master Server...";

        // Add listeners to buttons
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    public override void OnConnectedToMaster()
    {
        isConnectedToMaster = true;
        statusText.text = "Connected to Master Server. Ready to host or join.";
    }

    private void StartHost()
    {
        string playerName = hostNameInputField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

        PhotonNetwork.NickName = playerName;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            statusText.text = "Leaving current room to return to Master Server...";
            return;
        }

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        string roomName = "Room_" + Random.Range(1000, 9999);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        statusText.text = $"Creating room with code: {roomName}...";
    }

    public override void OnCreatedRoom()
    {
        statusText.text = "Room created successfully!";
        lobbyManager.UpdateLobbyUI();
        lobbyManager.DisplayRoomCode(PhotonNetwork.CurrentRoom.Name);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        gameManager.InitializeGameManager();
        lobbyManager.EnableStartGameButton();
    }

    private void StartClient()
    {
        string playerName = clientNameInputField.text;
        string joinCode = joinCodeInputField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

        if (string.IsNullOrEmpty(joinCode))
        {
            statusText.text = "Please enter a join code.";
            return;
        }

        PhotonNetwork.NickName = playerName;
        PhotonNetwork.JoinRoom(joinCode);
        statusText.text = $"Joining room with code: {joinCode}...";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Successfully joined the room!";
        lobbyManager.UpdateLobbyUI();
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        // Display the room code or any relevant room information
        if (PhotonNetwork.CurrentRoom != null)
        {
            lobbyManager.DisplayRoomCode(PhotonNetwork.CurrentRoom.Name);
        }

        // Initialize other UI components as needed
        gameManager.InitializeGameManager();
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Failed to join room: {message}";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Failed to create room: {message}";
    }

    public override void OnLeftRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
        statusText.text = "Returning to Master Server...";
    }
}
