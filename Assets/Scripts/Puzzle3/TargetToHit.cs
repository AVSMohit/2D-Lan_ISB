using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetToHit : MonoBehaviourPun
{
    private bool isHit = false;
    public Color defaultColor;
    public Color activeColor;

    public void OnHit()
    {
        if (!isHit)
        {
            isHit = true;
            photonView.RPC("UpdateTargetRPC", RpcTarget.AllBuffered); // Sync the hit event with all clients

            // Logic specific to the host, like enabling puzzle completion
            GetComponent<PuzzleComplete>().enabled = true;
            Debug.Log("Target hit by light beam!");
        }
    }

    [PunRPC]
    private void UpdateTargetRPC()
    {
        GetComponentInChildren<SpriteRenderer>().color = activeColor;
        GetComponent<BoxCollider2D>().isTrigger = true;
        Debug.Log("Target hit on client!");
    }

    public void ResetTarget()
    {
        isHit = false;
        photonView.RPC("ResetTargetRPC", RpcTarget.AllBuffered); // Sync the reset event with all clients
        Debug.Log("Target reset.");
    }

    [PunRPC]
    private void ResetTargetRPC()
    {
        GetComponentInChildren<SpriteRenderer>().color = defaultColor;
        GetComponent<BoxCollider2D>().isTrigger = false;
    }
}
