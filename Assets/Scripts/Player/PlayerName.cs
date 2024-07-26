using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    public TMP_Text playerNameText;
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("PlayerName", "Player " + OwnerClientId));
        }
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
