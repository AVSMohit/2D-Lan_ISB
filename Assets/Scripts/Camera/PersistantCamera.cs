using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PersistantCamera : MonoBehaviourPun
{
    private static PersistantCamera instance;

    void Awake()
    {
        // Check if the current instance is a remote instance in Photon PUN
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If the instance is not mine or already exists, destroy this duplicate
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
