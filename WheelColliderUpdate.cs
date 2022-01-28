using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelColliderUpdate : MonoBehaviour
{
    [SerializeField] Transform wheel;
    WheelCollider wheelCollider;

    private void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }


    private void Update()
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheel.rotation = rot;
        wheel.Rotate(0f, -90, 0f);
        wheel.position = pos;
    }
}
