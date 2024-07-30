using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class Switch : NetworkBehaviour
{

    public InteractableClass[] objectsToBeInteracted;
    public SpriteRenderer spriteRenderer;
    public Color activatedColor = Color.green;
    bool isActivated = false;
    public bool mainDoorSwitch = false;
    public bool mainDoorToggle = false;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
       

            if(collision.CompareTag("Player") )
            {
                PlayerController controller= collision.GetComponent<PlayerController>();

                controller.interactText.gameObject.SetActive(true);
                if (!isActivated)
                {               
                        isActivated = true;
                                 
                }

            }

        
        
    }

    private void Update()
    {
        if (isActivated)
        {
           
                if (Input.GetKeyDown(KeyCode.E))
                {
                    

                    ActivateObjectServerRpc();
                }
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController controller = collision.GetComponent<PlayerController>();

            controller.interactText.gameObject.SetActive(false);
             isActivated = false;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivateObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        ActivateObjectClientRpc();
    }

    [ClientRpc]
    void ActivateObjectClientRpc()
    {
        spriteRenderer.color = activatedColor;
        if (!mainDoorSwitch)
        { 
            foreach(var interactable in objectsToBeInteracted)
            {
                interactable.Interact();
            }
        }
        else
        {
            foreach (var interactable in objectsToBeInteracted)
            {
                interactable.Interact();
            }
            mainDoorToggle = true;
        }
    }

    
}
