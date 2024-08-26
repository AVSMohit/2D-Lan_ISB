using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : NetworkBehaviour
{
    private SpawnManager spawnManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            StartCoroutine(InitializePlayer());
        }
    }

    private IEnumerator InitializePlayer()
    {
        // Wait until the game scene is fully loaded
        while (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null; // Wait for the next frame
        }

        // Wait until the SpawnManager is found
        while (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager == null)
            { 
                Debug.LogError("SpawnManager not found in game scene!");
                yield return null; // Wait for the next frame
            }
        }

        // Request a spawn point from the server
        if (NetworkManager.Singleton.IsHost)
        {
            spawnManager.RequestSpawnPointServerRpc();
        }
    }

    private void OnDestroy()
    {
        if (IsOwner && spawnManager != null)
        {
            int index = System.Array.IndexOf(spawnManager.spawnPoints, transform);
            if (index >= 0)
            {
                spawnManager.ReleaseSpawnPointServerRpc(index);
            }
        }
    }
}
