using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;
using MLAPI.Messaging;
using MLAPI.Connection;

public class PauseMenuHandler : NetworkBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] GameObject configMenuPanel;
    [SerializeField] GameObject confirmExitPanel;

    [Header("Pause Menu Buttons")]
    [SerializeField] Button settingsButton;
    [SerializeField] Button leaveGameButton;
    [SerializeField] Button resumeGameButton;

    [Header("Settings Menu Buttons")]
    [SerializeField] Button applySettingsButton;
    [SerializeField] Button cancelSettingsButton;

    [Header("Confirm Exit Buttons")]
    [SerializeField] Button leaveGameConfirmButton;
    [SerializeField] Button leaveGameCancelButton;

    [SerializeField] GameManager gm;
    [SerializeField] GameNetworkManager gnm;


    private void Update()
    {
        OpenClosePauseMenu();
        Cursor.visible = (pauseMenuPanel.activeSelf | configMenuPanel.activeSelf | confirmExitPanel.activeSelf);
    }

    void OpenClosePauseMenu()
    {
        if (!pauseMenuPanel.activeSelf & Input.GetKeyDown(KeyCode.Escape))
        {
            configMenuPanel.SetActive(false);
            confirmExitPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
            Cursor.visible = true;
        } 
        else if (pauseMenuPanel.activeSelf & Input.GetKeyDown(KeyCode.Escape)) 
        {
            pauseMenuPanel.SetActive(false);
            Cursor.visible = false;
        }
    }

    public void OpenSettingsMenu()
    {
        configMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void ConfirmSettings()
    {
        configMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void CancelSettings()
    {
        configMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void LeaveGameButton()
    {
        confirmExitPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void ConfirmLeaveGame()
    {
        if (IsHost)
        {
            foreach (GameObject playerCar in GameObject.FindGameObjectsWithTag("PlayerCar"))
            {
                playerCar.GetComponent<NetworkObject>().Despawn();
            }
            NetworkSceneManager.SwitchScene("MainMenu");
        } 
        else if (IsClient)
        {
            //NetworkManager.Singleton.gameObject.GetComponent<CustomNetworkManagerProperties>().DespawnObjectServerRpc(NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.StopClient();
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene("MainMenu");
        } else
        {
            NetworkManager.Singleton.StopClient();
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void SpectateButton()
    {
        DestroyAllPlayerCarServerRpc(NetworkManager.Singleton.LocalClientId);
        ResumeButton();
    }

    [ServerRpc]
    public void DestroyAllPlayerCarServerRpc(ulong clientId)
    {
        foreach (GameObject playerCar in GameObject.FindGameObjectsWithTag("PlayerCar"))
        {
            if (playerCar.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameNetworkManager>().activeRacers.Remove(playerCar);
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameNetworkManager>().positions.Remove(playerCar);
                playerCar.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }



    public void CancelLeaveGame()
    {
        confirmExitPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeButton()
    {
        configMenuPanel.SetActive(false);
        confirmExitPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
    }
}
