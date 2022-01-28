using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System;
using MLAPI.Transports;
using MLAPI.Connection;
using MLAPI.SceneManagement;

public class LobbyHandler : NetworkBehaviour
{
    private NetworkVariable<int> track = new NetworkVariable<int>(0);
    private NetworkVariable<int> collision = new NetworkVariable<int>(0);
    private NetworkVariable<int> lapsNumber = new NetworkVariable<int>(0);
    private NetworkVariable<int> teamsMode = new NetworkVariable<int>(0);
    private NetworkVariable<int> timeCondition = new NetworkVariable<int>(0);

    [Header("Insert level images")]
    [SerializeField] Texture[] previewImages;

    [Header("Insert level names")]
    [SerializeField] string[] racingTracks;


    [Header("UI Serialization")]
    [SerializeField] RawImage previewImage;
    [SerializeField] Dropdown trackDropdown;
    [SerializeField] Dropdown collisionDropdown;
    [SerializeField] Dropdown lapsNumberDropdown;
    [SerializeField] Dropdown teamsDropdown;
    [SerializeField] Dropdown timeConditionDropdown;
    [SerializeField] Button startGameButton;

    GameObject[] playerSlots = new GameObject[20];
    GameObject[] players = new GameObject[20];

    [SerializeField] GameObject playerPrefab;

    [SerializeField] MainMenuHandler mmh;
    private void Start()
    {
        track.OnValueChanged += collision.OnValueChanged += lapsNumber.OnValueChanged += teamsMode.OnValueChanged += timeCondition.OnValueChanged += valueChanged;
        SetPlayerSlots();
    }

    [ServerRpc]
    public void OnSettingsChangeServerRpc()
    {
        track.Value = trackDropdown.value;
        collision.Value = collisionDropdown.value;
        lapsNumber.Value = lapsNumberDropdown.value;
        teamsMode.Value = teamsDropdown.value;
        timeCondition.Value = timeConditionDropdown.value;
    }

    private void valueChanged(int prevValue, int newValue)
    {
        trackDropdown.value = track.Value;
        collisionDropdown.value = collision.Value;
        lapsNumberDropdown.value = lapsNumber.Value;
        teamsDropdown.value = teamsMode.Value;
        timeConditionDropdown.value = timeCondition.Value;
        previewImage.texture = previewImages[track.Value];
    }

    public void StartGame()
    {
        NetworkSceneManager.SwitchScene(racingTracks[trackDropdown.value]);
    }

    private void OnEnable()
    {
        Invoke("SetInteractable", 0.1f);
        if (IsHost)
        {
            CheckConnectedPlayers();
        }
    }

    void SetInteractable()
    {
        if (!IsHost)
        {
            trackDropdown.interactable = false;
            collisionDropdown.interactable = false;
            lapsNumberDropdown.interactable = false;
            teamsDropdown.interactable = false;
            timeConditionDropdown.interactable = false;
            startGameButton.gameObject.SetActive(false);
        }
        else
        {
            trackDropdown.interactable = true;
            collisionDropdown.interactable = true;
            lapsNumberDropdown.interactable = true;
            teamsDropdown.interactable = true;
            timeConditionDropdown.interactable = true;
            startGameButton.gameObject.SetActive(true);
        }
        trackDropdown.value = track.Value;
        collisionDropdown.value = collision.Value;
        lapsNumberDropdown.value = lapsNumber.Value;
        teamsDropdown.value = teamsMode.Value;
        timeConditionDropdown.value = timeCondition.Value;
    }

    private void FixedUpdate()
    {
        GetPlayerList();
        AllocatePlayersToSlots();
        UpdateSettings();
    }

    void UpdateSettings()
    {
        NetworkManager.Singleton.GetComponent<CustomNetworkManagerProperties>().totalLaps = lapsNumberDropdown.value + 1;
        NetworkManager.Singleton.GetComponent<CustomNetworkManagerProperties>().teams = teamsDropdown.value;
        NetworkManager.Singleton.GetComponent<CustomNetworkManagerProperties>().collision = collisionDropdown.value == 0 ? true : false;
    }

    private void CheckConnectedPlayers()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            bool hasOwner = false;
            ulong owner = player.GetComponent<NetworkObject>().OwnerClientId;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId == owner)
                {
                    hasOwner = true;
                    break;
                } else
                {
                    hasOwner = false;
                }
            }
            if (!hasOwner)
            {
                player.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    private void GetPlayerList()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }

    private void SetPlayerSlots()
    {
        GameObject[] tempPlayerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");

        for (int i = 0; i < 15; i++)
        {
            foreach (GameObject playerSlot in tempPlayerSlots)
            {
                if (Convert.ToInt32(playerSlot.name) == i)
                {
                    playerSlot.GetComponent<Text>().text = "";
                    playerSlots[i] = playerSlot;
                    break;
                }
            }
        }
    }
    
    private void AllocatePlayersToSlots()
    {
        if (players[0] != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (string.IsNullOrEmpty(playerSlots[i].GetComponent<Text>().text))
                {
                    playerSlots[i].GetComponent<Text>().text = players[i].GetComponent<PlayerManager>().playerName.Value;
                } 
                else if (playerSlots[i].GetComponent<Text>().text != players[i].GetComponent<PlayerManager>().playerName.Value)
                {
                    playerSlots[i].GetComponent<Text>().text = players[i].GetComponent<PlayerManager>().playerName.Value;
                }

            }
            for (int i = players.Length; i < playerSlots.Length; i++)
            {
                if (playerSlots[i] != null)
                    playerSlots[i].GetComponent<Text>().text = "";
            }
        }
    }

    public void Lobby_AbandonLobby()
    {
        if (IsHost)
        {
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (!IsHost)
                {
                    foreach (NetworkObject ownedObject in client.OwnedObjects)
                    {
                        ownedObject.Despawn(true);
                    }
                    NetworkManager.Singleton.DisconnectClient(client.ClientId);
                }
            }
            NetworkManager.Singleton.StopHost();
        }
        if (IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
    }
}
