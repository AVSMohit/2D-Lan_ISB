using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints; // Array of spawn points
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private HashSet<int> occupiedSpawnPoints = new HashSet<int>(); // Track indices of occupied spawn points

    private void Start()
    {
        if (IsServer)
        {
            // Initialize the available spawn points list
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    availableSpawnPoints.Add(spawnPoints[i]);
                    Debug.Log($"Spawn point added: {spawnPoints[i].position}");
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnPointServerRpc(ServerRpcParams rpcParams = default)
    {
        Transform spawnPoint = GetSpawnPoint();
        if (spawnPoint != null)
        {
            int index = System.Array.IndexOf(spawnPoints, spawnPoint);
            occupiedSpawnPoints.Add(index);
            AssignSpawnPointClientRpc(index, rpcParams.Receive.SenderClientId);
        }
        else
        {
            Debug.LogWarning("No available spawn points!");
        }
    }

    private Transform GetSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No available spawn points!");
            return null;
        }

        // Get the first available spawn point
        Transform spawnPoint = availableSpawnPoints[0];
        availableSpawnPoints.RemoveAt(0);

        Debug.Log($"Assigned spawn point: {spawnPoint.position}");
        return spawnPoint;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseSpawnPointServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (occupiedSpawnPoints.Contains(index))
        {
            occupiedSpawnPoints.Remove(index);
            availableSpawnPoints.Add(spawnPoints[index]);
            Debug.Log($"Released spawn point: {spawnPoints[index].position}");
        }
    }

    [ClientRpc]
    private void AssignSpawnPointClientRpc(int index, ulong clientId)
    {
        if (index >= 0 && index < spawnPoints.Length)
        {
            Transform spawnPoint = spawnPoints[index];
            Debug.Log($"Client {clientId} assigned to spawn point: {spawnPoint.position}");

            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = spawnPoint.position;
                NotifyNextClientServerRpc(clientId);
            }
        }
        else
        {
            Debug.LogWarning($"Client {clientId} could not be assigned a spawn point!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyNextClientServerRpc(ulong clientId)
    {
        ulong nextClientId = GetNextClientId(clientId);
        if (nextClientId != ulong.MaxValue)
        {
            NotifyClientToRequestSpawnClientRpc(nextClientId);
        }
    }

    private ulong GetNextClientId(ulong currentClientId)
    {
        List<ulong> clientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        int currentIndex = clientIds.IndexOf(currentClientId);
        if (currentIndex + 1 < clientIds.Count)
        {
            return clientIds[currentIndex + 1];
        }
        return ulong.MaxValue;
    }

    [ClientRpc]
    private void NotifyClientToRequestSpawnClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            RequestSpawnPointServerRpc();
        }
    }

    public Transform GetSpawnPointForPlayer(ulong clientId)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (!occupiedSpawnPoints.Contains(System.Array.IndexOf(spawnPoints, spawnPoint)))
            {
                return spawnPoint;
            }
        }
        return null;
    }
}
