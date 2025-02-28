using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab; // Assign this in the Inspector
    public TMP_Text clientStatusText; // Assign this in the Inspector


    private void Start()
    {
      
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            if (clientStatusText != null)
            {
                clientStatusText.text = "Waiting for clients...";
            }
        }
        // Logger.Log("GameManager started.");
    }

    public void OnClientConnected(ulong clientId)
    {
        // If this client is the host and is acting as the room creator, skip spawning.
        if (clientId == NetworkManager.Singleton.LocalClientId && RoomSettings.IsRoomCreator)
        {
            Debug.Log($"Client {clientId} is the room creator; no player spawned.");
            return;
        }

        Debug.Log($"Client {clientId} connected.");

        // If a scene is not currently loading, spawn the player for this client.
        if (!SceneTransitionManager.Instance.isSceneLoading)
        {
            Debug.Log($"Spawning player for client {clientId}");
            SpawnPlayer(clientId);
        }
    }



    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawnPosition = spawnManager.GetSpawnPointForPlayer(clientId).position;
            var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log($"Player {clientId} spawned at {spawnPosition}");
        }
        else
        {
            Debug.LogError("SpawnManager not found!");
        }
    }

    //private void OnEnable()
    //{
    //    NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneChanged;
    //}

    public void InitializeGameManager()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            // Only subscribe to scene events if this is the server
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneChanged;
                Debug.Log("GameManager initialized and subscribed to scene events.");
            }
        }
        else
        {
            Debug.LogError("NetworkManager or SceneManager is not initialized.");
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneChanged;
        }
    }

    public void OnSceneChanged(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Scene loaded, respawning players.");

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Skip spawning if the player object is already present...
                if (client.PlayerObject == null)
                {
                    // Skip for the host if they are the room creator.
                    if (client.ClientId == NetworkManager.Singleton.LocalClientId && RoomSettings.IsRoomCreator)
                    {
                        Debug.Log("Host is room creator; skipping respawn.");
                        continue;
                    }
                    RespawnPlayer(client.ClientId);
                }
            }

            // Unsubscribe from the scene loaded event to prevent multiple respawns.
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneChanged;
        }
    }


    private void ReassignPlayersToSpawnPoints()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId); // Re-spawn the player at the correct spawn point
        }
    }
    public void RespawnPlayer(ulong clientId)
    {
        // Skip respawn for the host acting as room creator.
        if (clientId == NetworkManager.Singleton.LocalClientId && RoomSettings.IsRoomCreator)
        {
            Debug.Log("Host is room creator; no respawn needed.");
            return;
        }

        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawnPosition = spawnManager.GetSpawnPointForPlayer(clientId).position;
            var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log($"Respawned player {clientId} at {spawnPosition}");
        }
        else
        {
            Debug.LogError("SpawnManager not found in the new scene!");
        }
    }

}
