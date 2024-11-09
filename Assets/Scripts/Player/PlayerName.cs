using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerName : MonoBehaviourPun, IPunObservable
{
    public TMP_Text playerNameText; // TextMeshPro Text to display the player name
    private string playerName;

    private void Start()
    {
        if (photonView.IsMine)
        {
            // Get the player's name from PlayerPrefs
            playerName = PlayerPrefs.GetString("PlayerName", $"Player {PhotonNetwork.LocalPlayer.ActorNumber}");

            // Set the player's name for the Photon Player
            PhotonNetwork.LocalPlayer.NickName = playerName;

            // Sync the name across all clients
            photonView.RPC("UpdatePlayerName", RpcTarget.All, playerName);
        }

        // Display the player name at start
        playerNameText.text = playerName;
    }

    [PunRPC]
    private void UpdatePlayerName(string name)
    {
        playerName = name;
        playerNameText.text = name;
    }

    // IPunObservable interface implementation for custom synchronization (optional)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the player name to other clients
            stream.SendNext(playerName);
        }
        else
        {
            // Receive the player name from other clients
            playerName = (string)stream.ReceiveNext();
            playerNameText.text = playerName;
        }
    }
}
