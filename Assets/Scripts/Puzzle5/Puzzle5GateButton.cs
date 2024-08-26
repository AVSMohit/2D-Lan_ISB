using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;

public class Puzzle5GateButton : NetworkBehaviour
{
    NetworkVariable<int> activePadsCount = new NetworkVariable<int>(0);

    public DoorController door;
    public SpriteRenderer padRenderer;
    public Color inactiveColor;
    public Color activeColor;
    public Light2D padLight;

    bool isActivated = false;
    // Start is called before the first frame update
    void Start()
    {
        padRenderer = GetComponent<SpriteRenderer>();
        if (padRenderer != null) 
        {
            padRenderer.color = inactiveColor;
        }

        if (padLight != null) 
        {
            padLight.enabled = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            UpdateVisuals(true);

            if (IsServer)
            {
                activePadsCount.Value++;
                CheckDoorStateServerRpc();
                
            }
            else
            {
                ActivatePadServerRpc();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isActivated)
        {
            isActivated = false;
            UpdateVisuals(false);  // Update the visual feedback on the pad

            if (IsServer)
            {
                activePadsCount.Value--;  // Only the server updates this value
                CheckDoorStateServerRpc();  // Server checks if the door should open/close
            }
            else
            {
                // Client requests the server to update the state
                DeactivatePadServerRpc();
            }
        }
    }
    // Update is called once per frame
    void UpdateVisuals(bool activated)
    {
        if (padRenderer != null)
        {
            padRenderer.color = activated ? activeColor : inactiveColor;
        }

        // Optionally, toggle a light effect
        if (padLight != null)
        {
            padLight.enabled = activated;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivatePadServerRpc()
    {
        activePadsCount.Value++;
        CheckDoorStateServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DeactivatePadServerRpc()
    {
        activePadsCount.Value--;
        CheckDoorStateServerRpc();
    }

    [ServerRpc]
    void CheckDoorStateServerRpc()
    {
        if (activePadsCount.Value == 3)
        {
            OpenDoorClientRpc();  // Open the door on all clients
        }
        else
        {
            CloseDoorClientRpc();  // Close the door on all clients

        }
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        door.OpenDoor();  // Open the door for all players
    }

    [ClientRpc]
    private void CloseDoorClientRpc()
    {
        door.CloseDoor();  // Close the door for all players
    }
}
