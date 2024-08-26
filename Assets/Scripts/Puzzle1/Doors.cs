using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : InteractableClass
{
   [SerializeField] Vector3 rotation;
    public override void Interact()
    {
        base.Interact();
        transform.rotation = Quaternion.Euler(rotation.x,rotation.y,rotation.z);
    }
}
