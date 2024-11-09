using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using UnityEngine;

public class MainGAte : InteractableClass, IPunObservable
{
    public Switch[] switchsForMainGate;
    public SpriteRenderer spriteRenderer;
    private bool allTogglesOn;

    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        allTogglesOn = false;
    }

    public void CheckToggle()
    {
        if (!photonView.IsMine) return;

        allTogglesOn = true;
        foreach (Switch _switch in switchsForMainGate)
        {
            if (!_switch.mainDoorToggle)
            {
                allTogglesOn = false;
                photonView.RPC("UpdateGateState", RpcTarget.AllBuffered, false);
                Debug.Log("Switch " + _switch.gameObject.name + " is not activated.");
                return;
            }
        }

        if (allTogglesOn)
        {
            photonView.RPC("UpdateGateState", RpcTarget.AllBuffered, true);
            Debug.Log("All switches are activated. Opening the gate.");
        }
    }

    [PunRPC]
    private void UpdateGateState(bool isActive)
    {
        if (isActive)
        {
            spriteRenderer.color = Color.green;
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        }
        else
        {
            spriteRenderer.color = Color.red;
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    public override void Interact()
    {
        CheckToggle();
    }

    // IPunObservable implementation for custom synchronization (optional)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(spriteRenderer.color);
            stream.SendNext(this.gameObject.GetComponent<BoxCollider2D>().isTrigger);
        }
        else
        {
            spriteRenderer.color = (Color)stream.ReceiveNext();
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = (bool)stream.ReceiveNext();
        }
    }
}
