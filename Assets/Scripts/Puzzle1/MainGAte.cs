using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainGAte : InteractableClass
{
    public Switch[] switchsForMainGate;

    bool allTogglesOn;
    public override void Interact()
    {
        base.Interact();
        for(int i = 0; i < switchsForMainGate.Length; i++)
        {
            
                if (switchsForMainGate[i].mainDoorToggle == true)
                {
                    allTogglesOn = true;
                }
                
        }
    }

    private void Update()
    {
        if (allTogglesOn)
        { 
            this.gameObject.SetActive(false);
        }
    }
}
