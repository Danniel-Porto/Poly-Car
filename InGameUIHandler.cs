using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class InGameUIHandler : MonoBehaviour
{
    public int lap;

    [SerializeField] Text velosimetro;

    GameManager gm;
    GameNetworkManager gnm;
    PlayerManager localPlayer;

    [Header("Ready Panel")]
    [SerializeField] GameObject readyPanel;
    [SerializeField] GameObject notReadyPanel;
    [SerializeField] GameObject readyOrNotPanel;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        gnm = gm.GetComponent<GameNetworkManager>();
        GetLocalPlayer();
    }


    private void FixedUpdate()
    {
        UpdateSpeed();
    }

    private void Update()
    {
        UpdateReadyPanel();
    }

    private void GetLocalPlayer()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PlayerManager>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                localPlayer = player.GetComponent<PlayerManager>();
            } 
        }
    }

    private void UpdateReadyPanel()
    {
        readyOrNotPanel.SetActive(gnm.isPreMatch.Value);
        if (gnm.isPreMatch.Value & !localPlayer.isReady.Value & Input.GetKeyDown(KeyCode.Space))
        {
            localPlayer.SetReadyServerRpc(true);
        } else if (gnm.isPreMatch.Value & localPlayer.isReady.Value & Input.GetKeyDown(KeyCode.Space))
        {
            localPlayer.SetReadyServerRpc(false);
        }

        readyPanel.SetActive(localPlayer.isReady.Value);
        notReadyPanel.SetActive(!localPlayer.isReady.Value);
    }

    void UpdateSpeed()
    {
        if (gm.localPlayerCar != null)
        {
            velosimetro.text = ("velosidade: " + gm.localPlayerCar.GetComponent<CarController>().kmph.ToString("F2"));
        }
    }

    public void UpdateUILap(int lap)
    {
        print("actual lap: " + lap);
    }

    public void Win()
    {
        print("you won the race");
    }
}
