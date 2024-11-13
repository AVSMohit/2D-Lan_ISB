using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    public Animator doorAnimator;

    [PunRPC]
    public void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void CloseDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
