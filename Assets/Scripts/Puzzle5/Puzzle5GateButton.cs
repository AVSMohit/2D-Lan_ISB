using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Puzzle5GateButton : MonoBehaviourPun
{
    private int activePadsCount = 0;

    public DoorController door;
    public SpriteRenderer padRenderer;
    public Color inactiveColor;
    public Color activeColor;
    public Light2D padLight;

    private bool isActivated = false;

    private void Start()
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
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            photonView.RPC("UpdateVisuals", RpcTarget.All, true);

            if (PhotonNetwork.IsMasterClient)
            {
                activePadsCount++;
                CheckDoorState();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isActivated)
        {
            isActivated = false;
            photonView.RPC("UpdateVisuals", RpcTarget.All, false);

            if (PhotonNetwork.IsMasterClient)
            {
                activePadsCount--;
                CheckDoorState();
            }
        }
    }

    [PunRPC]
    private void UpdateVisuals(bool activated)
    {
        if (padRenderer != null)
        {
            padRenderer.color = activated ? activeColor : inactiveColor;
        }

        if (padLight != null)
        {
            padLight.enabled = activated;
        }
    }

    private void CheckDoorState()
    {
        if (activePadsCount == 3)
        {
            photonView.RPC("OpenDoor", RpcTarget.All);
        }
        else
        {
            photonView.RPC("CloseDoor", RpcTarget.All);
        }
    }

    [PunRPC]
    private void OpenDoor()
    {
        door.OpenDoor();
    }

    [PunRPC]
    private void CloseDoor()
    {
        door.CloseDoor();
    }
}
