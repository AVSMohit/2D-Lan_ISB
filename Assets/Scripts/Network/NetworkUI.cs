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
    public TMP_InputField joinCodeInputField;
    public Button hostButton;
    public Button clientButton;
    public TMP_Text joinCodeDisplay;
    public TMP_Text statusText;
    public TMP_InputField nameInputField;
    private async void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        Logger.Log("NetworkUI started.");

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

        Logger.Log("Starting Host...");
        Debug.Log("Starting Host...");

        await UnityServicesInitializer.InitializeUnityServices();
        var allocation = await RelayService.Instance.CreateAllocationAsync(4); // Allow up to 4 connections
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Logger.Log($"Join Code: {joinCode}");
        Debug.Log($"Join Code: {joinCode}");

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        if (statusText != null)
        {
            statusText.text = $"Join Code: {joinCode}";
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

        Logger.Log($"Attempting to join with code: {joinCode}");
        Debug.Log($"Attempting to join with code: {joinCode}");

        await UnityServicesInitializer.InitializeUnityServices();
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        if (NetworkManager.Singleton.StartClient())
        {
            Logger.Log("Client started successfully.");
            Debug.Log("Client started successfully.");
        }
        else
        {
            Logger.Log("Failed to start Client.");
            Debug.Log("Failed to start Client.");
        }
    }
}
