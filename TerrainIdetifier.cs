using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainIdetifier : MonoBehaviour
{
    public bool isInContactWithOtherCar;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCarTrigger"))
        {
            isInContactWithOtherCar = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCarTrigger"))
        {
            isInContactWithOtherCar = false;
        }
    }
}
