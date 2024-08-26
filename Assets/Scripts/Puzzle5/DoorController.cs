using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator doorAnimator;  // Optional: use an animator for the door

    public void OpenDoor()
    {
        // Open the door with an animation or deactivate the door object
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            gameObject.SetActive(false);  // Disable the door object to "open" it
        }
    }

    public void CloseDoor()
    {
        // Close the door with an animation or activate the door object
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }
        else
        {
            gameObject.SetActive(true);  // Enable the door object to "close" it
        }
    }
}
