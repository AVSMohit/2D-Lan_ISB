using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public bool enableGravity = true;
    private PlayerController[] players;

    private void Start()
    {
        UpdatePlayerGravity();
    }

    public void UpdatePlayerGravity()
    {
        players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.SetGravity(enableGravity);
        }
    }
}
