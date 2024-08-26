using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner)
        {
            StartCoroutine(WaitForSceneLoadAndPosition());
        }
    }

    private IEnumerator WaitForSceneLoadAndPosition()
    {
        // Wait until the scene has fully loaded and the SpawnManager is initialized
        yield return new WaitUntil(() => FindObjectOfType<SpawnManager>() != null);

        // Get the SpawnManager from the scene
        var spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene!");
            yield break;
        }

        // Assign the player to the correct spawn point
        Transform spawnPoint = spawnManager.GetSpawnPointForPlayer(NetworkManager.LocalClientId);

        if (spawnPoint != null)
        {
            // Move the player to the assigned spawn point
            transform.position = spawnPoint.position;
            Debug.Log($"Player {NetworkManager.LocalClientId} moved to spawn point {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning($"No spawn point found for player {NetworkManager.LocalClientId}");
        }
    }
}

