using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] Behaviour[] componentsToEnable;
    [SerializeField] GameObject[] gameObjectsToDisable;
    [SerializeField] GameObject[] gameObjectsToEnable;

    private NetworkVariable<float> motorPitch = new NetworkVariable<float>(1);
    private NetworkVariable<float> tireGravelPitch = new NetworkVariable<float>(1);
    private NetworkVariable<float> tireGravelVolume = new NetworkVariable<float>(0);
    private NetworkVariable<float> tireSqueakPitch = new NetworkVariable<float>(1);
    private NetworkVariable<float> tireSqueakVolume = new NetworkVariable<float>(0);

    public NetworkVariable<string> playerName = new NetworkVariable<string>("");
    private NetworkVariable<int> carColor = new NetworkVariable<int>(0);
    private NetworkVariable<int> carType = new NetworkVariable<int>(0);
    private NetworkVariable<bool> collisionEnabled = new NetworkVariable<bool>(true);

    public NetworkVariable<int> lap = new NetworkVariable<int>(0);
    public NetworkVariable<int> checkpointNumber = new NetworkVariable<int>(0);

    [SerializeField] TextMesh playerNameText;

    [SerializeField] public GameObject carModel;
    [SerializeField] Material[] color;
    [SerializeField] public Material[] transparentCarMaterials;
    public Material[] thisCarMaterials;

    Rigidbody thisRigidbody;
    [SerializeField] AudioSource motorAudioSource;
    [SerializeField] AudioSource tireOnGravelAudiosource;
    [SerializeField] AudioSource tireSqueakAudioSource;

    [SerializeField] public TerrainIdetifier terrainIdentifier;

    bool isMainMenu;

    private void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();

        if (!IsOwner)
        {
            thisRigidbody.isKinematic = false;
            thisRigidbody.useGravity = false;
            foreach (Behaviour component in componentsToDisable)
            {
                component.enabled = false;
            }
            foreach (GameObject gameObject in gameObjectsToDisable)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in gameObjectsToEnable)
            {
                gameObject.SetActive(true);
            }
        }
        if (IsOwner)
        {
            foreach (Behaviour component in componentsToEnable)
            {
                component.enabled = true;
            }

            SetCarDefinitionsServerRpc(PlayerPrefs.GetInt("CarModel"), PlayerPrefs.GetInt("CarColor"), PlayerPrefs.GetString("PlayerName"));
        }

        Invoke("GetThisCarMaterials", 1);

        //NetworkVariable method subscription to set local changes
        carColor.OnValueChanged += OnCarColorChanged;
        carType.OnValueChanged += OnCarTypeChanged;
        playerName.OnValueChanged += OnPlayerNameChanged;

        motorPitch.OnValueChanged += OnCarPitchChanged;
        tireGravelPitch.OnValueChanged += OnTireOnGravelPitchChanged;
        tireGravelVolume.OnValueChanged += OnTireOnGravelVolumeChanged;
        tireSqueakPitch.OnValueChanged += OnTireSqueakPitchChanged;
        tireSqueakVolume.OnValueChanged += OnTireSqueakVolumeChanged;

        collisionEnabled.OnValueChanged += OnCollisionEnabledValueChanged;
    }

    void GetThisCarMaterials()
    {
        thisCarMaterials = carModel.GetComponent<MeshRenderer>().materials;
    }

    [ServerRpc]
    public void SetCheckpointNumberServerRpc(int lap, int checkpoint)
    {
        if (IsServer)
        {
            this.lap.Value = lap;
            this.checkpointNumber.Value = checkpoint;
        }
    }

    #region Car sound update region

    [ServerRpc]
    public void SetCarPitchServerRpc(float carPitch, float tireOnGravelPitch, float tireOnGravelVolume, float tireSqueakPitch, float tireSqueakVolume)
    {
        if (IsServer)
        {
            this.motorPitch.Value = carPitch;
            this.tireGravelPitch.Value = tireOnGravelPitch;
            this.tireGravelVolume.Value = tireOnGravelVolume;
            this.tireSqueakPitch.Value = tireSqueakPitch;
            this.tireSqueakVolume.Value = tireSqueakVolume;
        }
    }

    [ServerRpc]
    public void SetCarCollisionServerRpc(bool collisionEnabled)
    {
        this.collisionEnabled.Value = collisionEnabled;
    }

    private void OnCollisionEnabledValueChanged(bool prevValue, bool newValue)
    {
        if (!IsLocalPlayer)
        {
            if (!newValue)
            {
                GetComponent<PlayerNetwork>().carModel.GetComponent<MeshRenderer>().materials = transparentCarMaterials;
            }
            else
            {
                GetComponent<PlayerNetwork>().carModel.GetComponent<MeshRenderer>().materials = thisCarMaterials;
            }

            Collider[] colliders = carModel.GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = newValue;
            }

            CapsuleCollider[] capsuleColliders = carModel.GetComponents<CapsuleCollider>();
            foreach (CapsuleCollider collider in capsuleColliders)
            {
                collider.enabled = newValue;
            }
        }
    }

    private void OnCarPitchChanged(float prevPitch, float newPitch)
    {
        if (!IsLocalPlayer)
        {
            motorAudioSource.pitch = motorPitch.Value;
        }
    }

    private void OnTireOnGravelPitchChanged(float prevPitch, float newPitch)
    {
        if (!IsLocalPlayer)
        {
            tireOnGravelAudiosource.pitch = tireGravelPitch.Value;
        }
    }

    private void OnTireOnGravelVolumeChanged(float prevPitch, float newPitch)
    {
        if (!IsLocalPlayer)
        {
            tireOnGravelAudiosource.volume = tireGravelVolume.Value;
        }
    }

    private void OnTireSqueakPitchChanged(float prevPitch, float newPitch)
    {
        if (!IsLocalPlayer)
        {
            tireSqueakAudioSource.pitch = tireSqueakPitch.Value;
        }
    }
    private void OnTireSqueakVolumeChanged(float prevPitch, float newPitch)
    {
        if (!IsLocalPlayer)
        {
            tireSqueakAudioSource.volume = tireSqueakVolume.Value;
        }
    }


    #endregion

    #region Car/Player Definitions Region
    [ServerRpc]
    public void SetCarDefinitionsServerRpc(int carType, int carColor, string playerName)
    {
        if (IsServer)
        {
            this.carColor.Value = carColor;
            this.carType.Value = carType;
            this.playerName.Value = playerName;
            SetCarDefinitionsClientRpc(carType, carColor, playerName);
        }
    }

    [ClientRpc]
    public void SetCarDefinitionsClientRpc(int carType, int carColor, string playerName)
    {
        carModel.GetComponent<MeshRenderer>().material = color[carColor];
        playerNameText.text = playerName;
    }

    private void OnCarColorChanged(int prevColor, int newColor)
    {
        carModel.GetComponent<MeshRenderer>().material = color[newColor];
    }

    private void OnCarTypeChanged(int prevType, int newType)
    {

    }

    private void OnPlayerNameChanged(string prevName, string newName)
    {
        playerNameText.text = newName;
    }
    #endregion
}
