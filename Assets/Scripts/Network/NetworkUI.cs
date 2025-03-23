using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

//using UnityEditor.EditorTools;

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

    private void Start()
    {

        // Add listeners to buttons
        //  hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(JoinOrCreateRoom);
    }
    private async void JoinOrCreateRoom()
    {
        Debug.Log("🕒 Waiting for Unity Services...");
        await UnityServicesInitializer.WaitForInitialization();
        Debug.Log("✅ Unity Services Ready!");

        string roomCode = joinCodeInputField.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "Please enter a room code.";
            return;
        }

        Lobby joinedLobby = null;
        string relayJoinCode = null;

        // 🧠 QUERY ATTEMPT 1
        var query = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
    {
        new QueryFilter(
            field: QueryFilter.FieldOptions.Name,
            op: QueryFilter.OpOptions.EQ,
            value: roomCode)
    }
        });


        if (query.Results.Count == 0)
        {
            Debug.LogWarning("⚠ No matching lobby found. Retrying in 1 second...");
            await Task.Delay(1000); // Wait and try again

            // 🧠 QUERY ATTEMPT 2
            query = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
              new QueryFilter(
                field: QueryFilter.FieldOptions.Name,
                op: QueryFilter.OpOptions.EQ,
                value: roomCode
            )

            }
            });
        }

        if (query.Results.Count > 0)
        {
            joinedLobby = query.Results[0];
            relayJoinCode = joinedLobby.Data["relayCode"].Value;

            Debug.Log($"🎯 Found lobby with code {roomCode}, relay join code: {relayJoinCode}");

            try
            {
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                var relayData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
                NetworkManager.Singleton.StartClient();

                Debug.Log($"✅ Client joined room '{roomCode}'");
                RoomManager.ActiveRoomCode = roomCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"❌ Failed to join Relay: {e.Message}");
                statusText.text = "Failed to join room. Try again.";
                return;
            }
        }
        else
        {
            Debug.LogWarning("🛑 No lobby found after 2 attempts. Creating new lobby...");

            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(4);
                relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var createOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                {
                    { "relayCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
                };

                joinedLobby = await Lobbies.Instance.CreateLobbyAsync(roomCode, 4, createOptions);

                var relayData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
                NetworkManager.Singleton.StartHost();

                Debug.Log($"✅ Host created lobby '{roomCode}' with relay code '{relayJoinCode}'");
                RoomManager.ActiveRoomCode = roomCode;

                // Optional: start sending heartbeat to keep lobby alive
                _ = SendLobbyHeartbeat(joinedLobby.Id);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"❌ Relay or Lobby creation failed: {e.Message}");
                statusText.text = $"Error: {e.Message}";
                return;
            }
        }

        // Finalize UI
        gameManager.InitializeGameManager();
        lobbyManager.UpdateLobbyUI();
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    private async Task SendLobbyHeartbeat(string lobbyId)
    {
        while (true)
        {
            await Task.Delay(15000);
            try
            {
                await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                Debug.Log("💓 Sent lobby heartbeat ping");
            }
            catch
            {
                Debug.LogWarning("Heartbeat failed or lobby expired.");
                break;
            }
        }

    }








}
