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
            GetComponentInChildren<SpriteRenderer>().color = activeColor;
            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<PuzzleComplete>().enabled = true;
            // Logic to unlock the next area or perform an action
            Debug.Log("Target hit by light beam!");
            // Add additional logic here, e.g., opening a door, triggering an event, etc.
        }
    }

    public void ResetTarget()
    {
        isHit = false;
        Debug.Log("Target reset.");
        GetComponentInChildren<SpriteRenderer>().color = defaultColor;
        GetComponent<BoxCollider2D>().isTrigger = false;
        GetComponent<PuzzleComplete>().enabled = false;
        // Logic to reset the target if needed
    }
}
