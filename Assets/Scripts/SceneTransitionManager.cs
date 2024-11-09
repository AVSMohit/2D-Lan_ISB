using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviourPunCallbacks
{
    public static SceneTransitionManager Instance;
    public bool isSceneLoading = false;
    private GameManager gameManager;

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

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void TransitionToScene(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)  // Ensure only the master client handles scene transition
        {
            DestroyAllPlayers();
            isSceneLoading = true;

            // Load the scene for all clients
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    public override void OnJoinedRoom()
    {
        // Place players at their spawn points when they join a new room or after a scene transition
        PlacePlayersAtSpawnPoints();
    }

    private void PlacePlayersAtSpawnPoints()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene!");
            return;
        }

        // Ensure all players are moved to their spawn points
        spawnManager.AssignSpawnPoints();
    }

    private void DestroyAllPlayers()
    {
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView photonView = playerObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                PhotonNetwork.Destroy(playerObject);  // Destroy the player object
                Debug.Log($"Destroyed player with ID {photonView.OwnerActorNr}");
            }
        }
    }
}
