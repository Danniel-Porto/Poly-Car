using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class SpawnPointInfo : NetworkBehaviour
{
    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);

    [Header("Posição do jogador neste spawn")]
    public int spawnPosition;

    [ServerRpc]
    public void SetOccupiedServerRpc(bool occupied)
    {
        if (IsServer)
        {
            isOccupied.Value = occupied;
            SetOccupiedClientRpc(occupied);
        }
    }

    [ClientRpc]
    private void SetOccupiedClientRpc(bool occupied)
    {
        isOccupied.Value = occupied;
    }
}
