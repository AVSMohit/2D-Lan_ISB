using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class SpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints; // Array of spawn points in the scene
    private Dictionary<ulong, Transform> playerSpawnPoints = new Dictionary<ulong, Transform>();

    private void Start()
    {
        // Ensure this script is executed after all players have joined and the scene has loaded
        if (IsServer)
        {
            AssignSpawnPoints();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }
    private void OnClientConnected(ulong clientId)
    {
        // Assign a spawn point to the new client
        MovePlayerToSpawnPoint(clientId);
    }

    // Assign spawn points to all connected players
    public void AssignSpawnPoints()
    {
        ulong[] clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();

        for (int i = 0; i < clientIds.Length && i < spawnPoints.Length; i++)
        {
            playerSpawnPoints[clientIds[i]] = spawnPoints[i];
            MovePlayerToSpawnPoint(clientIds[i]);
        }
    }
    public Transform GetSpawnPointForPlayer(ulong clientId)
    {
        if (!playerSpawnPoints.ContainsKey(clientId))
        {
            // Assign a new spawn point based on the available points
            int index = (int)(clientId % (ulong)spawnPoints.Length);
            playerSpawnPoints[clientId] = spawnPoints[index];
        }

        return playerSpawnPoints[clientId];
    }

    // Move the player to the assigned spawn point
    private void MovePlayerToSpawnPoint(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                Transform spawnPoint = playerSpawnPoints[clientId];
                client.PlayerObject.transform.position = spawnPoint.position;
                Debug.Log($"Player {clientId} moved to spawn point {spawnPoint.position}");
            }
            else
            {
                Debug.LogWarning($"Player object for client {clientId} not found!");
            }
        }
    }
}
