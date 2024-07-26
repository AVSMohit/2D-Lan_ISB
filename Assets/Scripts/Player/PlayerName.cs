using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    public TMP_Text playerNameText; // Assign this in the Inspector

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("PlayerName", "Player " + OwnerClientId));
        }

        // Update the player name text immediately after spawning
        OnPlayerNameChanged(default, playerName.Value);
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        playerNameText.text = newValue.ToString();
    }
}
