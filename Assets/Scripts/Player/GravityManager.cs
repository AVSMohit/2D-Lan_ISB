using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GravityManager : MonoBehaviourPun
{
    public bool enableGravity = true;
    [SerializeField] private PlayerController[] players;

    private void Start()
    {
        if (PhotonNetwork.IsConnected || PhotonNetwork.OfflineMode)
        {
            UpdatePlayerGravity();
        }
        else
        {
            Debug.LogWarning("Photon is not connected. Gravity changes will not be synced.");
        }
    }

    public void UpdatePlayerGravity()
    {
        players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.SetGravity(enableGravity);
        }

        // Only send the RPC if connected to Photon
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncGravityState", RpcTarget.Others, enableGravity);
        }
    }

    [PunRPC]
    private void SyncGravityState(bool gravityEnabled)
    {
        enableGravity = gravityEnabled;
        players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.SetGravity(enableGravity);
        }
    }
}
