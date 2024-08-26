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
    private bool isActivated = false;
    public bool mainDoorSwitch = false;
    public bool mainDoorToggle = false;
    public ulong playerID;
    private bool playerInRange = false;
    private PlayerController playerController;
    public MainGAte mainGate; // Reference to MainGate

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

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
            Debug.Log("Player " + playerController.OwnerClientId + " is activating the switch.");
            ActivateObjectServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivateObjectServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (!isActivated && (!mainDoorSwitch || clientId == playerID))
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
            Debug.Log("Main door switch activated: " + gameObject.name);
            if (mainGate != null)
            {
                mainGate.Interact();
            }
        }
    }

}
