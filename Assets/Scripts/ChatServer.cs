using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Diagnostics;
using System;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer) 
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if (IsHost) 
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            } else {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
                }
            } else 
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are a client {NetworkManager.LocalClientId}");
            }
    }

    private void ServerOnClientConnected(ulong clientId) {
        ServerSendDirectMessage($"I ({NetworkManager.LocalClientId}) see you ({clientId}) have connected to the server, well done",
        clientId,
        clientId);
        SendChatMessageServerRpc($"Player {clientId} connected");
    }
    private void ServerOnClientDisconnected(ulong clientId) {
        SendChatMessageServerRpc($"Player {clientId} disconnected");
    }

    public void DisplayMessageLocally(ulong from, string message) {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if(from == NetworkManager.LocalClientId) {
            fromStr = "you";
            textColor = Color.magenta;
        } else if (from == SYSTEM_ID) {
            fromStr = "SYS";
            textColor = Color.green;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }
    private void OnChatUiMessageEntered(string message) {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) {
        if (message.StartsWith("@")) {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);
            if(!IsClientIdValid(toClientId)) {
                ServerSendDirectMessage("Sorry, this client does not exist", SYSTEM_ID, serverRpcParams.Receive.SenderClientId);
            } else {
            ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
            }
        } else {
        ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default) {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to) {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;
        if (from == to) {
        ReceiveChatMessageClientRpc(message, from, rpcParams);
        } else {
        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
        }
    }
    
        private bool IsClientIdValid(ulong clientId)
    {
        if (IsServer)
        {
            // If you are on the server, iterate through connected clients
            foreach (NetworkClient kvp in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (kvp.ClientId == clientId)
                {
                    // The provided clientId matches a connected client
                    return true;
                }
            }
        }
        else if (IsClient)
        {
            // If you are on a client, you can check your own clientId
            ulong ownClientId = NetworkManager.Singleton.LocalClientId;

            if (ownClientId == clientId)
            {
                // The provided clientId matches your own clientId
                return true;
            }
        }

        // The provided clientId is not valid
        return false;
    }
}
