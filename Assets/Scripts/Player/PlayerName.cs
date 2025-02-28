using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    public TMP_Text playerNameText; // TextMeshPro Text to display the player name

    // Network variable holding the player's name (college ID or generic label)public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""));


    // Expose the network variable value via a property called playerID
    public FixedString32Bytes playerID
    {
        get { return playerName.Value; }
    }

    public override void OnNetworkSpawn()
    {
        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TMP_Text>();
            if (playerNameText == null)
            {
                Debug.LogError("PlayerName: TMP_Text is not assigned and could not be found.");
                return;
            }
        }

        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            // For the local player, set name to "YOU"
            SetPlayerNameServerRpc("YOU");
        }
        else
        {
            // For remote players, set a generic label
            SetPlayerNameServerRpc("Player");
        }

        // Update immediately
        OnPlayerNameChanged(default, playerName.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        playerName.Value = name;
        UpdatePlayerNameClientRpc(name);
    }

    [ClientRpc]
    private void UpdatePlayerNameClientRpc(string name)
    {
        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TMP_Text>();
        }
        if (playerNameText != null)
        {
            playerNameText.text = name;
        }
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TMP_Text>();
        }
        if (playerNameText != null)
        {
            playerNameText.text = newName.ToString();
        }
    }
}
