using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviourPun
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
            photonView.RPC("SendMessageToClients", RpcTarget.All, message, PhotonNetwork.NickName);
        }
    }

    private void OnChatInputEndEdit(string message)
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(message))
        {
            chatInputField.text = "";
            chatInputField.ActivateInputField();
            photonView.RPC("SendMessageToClients", RpcTarget.All, message, PhotonNetwork.NickName);
        }
    }

    [PunRPC]
    private void SendMessageToClients(string message, string senderName)
    {
        string formattedMessage = $"{senderName}: {message}";
        chatDisplayText.text += formattedMessage + "\n";
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    private void Update()
    {
        // Any additional updates as needed
    }
}
