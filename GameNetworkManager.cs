using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Connection;
using MLAPI.NetworkVariable;
using System;

public class GameNetworkManager : NetworkBehaviour
{
    [SerializeField] GameObject[] playerPrefabs;

    public NetworkVariable<int> totalLaps = new NetworkVariable<int>();
    public NetworkVariable<int> teams = new NetworkVariable<int>();
    public NetworkVariable<bool> collision = new NetworkVariable<bool>();

    public NetworkVariable<bool> isPreMatch = new NetworkVariable<bool>(true);

    public NetworkVariable<ulong[]> connectionsPositions = new NetworkVariable<ulong[]>();

    public List<GameObject> positions = new List<GameObject>();

    public List<GameObject> activeRacers = new List<GameObject>();


    GameObject[] spawnPoints;

    GameManager gm;

    CustomNetworkManagerProperties cnmp;

    public bool isRaceFreezed = true;

    private void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        SortSpawnPoints();
        gm = GetComponent<GameManager>();
        cnmp = NetworkManager.Singleton.GetComponent<CustomNetworkManagerProperties>();

        Invoke("RpcInvoke", 1);

        if (IsServer)
        {
            isPreMatch.Value = true;
        }
    }

    void RpcInvoke()
    {
        if (IsServer)
        {
            SetGameSettingsServerRpc(cnmp.totalLaps, cnmp.teams, cnmp.collision);
        }
    }

    private void Update()
    {
        if (isPreMatch.Value & IsHost)
        {
            if (AllPlayersReady())
            {
                SpawnPlayers();
                isPreMatch.Value = false;
            }
        }

        if (IsServer)
        {
            PositionManage();
        }

        if (!IsServer)
        {
            SortPosition();
        }
    }

    bool AllPlayersReady()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerManager info = player.GetComponent<PlayerManager>();
            if (!info.isReady.Value)
            {
                return false;
            }
        }

        return true;
    }

    [ServerRpc]
    void SetGameSettingsServerRpc(int totalLaps, int teams, bool collision)
    {
        this.totalLaps.Value = totalLaps;
        this.teams.Value = teams;
        this.collision.Value = collision;
        UpdateGameManagerClientRpc(totalLaps);
    }

    [ClientRpc]
    void UpdateGameManagerClientRpc(int totalLaps)
    {
        gm.totalLaps = totalLaps;
    }


    void PositionManage()
    {
        foreach (GameObject player in activeRacers)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                PlayerNetwork pn = player.GetComponent<PlayerNetwork>();
                PlayerNetwork pn2 = positions[i].GetComponent<PlayerNetwork>();

                if (pn2 == pn) { break; }

                if (pn.lap.Value > pn2.lap.Value)
                {
                    positions.Remove(player);
                    positions.Insert(i, player);
                    break;
                }
                else if (pn.lap.Value == pn2.lap.Value & pn.checkpointNumber.Value > pn2.checkpointNumber.Value)
                {
                    positions.Remove(player);
                    positions.Insert(i, player);
                    break;
                }
            }
        }
        ulong[] tempConnections = new ulong[positions.Count];
        foreach (GameObject player in positions)
        {
            tempConnections[positions.IndexOf(player)] = player.GetComponent<NetworkObject>().OwnerClientId;
        }
        PositionsServerRpc(tempConnections);
    }

    [ServerRpc]
    void PositionsServerRpc(ulong[] connections)
    {
        connectionsPositions.Value = connections;
        //PositionsClientRpc(connections);
    }

    [ClientRpc]
    void PositionsClientRpc(ulong[] connections)
    {
        connectionsPositions.Value = connections;
    }

    void SortPosition()
    {
        positions.Clear();
        foreach (ulong connection in connectionsPositions.Value)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("PlayerCar"))
            {
                if (player.GetComponent<NetworkObject>().OwnerClientId == connection)
                {
                    positions.Add(player);
                }
            }
        }
    }


    private void SortSpawnPoints()
    {
        GameObject[] tempSpawnPoints = spawnPoints;
        for (int i = 0; i < tempSpawnPoints.Length; i++)
        {
            foreach (GameObject sp in tempSpawnPoints)
            {
                SpawnPointInfo info = sp.GetComponent<SpawnPointInfo>();
                if (info.spawnPosition == i)
                {
                    spawnPoints[i] = sp;
                    break;
                }
            }
        }
    }


    private void SpawnPlayers()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++) 
        {
            InstantiatePlayerVehicle(NetworkManager.Singleton.ConnectedClientsList[i], spawnPoints[i]);

            spawnPoints[i].GetComponent<SpawnPointInfo>().SetOccupiedServerRpc(true);
            spawnPoints[i].GetComponent<SpawnPointInfo>().isOccupied.Value = true;
        }
    }

    private void InstantiatePlayerVehicle(NetworkClient vehicleOwner, GameObject spawnLocation)
    {
        PlayerManager pm = GetPlayerManager(vehicleOwner);

        GameObject thisVehicle = Instantiate(playerPrefabs[pm.carModel.Value], spawnLocation.transform.position, spawnLocation.transform.rotation);
        thisVehicle.GetComponent<NetworkObject>().SpawnAsPlayerObject(vehicleOwner.ClientId);

        activeRacers.Add(thisVehicle); //Adiciona o veículo instanciado a lista de carros
        positions.Add(thisVehicle);
    }

    private PlayerManager GetPlayerManager(NetworkClient connectedUser)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in playerObjects)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == connectedUser.ClientId)
            {
                print("FOUND PLAYER MANAGER");
                return player.GetComponent<PlayerManager>();
            }
        }
        print("no player manager found, wtf?");
        return null;
    }

}
