using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab; // Assign this in the Inspector
    public TMP_Text clientStatusText; // Assign this in the Inspector

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (clientStatusText != null)
            {
                clientStatusText.text = "Waiting for clients...";
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} connected.");
        if (PhotonNetwork.IsMasterClient && !SceneTransitionManager.Instance.isSceneLoading)
        {
            Debug.Log($"Spawning player for player {newPlayer.ActorNumber}");
            SpawnPlayer(newPlayer.ActorNumber);
        }
    }

    private void SpawnPlayer(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawnPosition = spawnManager.GetSpawnPointForPlayer(actorNumber).position;
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
            Debug.Log($"Player {actorNumber} spawned at {spawnPosition}");
        }
        else
        {
            Debug.LogError("SpawnManager not found!");
        }
    }

    public void InitializeGameManager()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("GameManager initialized and ready.");
        }
        else
        {
            Debug.LogError("This client is not the Master Client. Initialization skipped.");
        }
    }

    private void OnDisable()
    {
        // Clean up any scene-specific logic if necessary
        Debug.Log("GameManager disabled.");
    }

    public void RespawnPlayer(int actorNumber)
    {
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawnPosition = spawnManager.GetSpawnPointForPlayer(actorNumber).position;
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
            Debug.Log($"Respawned player {actorNumber} at {spawnPosition}");
        }
        else
        {
            Debug.LogError("SpawnManager not found in the new scene!");
        }
    }
}
