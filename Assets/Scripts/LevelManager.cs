using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public CameraController cameraController;
    public Tilemap tilemap; // Reference to the Tilemap component

    void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null && tilemap != null)
        {
            cameraController.SetMapBoundaries(tilemap);
        }
    }
}
