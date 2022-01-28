using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using System;

public class EmitterHandler : NetworkBehaviour
{

    [Header("Wheel Colliders for positioning emitters")]
    [SerializeField] GameObject[] wheelColliders;
    
    [Header("References For Tire Smoke")]
    [SerializeField] ParticleSystem rearLeftTireSmoke, rearRightTireSmoke;
    [SerializeField] GameObject rearLeftTireSmokeObject, rearRightTireSmokeObject;
    bool rlts, rrts;

    [Header("References For Lights")]
    [SerializeField] Light leftBrakeLight, rightBrakeLight, leftFrontLight, rightFrontLight;
    bool lightSwitch, brakeLight;

    [Header("References For Dirt Smoke")]
    [SerializeField] ParticleSystem[] dirtSmokeParticleSystems;
    [SerializeField] float dirtSmokeExitTime = 1;
    bool atds, isOnDirt;
    float dirtSmokeIntensity;

    string terrainTag;
    LocalPlayerGameHandler lpgh;
    CarController car;
    
    
    private void Start()
    {
        if (IsOwner)
        {
            car = GetComponent<CarController>();
            lpgh = GetComponent<LocalPlayerGameHandler>();
        }
        Invoke("PositionEmitters", 1);
    }

    private void FixedUpdate()
    {
        ApplyDriftSmoke();
        ApplyDirtSmoke();
    }

    private void Update()
    {
        if (IsOwner)
        {
            GetInput();
            SetTireSmoke();
            SetDirtSmoke();

            
            GetTerrainTag();

            if (IsClient)
                SyncCarStateServerRpc(lightSwitch, brakeLight, rlts, rrts, atds, dirtSmokeIntensity);
        }
        CarLightSwitcher();
    }

    private void GetTerrainTag()
    {
        terrainTag = lpgh.terrainTag;
        if (terrainTag == "Terrain")
        {
            isOnDirt = true;
        } else
        {
            isOnDirt = false;
        }
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            lightSwitch = !lightSwitch;
        }

        brakeLight = car.isBraking;
    }

    void PositionEmitters()
    {
        for (int i = 0; i < dirtSmokeParticleSystems.Length; i++)
        {
            dirtSmokeParticleSystems[i].transform.position = wheelColliders[i].transform.position + new Vector3(0f, -0.2f, 0f);
        }
    }

    void SetTireSmoke()
    {
        if (car.isRearLeftWheelGrounded & car.isDrifting & rearLeftTireSmoke.isStopped & !isOnDirt)
        {
            rlts = true;
        }
        else if (!car.isRearLeftWheelGrounded || (!car.isDrifting & rearLeftTireSmoke.isPlaying))
        {
            rlts = false;
        }

        if (car.isRearRightWheelGrounded & car.isDrifting & rearRightTireSmoke.isStopped & !isOnDirt)
        {
            rrts = true;
        }
        else if (!car.isRearRightWheelGrounded || (!car.isDrifting & rearRightTireSmoke.isPlaying))
        {
            rrts = false;
        }
    }

    void SetDirtSmoke()
    {
        if (isOnDirt & car.isFrontLeftWheelGrounded & car.isFrontRightWheelGrounded & car.isRearLeftWheelGrounded & car.isRearRightWheelGrounded)
        {
            float oldDirtSmokeIntensity = dirtSmokeIntensity;
            dirtSmokeIntensity = car.kmph / 25;
            dirtSmokeIntensity = Mathf.Clamp(dirtSmokeIntensity, 0, 3.6f);
            dirtSmokeIntensity = Mathf.Lerp(oldDirtSmokeIntensity, dirtSmokeIntensity, dirtSmokeExitTime * Time.deltaTime);
        }
        else
        {
            dirtSmokeIntensity = Mathf.Lerp(dirtSmokeIntensity, 0, dirtSmokeExitTime * Time.deltaTime);
        }

        if (dirtSmokeIntensity > 0.1f)
        {
            atds = true;
        } else 
        {
            atds = false;
        }
    }

    private void ApplyDriftSmoke()
    {
        if (rlts)
        {
            rearLeftTireSmoke.Play();
        }
        else 
        {
            rearLeftTireSmoke.Stop();
        }

        if (rrts)
        {
            rearRightTireSmoke.Play();
        }
        else 
        {
            rearRightTireSmoke.Stop();
        }
    }

    void ApplyDirtSmoke()
    {
        if (atds)
        {
            foreach (ParticleSystem dirtSmoke in dirtSmokeParticleSystems)
            {
                var lifeTime = dirtSmoke.main;
                lifeTime.startSize = dirtSmokeIntensity;
                lifeTime.startSpeed = dirtSmokeIntensity * 0.29f;
                lifeTime.startLifetime = dirtSmokeIntensity;
                dirtSmoke.Play();
            }
        }
        else
        {
            foreach (ParticleSystem dirtSmoke in dirtSmokeParticleSystems)
            {
                dirtSmoke.Stop();
            }
        }
    }

    void CarLightSwitcher()
    {
        leftBrakeLight.enabled = rightBrakeLight.enabled = brakeLight;
        leftFrontLight.enabled = rightFrontLight.enabled = lightSwitch;
    }

    [ServerRpc]
    void SyncCarStateServerRpc(bool lightSwitch, bool brakeLight, bool rlts, bool rrts, bool atds, float dirtIntensity)
    {
        if (IsServer)
            SyncCarStateClientRpc(lightSwitch, brakeLight, rlts, rrts, atds, dirtIntensity);
    }

    [ClientRpc] 
    void SyncCarStateClientRpc(bool lightSwitch, bool brakeLight, bool rlts, bool rrts, bool atds, float dirtIntensity)
    {
        if (!IsLocalPlayer) 
        {
            this.lightSwitch = lightSwitch;
            this.brakeLight = brakeLight;
            this.rlts = rlts;
            this.rrts = rrts;
            this.atds = atds;
            this.dirtSmokeIntensity = dirtIntensity;
        }
    }
}
