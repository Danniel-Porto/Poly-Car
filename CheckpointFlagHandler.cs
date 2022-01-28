using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointFlagHandler : MonoBehaviour
{
    private void Start()
    {
        Invoke("GetAllCarColliders", 8);
    }

    void GetAllCarColliders()
    {
        CapsuleCollider[] allCapsulesColliders = new CapsuleCollider[GameObject.FindGameObjectsWithTag("PlayerCar").Length];
        int index = 0;
        foreach (GameObject car in GameObject.FindGameObjectsWithTag("PlayerCar"))
        {
            allCapsulesColliders[index] = car.GetComponent<PlayerNetwork>().terrainIdentifier.GetComponent<CapsuleCollider>();
            index += 1;
        }
        GetComponent<Cloth>().capsuleColliders = allCapsulesColliders;
    }
}
