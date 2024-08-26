using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public float padding = 2f; // Padding around the players
    public float minCameraSize = 5f; // Minimum camera size
    public float maxCameraSize = 10f; // Maximum camera size
    private Camera cam;
    [SerializeField] private List<Transform> playerTransforms = new List<Transform>();

    // Map boundaries
    private float minX, maxX, minY, maxY;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdatePlayerList();
    }

    void LateUpdate()
    {
        if (playerTransforms.Count == 0)
            return;

        Vector3 centerPoint = GetCenterPoint(playerTransforms);
        Vector3 newPosition = centerPoint;
        newPosition.z = -10f; // Set the camera z position to be behind the players

        // Clamp the camera position within the map boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minX + cam.orthographicSize * cam.aspect, maxX - cam.orthographicSize * cam.aspect);
        newPosition.y = Mathf.Clamp(newPosition.y, minY + cam.orthographicSize, maxY - cam.orthographicSize);

        transform.position = newPosition;
        cam.orthographicSize = Mathf.Clamp(GetGreatestDistance(playerTransforms) / 2f + padding, minCameraSize, maxCameraSize);
    }

    Vector3 GetCenterPoint(List<Transform> players)
    {
        if (players.Count == 1)
        {
            return players[0].position;
        }

        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        return bounds.center;
    }

    float GetGreatestDistance(List<Transform> players)
    {
        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        return bounds.size.x > bounds.size.y ? bounds.size.x : bounds.size.y;
    }

    public void AddPlayer(Transform player)
    {
        if (!playerTransforms.Contains(player))
        {
            playerTransforms.Add(player);
        }
    }

    public void RemovePlayer(Transform player)
    {
        if (playerTransforms.Contains(player))
        {
            playerTransforms.Remove(player);
        }
    }

    public void UpdatePlayerList()
    {
        playerTransforms.Clear();

        // Find all player objects and add them to the list
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            playerTransforms.Add(player.transform);
        }

        Debug.Log("Player list updated in CameraController.");
    }

    // Set the boundaries of the current map using Tilemap
    public void SetMapBoundaries(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minY = bounds.min.y;
        maxY = bounds.max.y;
    }
}
