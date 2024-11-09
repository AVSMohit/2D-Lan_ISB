using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Doors : InteractableClass, IPunObservable
{
    [SerializeField] Vector3 rotation;

    private PhotonView photonView; // Add this if not using inheritance

    private void Awake()
    {
        // Ensure PhotonView is attached to the GameObject
        photonView = GetComponent<PhotonView>();
    }

    public override void Interact()
    {
        base.Interact();
        if (photonView != null && photonView.IsMine)
        {
            photonView.RPC("RotateDoor", RpcTarget.AllBuffered, rotation.x, rotation.y, rotation.z);
        }
    }

    [PunRPC]
    private void RotateDoor(float x, float y, float z)
    {
        transform.rotation = Quaternion.Euler(x, y, z);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.rotation.eulerAngles);
        }
        else
        {
            Vector3 receivedRotation = (Vector3)stream.ReceiveNext();
            transform.rotation = Quaternion.Euler(receivedRotation);
        }
    }
}
