using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System;

public class PlayerManager : NetworkBehaviour
{
    public NetworkVariable<string> playerName = new NetworkVariable<string>();
    public NetworkVariable<int> carModel = new NetworkVariable<int>();
    public NetworkVariable<int> carColor = new NetworkVariable<int>();
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);


    private void Start()
    {
        if (IsOwner)
        {
            InstantiateValuesServerRpc(PlayerPrefs.GetString("PlayerName"), PlayerPrefs.GetInt("CarModel"), PlayerPrefs.GetInt("CarColor"));
        }
    }

    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("GameManager") == null) { SetReadyServerRpc(false); }
    }

    [ServerRpc]
    private void InstantiateValuesServerRpc(string playerName, int carModel, int carColor)
    {
        this.playerName.Value = playerName;
        this.carModel.Value = carModel;
        this.carColor.Value = carColor;
    }

    [ServerRpc]
    public void UpdateValuesServerRpc(int carModel, int carColor)
    {
        this.carModel.Value = carModel;
        this.carColor.Value = carColor;
    }



    [ServerRpc]
    public void SetReadyServerRpc(bool isReady)
    {
        this.isReady.Value = isReady;
    }
}
