using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using Unity.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public bool isSceneLoading = false;
    GameManager gameManager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }
    public void TransitionToScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsServer)  // Ensure only the server handles scene transition
        {
            DestroyAllPlayers();
            // Set the flag to true to track that a scene is loading
            isSceneLoading = true;

            // Destroy all players before changing the scene

            // Subscribe to the scene event to respawn players after the new scene is loaded
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneLoaded;

            // Load the new scene across all clients
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void OnSceneLoaded(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType == SceneEventType.LoadComplete && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Scene loaded, respawning players.");

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Ensurei that players are respawned if they do not already exist
                if (client.PlayerObject == null)
                {
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.RespawnPlayer(client.ClientId);
                    }
                    else
                    {
                        Debug.LogError("GameManager not found to respawn players!");
                    }
                }
            }

            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneLoaded;
        }
    }

    private void PlacePlayersAtSpawnPoints()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene!");
            return;
        }

        // Ensure all players are moved to their spawn points
        spawnManager.AssignSpawnPoints();
    }
    private void DestroyAllPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Despawn(true);  // Despawn and destroy the player object
                    Debug.Log($"Destroyed player for client {client.ClientId}");
                }
            }
        }
    }
}
