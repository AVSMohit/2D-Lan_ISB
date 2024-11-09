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
        UpdatePlayerGravity();
    }

    public void UpdatePlayerGravity()
    {
        players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.SetGravity(enableGravity);
        }

        // Broadcast gravity state to all clients
        photonView.RPC("SyncGravityState", RpcTarget.Others, enableGravity);
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
