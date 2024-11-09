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
        if (!playerSpawnPoints.ContainsKey(actorNumber))
        {
            // Assign a new spawn point based on the available points
            int index = actorNumber % spawnPoints.Length;
            playerSpawnPoints[actorNumber] = spawnPoints[index];
        }

        return playerSpawnPoints[actorNumber];
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
