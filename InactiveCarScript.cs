using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InactiveCarScript : MonoBehaviour
{
    [Header("Referencias de colliders")]
    [SerializeField] public WheelCollider frontLeftWheelCollider;
    [SerializeField] public WheelCollider frontRightWheelCollider;
    [SerializeField] public WheelCollider rearLeftWheelCollider;
    [SerializeField] public WheelCollider rearRightWheelCollider;

    [Header("Referencias de transform")]
    [SerializeField] public Transform frontLeftWheelTransform;
    [SerializeField] public Transform frontRightWheeTransform;
    [SerializeField] public Transform rearLeftWheelTransform;
    [SerializeField] public Transform rearRightWheelTransform;
    [SerializeField] private Transform centerOfMass;

    [Header("Model")]
    [SerializeField] GameObject carMesh;

    [Header("Car Atributes")]
    public float speed = 0.5f;
    public float acceleration = 0.5f;
    public float handling = 0.5f;
    public float offroad = 0.5f;

    [SerializeField] public Material[] avaliableCarColors;

    Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        frontRightWheelCollider.brakeTorque = 9999f;
        frontLeftWheelCollider.brakeTorque = 9999f;
        rearLeftWheelCollider.brakeTorque = 9999f;
        rearRightWheelCollider.brakeTorque = 9999f;
    }

    private void Update()
    {
        UpdateWheels();
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.Rotate(0f, -90, 0f);
        wheelTransform.position = pos;
    }

    public void SetColor(int color)
    {
        carMesh.GetComponent<MeshRenderer>().material = avaliableCarColors[color];
    }
}
