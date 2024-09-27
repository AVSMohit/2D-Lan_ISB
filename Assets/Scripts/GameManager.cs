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
        Debug.Log($"Client {clientId} connected.");

        // Only spawn the player if a scene is not currently loading
        if (!SceneTransitionManager.Instance.isSceneLoading)
        {
            Debug.Log($"Spawning player for client {clientId}");
           // SpawnPlayer(clientId);  // Spawn only on first connection, not during scene transitions
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
           // isSceneLoading = false;

            // Respawn players after the new scene loads
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) // Ensure players are only respawned if they don't already exist
                {
                    RespawnPlayer(client.ClientId);
                }
            }

            // Unsubscribe from the scene loaded event to prevent multiple respawns
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
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawnPosition = spawnManager.GetSpawnPointForPlayer(clientId).position;
            var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            // Ensure the player is spawned as a networked object
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log($"Respawned player {clientId} at {spawnPosition}");
        }
        else
        {
            Debug.LogError("SpawnManager not found in the new scene!");
        }
    }
}
