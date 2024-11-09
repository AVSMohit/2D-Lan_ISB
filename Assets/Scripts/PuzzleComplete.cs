using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PuzzleComplete : MonoBehaviourPun
{
    private HashSet<int> playersInTrigger = new HashSet<int>();
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                playersInTrigger.Add(playerPhotonView.OwnerActorNr);
                CheckAllPlayersInTrigger();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                playersInTrigger.Remove(playerPhotonView.OwnerActorNr);
            }
        }
    }

    void CheckAllPlayersInTrigger()
    {
        if (playersInTrigger.Count == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("TriggerSceneTransition", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void TriggerSceneTransition()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(nextSceneName);
        }
    }
}
