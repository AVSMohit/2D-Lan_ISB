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
            string name = PlayerPrefs.GetString("PlayerName", $"Player {OwnerClientId}");
            SetPlayerNameServerRpc(name);
        }

        // Update the player name text immediately after spawning
        OnPlayerNameChanged(default, playerName.Value);
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string newName, ServerRpcParams rpcParams = default)
    {
        playerName.Value = newName;
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }
}
