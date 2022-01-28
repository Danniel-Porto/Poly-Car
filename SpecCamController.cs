using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SpecCamController : MonoBehaviour
{
    [SerializeField] GameObject podium;
    CinemachineFreeLook camera;
    int target = 0;

    private void Start()
    {
        camera = GetComponent<CinemachineFreeLook>();
    }

    private void Update()
    {
        if (GameObject.FindGameObjectsWithTag("PlayerCar").Length < 1)
        {
            camera.Follow = podium.transform;
            camera.LookAt = podium.transform;
        } else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                target += 1;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                target -= 1;
            }

            target = target < 0 ? GameObject.FindGameObjectsWithTag("PlayerCar").Length - 1 : target;
            target = target >= GameObject.FindGameObjectsWithTag("PlayerCar").Length ? 0 : target;

            camera.Follow = GameObject.FindGameObjectsWithTag("PlayerCar")[target].transform;
            camera.LookAt = GameObject.FindGameObjectsWithTag("PlayerCar")[target].transform;
        }
    }
}
