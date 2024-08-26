using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public CameraController cameraController; // Reference to the CameraController script

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

    public void TransitionToScene(string sceneName)
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load the new scene
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Once the scene is loaded, assign players to spawn points
        PlacePlayersAtSpawnPoints();

        // Unsubscribe from the scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void PlacePlayersAtSpawnPoints()
    {
        var spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene!");
            return;
        }

        // For each player, move them to their spawn point
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            Transform spawnPoint = spawnManager.GetSpawnPointForPlayer(clientId);

            if (spawnPoint != null)
            {
                var playerObject = client.Value.PlayerObject;
                playerObject.transform.position = spawnPoint.position;
                Debug.Log($"Player {clientId} moved to spawn point {spawnPoint.position}");
            }
            else
            {
                Debug.LogWarning($"No spawn point found for player {clientId}");
            }
        }
    }
}
