using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TargetToHit : NetworkBehaviour
{
    private bool isHit = false;
     public Color defaultColor;
     public Color activeColor;
    public void OnHit()
    {
        if (!isHit)
        {
            isHit = true;
            UpdateTargetClientRpc();  // Sync the hit event with all clients

            // Logic specific to the host, like enabling puzzle completion
            GetComponent<PuzzleComplete>().enabled = true;
            Debug.Log("Target hit by light beam!");
        }
    }
    [ClientRpc]
    private void UpdateTargetClientRpc()
    {
        GetComponentInChildren<SpriteRenderer>().color = activeColor;
        GetComponent<BoxCollider2D>().isTrigger = true;
        Debug.Log("Target hit on client!");
    }
    public void ResetTarget()
    {
        isHit = false;
        ResetTargetClientRpc();  // Sync the reset event with all clients
        Debug.Log("Target reset.");
    }

    [ClientRpc]
    private void ResetTargetClientRpc()
    {
        GetComponentInChildren<SpriteRenderer>().color = defaultColor;
        GetComponent<BoxCollider2D>().isTrigger = false;
    }
}
