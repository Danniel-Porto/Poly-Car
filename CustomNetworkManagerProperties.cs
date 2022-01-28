using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Connection;
using MLAPI.Transports;
using MLAPI.NetworkVariable;

public class CustomNetworkManagerProperties : NetworkBehaviour
{
    List<NetworkClient> connectedClientsPreviousList = new List<NetworkClient>();
    public int localClientCount, managerClientCount;
    public bool keepChecking;

    //Game Settings
    public int totalLaps;
    public int teams;
    public bool collision;

    private void Update()
    {
        localClientCount = connectedClientsPreviousList.Count;
        managerClientCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (IsHost)
        {
            OnListChanged();
        }
    }

    #region Message Handling

    void OnListChanged()
    {
        //DISCONNECTED CLIENT LOGIC
        if (connectedClientsPreviousList.Count > NetworkManager.Singleton.ConnectedClientsList.Count & keepChecking)
        {
            List<NetworkClient> changedClients = GetDisconnectedClients();
            foreach (NetworkClient disconnectedClient in changedClients)
            {
                foreach (NetworkObject playerObject in GameObject.FindObjectsOfType<NetworkObject>())
                {
                    if (playerObject.OwnerClientId == disconnectedClient.ClientId & playerObject.IsPlayerObject)
                    {
                        playerObject.Despawn(true);
                    }
                }
                print("CLIENT DISCONNECTED: " + disconnectedClient.ClientId);
            }
            NetworkClient thisClient = null;
        } 

        //CONNECTED CLIENT LOGIC
        else if (connectedClientsPreviousList.Count < NetworkManager.Singleton.ConnectedClientsList.Count & keepChecking)
        {
            List<NetworkClient> changedClients = GetConnectedClients();
            foreach (NetworkClient connectedClient in changedClients)
            {
                print("CLIENT CONNECTED: " + connectedClient.ClientId);
            }
        }
    }

    void UpdateList()
    {
        connectedClientsPreviousList.Clear();
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            connectedClientsPreviousList.Add(NetworkManager.Singleton.ConnectedClientsList[i]);
        }
    }

    List<NetworkClient> GetDisconnectedClients()
    {
        List<NetworkClient> disconnectedClients = new List<NetworkClient>();
        int i = 0;
        while (i < connectedClientsPreviousList.Count) 
        {
            if (i < NetworkManager.Singleton.ConnectedClientsList.Count)
            {
                if (connectedClientsPreviousList[i] != NetworkManager.Singleton.ConnectedClientsList[i])
                {
                    disconnectedClients.Add(connectedClientsPreviousList[i]);
                    connectedClientsPreviousList.RemoveAt(i);
                }
                else
                {
                    i += 1;
                }
            } else
            {
                disconnectedClients.Add(connectedClientsPreviousList[i]);
                connectedClientsPreviousList.RemoveAt(i);
                i += 1;
            }
        }
        UpdateList();
        return disconnectedClients;
    }

    List<NetworkClient> GetConnectedClients()
    {
        List<NetworkClient> connectedClients = new List<NetworkClient>();
        int i = connectedClientsPreviousList.Count;
        while (i < NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            connectedClients.Add(NetworkManager.Singleton.ConnectedClientsList[i]);
            connectedClientsPreviousList.Add(NetworkManager.Singleton.ConnectedClientsList[i]);
            i += 1;
        }
        UpdateList();
        return connectedClients;
    }

    [ServerRpc]
    public void DespawnObjectServerRpc(ulong item)
    {
        if (IsServer)
        {
            NetworkClient thisClient = null;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (item == client.ClientId)
                {
                    thisClient = client;
                }
            }

            foreach (NetworkObject nObject in thisClient.OwnedObjects)
            {
                nObject.Despawn(true);
            }
        }
    }

    #endregion
}
