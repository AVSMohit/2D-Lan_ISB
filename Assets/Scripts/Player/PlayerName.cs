using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    public TMP_Text playerNameText; // TextMeshPro Text to display the player name

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            // Get the player's name from PlayerPrefs
            string name = PlayerPrefs.GetString("PlayerName", $"Player {OwnerClientId}");

            // Set the player's name on the server
            SetPlayerNameServerRpc(name);
        }

        // Update the player name text immediately after spawning
        OnPlayerNameChanged(default, playerName.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        // Set this player's name on the server
        playerName.Value = name;

        // Sync the name across all clients
        UpdatePlayerNameClientRpc(name);
    }

    [ClientRpc]
    private void UpdatePlayerNameClientRpc(string name)
    {
        // Update the player name on each client
        playerNameText.text = name;
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();  // Display the updated player name
    }
}
