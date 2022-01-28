using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class LocalPlayerGameHandler : MonoBehaviour
{
    GameManager gm;
    Rigidbody rb;
    CarController cc;
    PlayerNetwork pn;

    public int lap;
    public int checkpointNumber;

    [SerializeField] AudioSource carEngineAudioSource;
    [SerializeField] AudioSource carTireOnGravelAudioSource;
    [SerializeField] AudioSource carTireSqueakAudioSource;

    [SerializeField] Transform terrainIdentifier;

    public string terrainTag;

    float carPitch = 1;
    float tireOnGravelPitch = 1;
    float tireOnGravelVolume = 0;
    float tireSqueakPitch = 1;
    float tireSqueakVolume = 0;
    [SerializeField] float carPitchDampingRate = 1;
    [SerializeField] float tireOnGravelDampingRate = 1;
    [SerializeField] float carSqueakDampingRate = 1;

    [Header("Ivulnerability")]
    [SerializeField] TerrainIdetifier carContactDetector;
    bool ivulnerability = false;
    float ivulnerabilityTime = 0;
    public bool isInContactWithOtherPlayer = false;

    [SerializeField] float floatingFactor = 1;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("GameManager") != null)
        {
            gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CarController>();
        pn = GetComponent<PlayerNetwork>();

        gm.localPlayerCar = this.gameObject;

        gm.gameObject.GetComponent<GameNetworkManager>().isRaceFreezed = false; // DELETA ESSA MERDA DEPOISSSS
    }
    private void FixedUpdate()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(terrainIdentifier.position, new Vector3(0f, -1f, 0f),out hit))
        {
            if (hit.collider != null)
            {
                terrainTag = hit.collider.tag;
            }
        }
    }

    private void Update()
    {
        lap = gm.lap;
        checkpointNumber = gm.lastGrabbedCheckpoint.GetComponent<CheckpointInfo>().number;

        UpdateMotorPitch();
        UpdateTiresOnGravelSound();
        UpdateTireSqueakSound();
        pn.SetCarPitchServerRpc(carPitch, tireOnGravelPitch, tireOnGravelVolume, tireSqueakPitch, tireSqueakVolume);
        pn.SetCheckpointNumberServerRpc(lap, checkpointNumber);

        //Detect collision with other cars and count time for ivulnerability
        isInContactWithOtherPlayer = carContactDetector.isInContactWithOtherCar;
        IvulnerabilityCountdown();
    }

    public void SetIvulnerability(bool value, float ivulnerabilityDurationInSeconds)
    {
        GameObject[] allCars = GameObject.FindGameObjectsWithTag("PlayerCar");

        //Will go through every player car in the scene and disable every collider inside carmodel
        foreach (GameObject car in allCars)
        {
            Collider[] colliders = car.GetComponent<PlayerNetwork>().carModel.GetComponents<BoxCollider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = !value;
            }

            CapsuleCollider[] ccolliders = car.GetComponent<PlayerNetwork>().carModel.GetComponents<CapsuleCollider>();
            foreach (Collider collider in ccolliders)
            {
                collider.enabled = !value;
            }
        }

        //Will re-enable the colliders on THIS car kkkk
        Collider[] colliders2 = gameObject.GetComponent<PlayerNetwork>().carModel.GetComponents<BoxCollider>();
        foreach (Collider collider in colliders2)
        {
            collider.enabled = true;
        }

        CapsuleCollider[] ccolliders2 = gameObject.GetComponent<PlayerNetwork>().carModel.GetComponents<CapsuleCollider>();
        foreach (Collider collider in colliders2)
        {
            collider.enabled = true;
        }

        //Set transparency 
        if (value)
        {
            pn.carModel.GetComponent<MeshRenderer>().materials = pn.transparentCarMaterials;
        } else
        {
            pn.carModel.GetComponent<MeshRenderer>().materials = pn.thisCarMaterials;
        }

        ivulnerability = value;
        ivulnerabilityTime = ivulnerabilityDurationInSeconds;
        gameObject.GetComponent<PlayerNetwork>().SetCarCollisionServerRpc(!value);
    }

    private void IvulnerabilityCountdown()
    {
        if (ivulnerability & ivulnerabilityTime <= 0 & !isInContactWithOtherPlayer)
        {
            SetIvulnerability(false, 0);
        }
        else if (ivulnerabilityTime > 0)
        {
            ivulnerabilityTime -= Time.deltaTime;
        }
    }

    private void UpdateMotorPitch()
    {
        float oldCarPitch = carPitch;
        float nextCarPitch;
        if (!(cc.isFrontLeftWheelGrounded & cc.isFrontRightWheelGrounded & cc.isRearLeftWheelGrounded & cc.isRearRightWheelGrounded)) 
        {
            nextCarPitch = (3.5f * cc.verticalInput);
        } 
        else
        {
            nextCarPitch = (cc.kmph * (cc.verticalInput + 1) / 90) + 1;
        }
        nextCarPitch = Mathf.Clamp(nextCarPitch, 1, 3.5f);
        carPitch = Mathf.Lerp(oldCarPitch, nextCarPitch, carPitchDampingRate * Time.deltaTime);
        carEngineAudioSource.pitch = carPitch;
    }

    private void UpdateTiresOnGravelSound()
    {
        float oldVolume = tireOnGravelVolume;
        float oldPitch = tireOnGravelPitch;
        float nextVolume;
        float nextPitch;
        if ((cc.isFrontLeftWheelGrounded | cc.isFrontRightWheelGrounded | cc.isRearLeftWheelGrounded | cc.isRearRightWheelGrounded) & terrainTag == "Terrain")
        {
            nextPitch = (cc.kmph / 300) + 0.9f;
            nextVolume = cc.kmph / 20;
        }
        else
        {
            nextPitch = 1;
            nextVolume = 0;
        }
        tireOnGravelPitch = Mathf.Clamp(nextPitch, 0.9f, 1.2f);
        tireOnGravelVolume = Mathf.Clamp(nextVolume, 0, 1);
        tireOnGravelVolume = Mathf.Lerp(oldVolume, nextVolume, tireOnGravelDampingRate * Time.deltaTime);
        carTireOnGravelAudioSource.pitch = tireOnGravelPitch;
        carTireOnGravelAudioSource.volume = tireOnGravelVolume;
    }

    private void UpdateTireSqueakSound()
    {
        float oldVolume = tireSqueakVolume;
        float oldPitch = tireSqueakPitch;
        float nextVolume;
        float nextPitch;
        if (cc.isDrifting & (cc.isRearLeftWheelGrounded | cc.isRearRightWheelGrounded) & terrainTag == "Untagged")
        {
            nextPitch = (cc.kmph / 300) + 0.9f;
            nextVolume = cc.kmph / 20;
        }
        else
        {
            nextPitch = 0.8f;
            nextVolume = 0;
        }
        tireSqueakPitch = Mathf.Clamp(nextPitch, 0.9f, 1.2f);
        tireSqueakVolume = Mathf.Clamp(nextVolume, 0, 1);
        tireSqueakPitch = Mathf.Lerp(oldPitch, nextPitch, carSqueakDampingRate * Time.deltaTime);
        tireSqueakVolume = Mathf.Lerp(oldVolume, nextVolume, carSqueakDampingRate * Time.deltaTime);
        carTireSqueakAudioSource.pitch = tireSqueakPitch;
        carTireSqueakAudioSource.volume = tireSqueakVolume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            gm.CheckpointGrab(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            rb.velocity += new Vector3(0f, floatingFactor * Time.deltaTime, 0f);
        }
    }


}
