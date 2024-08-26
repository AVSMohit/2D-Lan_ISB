using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public CameraController cameraController; // Reference to the CameraController script

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            StartCoroutine(WaitForSceneLoad(sceneName));
        }
    }

    private IEnumerator WaitForSceneLoad(string sceneName)
    {
        while (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return null;
        }

        if (cameraController != null)
        {
            cameraController.UpdatePlayerList();
        }

        RespawnPlayers();
    }

    private void RespawnPlayers()
    {
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                if (playerObject != null)
                {
                    var spawnPoint = spawnManager.GetSpawnPointForPlayer(clientId);
                    if (spawnPoint != null)
                    {
                        playerObject.transform.position = spawnPoint.position;
                    }
                }
            }
        }
    }
}
