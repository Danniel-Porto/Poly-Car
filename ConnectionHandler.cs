using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using UnityEngine.UI;
using MLAPI.Transports.UNET;

public class ConnectionHandler : NetworkBehaviour
{
    [SerializeField] NetworkManager network;

    [SerializeField] InputField ipAddressField;
    [SerializeField] Button connectButton, hostServerButton, disconnectButton;


    public void ConnectButton()
    {
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddressField.text;
        NetworkManager.Singleton.StartClient();
    }

    public void StartServerButton()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void DisconnectButton()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.StopHost();
        }
        else if (IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
        connectButton.interactable = hostServerButton.interactable = true;
    }

    private void Update()
    {
        connectButton.interactable = hostServerButton.interactable = !(NetworkManager.Singleton.IsConnectedClient | NetworkManager.Singleton.IsHost);
    }
}
