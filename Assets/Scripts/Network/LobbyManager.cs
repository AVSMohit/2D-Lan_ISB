using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_Text[] playerTexts; // Array to hold player Text UI elements
    public Button startGameButton;
    public TMP_Text roomCodeText; // Text to display the room code
    public GameObject joinPanel;
    public GameObject lobbyPanel;

    private List<string> playerNames = new List<string>();

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        UpdateLobbyUI();
    }

    public void EnableStartGameButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Enabling Start Game button for host.");
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = true;
            startGameButton.onClick.AddListener(OnStartGameClicked);
            Debug.Log("Host joined. Start Game button should be interactable and visible.");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} connected.");
        UpdateLobbyUI();
        SwitchToLobbyPanel();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} disconnected.");
        UpdateLobbyUI();
    }

    public void UpdateLobbyUI()
    {
        playerNames.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerNames.Add(player.NickName);
        }

        for (int i = 0; i < playerTexts.Length; i++)
        {
            if (i < playerNames.Count)
            {
                playerTexts[i].text = playerNames[i];
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
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Start Game clicked. Loading GameScene.");
            PhotonNetwork.LoadLevel("SplitPath");
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
