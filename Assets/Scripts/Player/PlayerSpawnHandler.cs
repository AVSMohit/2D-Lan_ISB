using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : NetworkBehaviour
{
    private void Start()
    {
        // Request a spawn point from the server when the player spawns
        if (IsOwner)
        {
          //  RequestSpawnPointServerRpc(NetworkManager.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPointServerRpc(ulong clientId)
    {
        Debug.Log($"Requesting spawn point for client {clientId}");
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            Transform spawnPoint = spawnManager.GetSpawnPointForPlayer(clientId);
            if (spawnPoint != null)
            {
                AssignPlayerToSpawnClientRpc(spawnPoint.position, clientId);
            }
            else
            {
                Debug.LogError("No available spawn points!");
            }
        }
    }

    [ClientRpc]
    private void AssignPlayerToSpawnClientRpc(Vector3 spawnPosition, ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Assigning spawn point for client {clientId}");
            transform.position = spawnPosition;
            Debug.Log($"Player {clientId} moved to spawn point at {spawnPosition}");
        }
    }
}

