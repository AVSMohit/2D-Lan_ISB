using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class NetworkUI : MonoBehaviour
{
    public InputField nameInputField;
    public InputField joinCodeInputField;
    public Button hostButton;
    public Button clientButton;
    public TMP_Text statusText;
    public LobbyManager lobbyManager; // Reference to the LobbyManager
    public GameObject joinPanel; // Reference to the Join Panel
    public GameObject lobbyPanel; // Reference to the Lobby Panel

    private async void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        await UnityServicesInitializer.InitializeUnityServices();
    }

    private async void StartHost()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Please enter a name.";
            return;
        }

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
            lobbyManager.DisplayRoomCode(joinCode); // Display the room code in the lobby panel
            joinPanel.SetActive(false);
            lobbyPanel.SetActive(true);

            // Enable Start Game button for the host
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
        string playerName = nameInputField.text;
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

        PlayerPrefs.SetString("PlayerName", playerName);

        await UnityServicesInitializer.InitializeUnityServices();
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started.");

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
