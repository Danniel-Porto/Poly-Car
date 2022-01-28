using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using UnityEngine.UI;
using MLAPI.Transports;
using System;
using MLAPI.Transports.UNET;
using MLAPI.Connection;

public class MainMenuHandler : NetworkBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settingsMenuPanel;
    [SerializeField] GameObject playPanel;
    [SerializeField] GameObject joinLobbyPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject garageMenuPanel;
    [SerializeField] GameObject lobbyPanelPrefab;

    [Header("Input Fields")]
    [SerializeField] InputField ipAddress;
    [SerializeField] InputField playerNameInputField;

    [Header("Cameras")]
    [SerializeField] GameObject carCam;
    [SerializeField] GameObject lobbyCam;
    [SerializeField] GameObject settingsCam;

    [Header("PlayerPrefab")]
    [SerializeField] GameObject playerPrefab;

    [Header("Inactive Car Prefabs")]
    [SerializeField] GameObject[] inactiveVehicles;
    [SerializeField] GameObject vehicleSpawnPosition;
    [SerializeField] GameObject actualVehicle;
    [SerializeField] Text carNameField;
    [SerializeField] Text colorField;
    [SerializeField] Button confirmSelectedCarButton;
    

    [Header("Audios")]
    [SerializeField] AudioClip wharislove;
    [SerializeField] AudioClip select;
    AudioSource menuAudioSource;

    int selectedCar, selectedColor;
    bool isMainMenuActive = true;

    private void Start()
    {
        Cursor.visible = true;

        menuAudioSource = GetComponent<AudioSource>();
        menuAudioSource.PlayOneShot(wharislove, 0.05f);

        GetSetSettings();

        if (IsHost)
        {
            Invoke("InstantiateLobby", 1);
        }
    }

    private void FixedUpdate()
    {
        CheckConnection();
    }

    private void Update()
    {
        SetCamera();
    }

    void GetSetSettings()
    {
        //TODO SETTINGS
        if (PlayerPrefs.HasKey("CarModel"))
        {
            selectedCar = PlayerPrefs.GetInt("CarModel");
            selectedColor = PlayerPrefs.GetInt("CarColor");
        } else
        {
            PlayerPrefs.SetInt("CarModel", 0);
            PlayerPrefs.SetInt("CarColor", 0);
        }

        if (PlayerPrefs.HasKey("LastTypedIp"))
        {
            ipAddress.text = PlayerPrefs.GetString("LastTypedIp");
        }
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SaveIpAddres()
    {
        PlayerPrefs.SetString("LastTypedIp", ipAddress.text);
    }

    private void SetCamera()
    {
        carCam.SetActive(garageMenuPanel.activeSelf);
        lobbyCam.SetActive(GameObject.FindGameObjectWithTag("Lobby"));
        settingsCam.SetActive(settingsMenuPanel.activeSelf);
    }

    private void CheckConnection()
    {
        if (!isMainMenuActive & GameObject.FindGameObjectsWithTag("Lobby").Length == 0)
        {
            ReturnToDefault();
            isMainMenuActive = true;
        } else if (isMainMenuActive & GameObject.FindGameObjectsWithTag("Lobby").Length > 0)
        {
            IsConnected();
            isMainMenuActive = false;
        }
    }

    #region Buttons Methods

    public void MainMenu_PlayButton()
    {
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void MainMenu_LeaveButton()
    {
        Application.Quit();
    }

    public void ReturnToDefault()
    {
        mainMenuPanel.SetActive(true); // this was true
        settingsMenuPanel.SetActive(false);
        playPanel.SetActive(false);
        joinLobbyPanel.SetActive(false);
    }

    public void IsConnected()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);
        playPanel.SetActive(false);
        joinLobbyPanel.SetActive(false);
    }

    public void MainMenu_SettingsButton()
    {
        //CONFIGURAR CARREGAR AS CONFIGS
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void MainMenu_GarageButton()
    {
        mainMenuPanel.SetActive(false);
        garageMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);

        selectedCar = PlayerPrefs.GetInt("CarModel");
        selectedColor = PlayerPrefs.GetInt("CarColor");

        SpawnInactiveCar(PlayerPrefs.GetInt("CarModel"), PlayerPrefs.GetInt("CarColor"));

        UpdateGarageUI();
    }

    public void PlayMenu_JoinButton()
    {
        playPanel.SetActive(false);
        joinLobbyPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void PlayMenu_CreateLobbyButton()
    {
        NetworkManager.Singleton.StartHost();
        menuAudioSource.PlayOneShot(select, 1f);
        Invoke("InstantiateLobby", 1);
    }

    void InstantiateLobby()
    {
        if(IsHost)
        {
            GameObject lobby = Instantiate(lobbyPanelPrefab);
            lobby.GetComponent<NetworkObject>().Spawn(null, true);
        }
    }

    public void PlayMenu_CancelButton()
    {
        playPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void Settings_ConfirmButton()
    {
        //CONFIGURAR SALVAR CONFIGS
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void Settings_DiscardButton()
    {
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    public void JoinLobby_JoinButton()
    {
        //CONECTAR COMO CLIENTE

        //disable all buttons on joinlobby screen TODO

        //Invoke("JoinButtonLogic", 1);

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddress.text;
        NetworkManager.Singleton.StartClient();
        menuAudioSource.PlayOneShot(select, 1f);
    }

    private void JoinButtonLogic()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            print("connection failed");
            NetworkManager.Singleton.StopClient();
        }
    }

    public void JoinLobby_CancelButton()
    {
        joinLobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
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
        //lobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    #endregion

    #region Garage Methods

    void SpawnInactiveCar(int carId, int carColorId)
    {
        if (GameObject.FindGameObjectWithTag("InactiveCar") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("InactiveCar"));
        }
        actualVehicle = Instantiate(inactiveVehicles[carId], vehicleSpawnPosition.transform.position, vehicleSpawnPosition.transform.rotation);

        if (selectedColor >= actualVehicle.GetComponent<InactiveCarScript>().avaliableCarColors.Length)
        {
            selectedColor = actualVehicle.GetComponent<InactiveCarScript>().avaliableCarColors.Length - 1;
        }
        actualVehicle.GetComponent<InactiveCarScript>().SetColor(selectedColor);
        UpdateGarageUI();
    }

    public void Garage_NextCarButton()
    {
        print("next car button");
        selectedCar += 1;
        if (selectedCar >= inactiveVehicles.Length)
        {
            selectedCar = 0;
        }
        SpawnInactiveCar(selectedCar, selectedColor);
        UpdateGarageUI();
    }

    public void Garage_PrevCarButton()
    {
        selectedCar -= 1;
        if (selectedCar < 0)
        {
            selectedCar = inactiveVehicles.Length - 1;
        }
        SpawnInactiveCar(selectedCar, selectedColor);
        UpdateGarageUI();
    }

    public void Garage_NextColorButton()
    {
        
        selectedColor += 1;
        if (selectedColor >= actualVehicle.GetComponent<InactiveCarScript>().avaliableCarColors.Length)
        {
            selectedColor = 0;
        }
        actualVehicle.GetComponent<InactiveCarScript>().SetColor(selectedColor);
        UpdateGarageUI();
        
    }

    public void Garage_PrevColorButton()
    {
        
        selectedColor -= 1;
        if (selectedColor < 0)
        {
            selectedColor = actualVehicle.GetComponent<InactiveCarScript>().avaliableCarColors.Length - 1;
        }
        actualVehicle.GetComponent<InactiveCarScript>().SetColor(selectedColor);
        UpdateGarageUI();
    }

    public void Garage_Confirm()
    {
        PlayerPrefs.SetInt("CarModel", selectedCar);
        PlayerPrefs.SetInt("CarColor", selectedColor);
        UpdateGarageUI();
    }

    public void Garage_ReturnButton()
    {
        mainMenuPanel.SetActive(true);
        garageMenuPanel.SetActive(false);
        menuAudioSource.PlayOneShot(select, 1f);
    }

    void UpdateGarageUI()
    {
        carNameField.text = actualVehicle.name;
        colorField.text = actualVehicle.GetComponent<InactiveCarScript>().avaliableCarColors[selectedColor].name;
        if (selectedCar != PlayerPrefs.GetInt("CarModel") | selectedColor != PlayerPrefs.GetInt("CarColor"))
        {
            confirmSelectedCarButton.interactable = true;
        } else
        {
            confirmSelectedCarButton.interactable = false;
        }
    }

    #endregion

    public void OnNameWritten()
    {
        PlayerPrefs.SetString("PlayerName", playerNameInputField.text);
    }
}
