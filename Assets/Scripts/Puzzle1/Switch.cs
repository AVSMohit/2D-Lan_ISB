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

    public ulong playerID;    

    PlayerController playerController;
    // Start is called before the first frame update
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController controller = collision.GetComponent<PlayerController>();
            if (controller != null && (!mainDoorSwitch || controller.OwnerClientId == playerID))
            {
                playerController = controller;
                controller.interactText.gameObject.SetActive(true);
                playerInRange = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController controller = collision.GetComponent<PlayerController>();
            if (controller == playerController)
            {
                controller.interactText.gameObject.SetActive(false);
                playerInRange = false;
                playerController = null;
            }
        }
    }

    private void Update()
    {
        if (playerInRange && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            ActivateObjectServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivateObjectServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (!isActivated && (!mainDoorSwitch || clientId == playerID)) // Validate the correct client
        {
            isActivated = true;
            ActivateObjectsClientRpc();
        }
    }

    [ClientRpc]
    void ActivateObjectsClientRpc()
    {
        spriteRenderer.color = activatedColor;
        foreach (InteractableClass interactable in objectsToBeInteracted)
        {
            interactable.Interact();
        }

        if (mainDoorSwitch)
        {
            mainDoorToggle = true;
        }
    }

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

}
