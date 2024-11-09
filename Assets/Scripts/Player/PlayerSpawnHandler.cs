using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawnHandler : MonoBehaviourPun
{
    private void Start()
    {
        // Request a spawn point for the local player when the object is owned by them
        if (photonView.IsMine)
        {
            RequestSpawnPoint();
        }
    }

    private void RequestSpawnPoint()
    {
        Debug.Log($"Requesting spawn point for player {PhotonNetwork.LocalPlayer.ActorNumber}");
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            Transform spawnPoint = spawnManager.GetSpawnPointForPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
            if (spawnPoint != null)
            {
                photonView.RPC("AssignPlayerToSpawn", RpcTarget.All, spawnPoint.position, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                Debug.LogError("No available spawn points!");
            }
        }
    }

    [PunRPC]
    private void AssignPlayerToSpawn(Vector3 spawnPosition, int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            Debug.Log($"Assigning spawn point for player {actorNumber}");
            transform.position = spawnPosition;
            Debug.Log($"Player {actorNumber} moved to spawn point at {spawnPosition}");
        }
    }
}
