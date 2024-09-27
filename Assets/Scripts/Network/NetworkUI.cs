using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
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

    private async void Start()
    {
        await UnityServicesInitializer.InitializeUnityServices();

        // Add listeners to buttons
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private async void StartHost()
    {
        string playerName = hostNameInputField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

        // Store the player name in PlayerPrefs
        PlayerPrefs.SetString("PlayerName", playerName);

        await UnityServicesInitializer.InitializeUnityServices();
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(4); // Allow up to 4 connections
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            statusText.text = $"Join Code: {joinCode}";
            Debug.Log($"Join code generated: {joinCode}");

            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started.");

            lobbyManager.UpdateLobbyUI();
            lobbyManager.DisplayRoomCode(joinCode);
            joinPanel.SetActive(false);
            lobbyPanel.SetActive(true);

            gameManager.InitializeGameManager();
            lobbyManager.EnableStartGameButton();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Service Error: {e.Message}");
            statusText.text = $"Relay Service Error: {e.Message}";
        }
    }

    private async void StartClient()
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

        // Store the player name in PlayerPrefs
        PlayerPrefs.SetString("PlayerName", playerName);

        await UnityServicesInitializer.InitializeUnityServices();
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started.");

            gameManager.InitializeGameManager();

            lobbyManager.UpdateLobbyUI();
            joinPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Service Error: {e.Message}");
            statusText.text = $"Relay Service Error: {e.Message}";
        }
    }
}
