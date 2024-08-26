using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    public InputField chatInputField; // TextMeshPro InputField
    public Button sendButton; // Unity UI Button
    public TMP_Text chatDisplayText; // TextMeshPro Text
    public ScrollRect chatScrollRect; // Unity UI ScrollRect

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        chatInputField.onEndEdit.AddListener(OnChatInputEndEdit);
    }

    private void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            string message = chatInputField.text;
            chatInputField.text = "";
            chatInputField.ActivateInputField();
            SendMessageToServerRpc(message);
        }
    }

    private void OnChatInputEndEdit(string message)
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(message))
        {
            chatInputField.text = "";
            chatInputField.ActivateInputField();
            SendMessageToServerRpc(message);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageToServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        string senderName = GetPlayerName(senderId);
        string formattedMessage = $"{senderName}: {message}";
        ReceiveMessageOnClientRpc(formattedMessage);
    }

    private string GetPlayerName(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerNameScript = client.PlayerObject.GetComponent<PlayerName>();
            return playerNameScript != null ? playerNameScript.playerNameText.text : $"Player {clientId}";
        }
        return $"Player {clientId}";
    }

    [ClientRpc]
    private void ReceiveMessageOnClientRpc(string message, ClientRpcParams rpcParams = default)
    {
        chatDisplayText.text += message + "\n";
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    private void Update()
    {
        
    }
}
