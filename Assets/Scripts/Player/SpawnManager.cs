using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
    public Transform[] spawnPoints; // Array of spawn points in the scene

    private Dictionary<ulong, Transform> playerSpawnPoints = new Dictionary<ulong, Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Transform GetSpawnPointForPlayer(ulong clientId)
    {
        // Assign a unique spawn point to each player
        if (!playerSpawnPoints.ContainsKey(clientId))
        {
            int index = (int)(clientId % (ulong)spawnPoints.Length); // Distribute players among the spawn points
            playerSpawnPoints[clientId] = spawnPoints[index];
        }

        return playerSpawnPoints[clientId];
    }
}
