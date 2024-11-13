using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // Array of spawn points in the scene
    private Dictionary<int, Transform> playerSpawnPoints = new Dictionary<int, Transform>();

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AssignSpawnPoints();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Assign a spawn point when a new player joins the room
        MovePlayerToSpawnPoint(newPlayer.ActorNumber);
    }

    // Assign spawn points to all connected players
    public void AssignSpawnPoints()
    {
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length && i < spawnPoints.Length; i++)
        {
            playerSpawnPoints[players[i].ActorNumber] = spawnPoints[i];
            MovePlayerToSpawnPoint(players[i].ActorNumber);
        }
    }

    public Transform GetSpawnPointForPlayer(int actorNumber)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points array is empty or null!");
            return null;
        }

        // Ensure the index is within the bounds of spawnPoints array
        int index = actorNumber % spawnPoints.Length;
        if (index >= 0 && index < spawnPoints.Length)
        {
            return spawnPoints[index];
        }
        else
        {
            Debug.LogError($"Index {index} is out of range for spawn points array.");
            return spawnPoints[0]; // Fallback to the first spawn point if out of range
        }
    }
    // Move the player to the assigned spawn point
    private void MovePlayerToSpawnPoint(int actorNumber)
    {
        GameObject playerObject = PhotonView.Find(actorNumber)?.gameObject;
        if (playerObject != null)
        {
            Transform spawnPoint = playerSpawnPoints[actorNumber];
            playerObject.transform.position = spawnPoint.position;
            Debug.Log($"Player {actorNumber} moved to spawn point {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning($"Player object for actor number {actorNumber} not found!");
        }
    }
}
