using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab; // Assign this in the Inspector
    public TMP_Text clientStatusText; // Assign this in the Inspector

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            if (clientStatusText != null)
            {
                clientStatusText.text = "Waiting for clients...";
            }
        }
       // Logger.Log("GameManager started.");
    }

    private void OnClientConnected(ulong clientId)
    {
       //  Logger.Log($"Client connected with ID: {clientId}");
       // Debug.Log($"Client connected with ID: {clientId}");
        if (clientStatusText != null)
        {
            clientStatusText.text = $"Client connected: {clientId}";
        }
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        var spawnPosition = Vector3.zero; // Set your spawn position
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
