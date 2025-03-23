using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections;

public class SpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints; // Array of spawn points in the scene
    private Dictionary<ulong, Transform> playerSpawnPoints = new Dictionary<ulong, Transform>();

    private void OnEnable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            StartCoroutine(RepositionAllPlayersAfterSceneLoad());
        }
    }

    private IEnumerator RepositionAllPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.25f); // Let NGO finish spawning

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = kvp.Key;
            var player = kvp.Value.PlayerObject;

            if (player != null)
            {
                if (!playerSpawnPoints.ContainsKey(clientId))
                {
                    int index = (int)(clientId % (ulong)spawnPoints.Length);
                    playerSpawnPoints[clientId] = spawnPoints[index];
                }

                player.transform.position = playerSpawnPoints[clientId].position;
                
                Debug.Log($"✅ Server moved Player {clientId} to spawn {playerSpawnPoints[clientId].position}");
            }
            else
            {
                Debug.LogWarning($"❌ PlayerObject null for client {clientId} after scene load");
            }
        }
    }


    private void Start()
    {
        // Ensure this script is executed after all players have joined and the scene has loaded
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
           
          //  AssignSpawnPoints();

        }
    }
    private void OnClientConnected(ulong clientId)
    {
       // AssignSpawnPoints();
        // Assign a spawn point to the new client
        MovePlayerToSpawnPoint(clientId);
    }

    // Assign spawn points to all connected players
    public void AssignSpawnPoints()
    {
        ulong[] clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();

        for (int i = 0; i < clientIds.Length && i < spawnPoints.Length; i++)
        {
            AssignSpawnPoint(clientIds[i]);
            MovePlayerToSpawnPoint(clientIds[i]);
        }
    }

    private void AssignSpawnPoint(ulong clientId)
    {
        if (!playerSpawnPoints.ContainsKey(clientId))
        {
            int index = (int)(clientId % (ulong)spawnPoints.Length);
            playerSpawnPoints[clientId] = spawnPoints[index];
            Debug.Log($"Spawn point assigned to client {clientId}: index {index}");
        }
    }
    public Transform GetSpawnPointForPlayer(ulong clientId)
    {
        if (!playerSpawnPoints.ContainsKey(clientId))
        {
            AssignSpawnPoint(clientId);
        }

        return playerSpawnPoints[clientId];
    }

    // Move the player to the assigned spawn point
    private void MovePlayerToSpawnPoint(ulong clientId)
    {
        StartCoroutine(WaitAndMove(clientId));
    }

    private IEnumerator WaitAndMove(ulong clientId)
    {
        // Wait a few frames to make sure the player object is available and initialized
        yield return new WaitForSeconds(0.2f);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var player = client.PlayerObject;
            if (player != null)
            {
                Transform spawnPoint = GetSpawnPointForPlayer(clientId);
                player.transform.position = spawnPoint.position;
                Debug.Log($"✅ Moved player {clientId} to spawn point: {spawnPoint.position}");
            }
            else
            {
                Debug.LogWarning($"❌ Player object for client {clientId} not found (after delay)");
            }
        }
    }

}
