using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainGAte : InteractableClass
{
    public Switch[] switchsForMainGate;
    public SpriteRenderer spriteRenderer;
    private bool allTogglesOn;
    private void Start()
    {
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        allTogglesOn = false;
        
    }

    public void CheckToggle()
    {
        allTogglesOn = true;
        foreach (Switch _switch in switchsForMainGate)
        {
            if (!_switch.mainDoorToggle)
            {
                allTogglesOn = false;
                spriteRenderer.color = Color.red;
                Debug.Log("Switch " + _switch.gameObject.name + " is not activated.");
                return;
            }
        }

        if (allTogglesOn)
        {
            spriteRenderer.color = Color.green;
            Debug.Log("All switches are activated. Opening the gate.");
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    public override void Interact()
    {
        CheckToggle();
    }
}
