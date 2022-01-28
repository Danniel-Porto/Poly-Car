using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFilterDetector : MonoBehaviour
{
    [SerializeField] GameObject filterCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            filterCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            filterCanvas.SetActive(false);
        }
    }
}
