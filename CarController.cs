using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    public const string VERTICAL = "Vertical";

    private float horizontalInput;
    public float verticalInput;
    private float currentSteerAngle;
    [SerializeField] private float currentbrakeForce;
    [SerializeField] private float currentHandBrakeForce;
    [SerializeField] public bool isBraking;
    [SerializeField] private bool handBrake;

    [Header("Aspectos da Maquina")]
    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    [SerializeField] private float handBrakeForce;
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private float topSpeed;
    [SerializeField] private float offroadStiffnessMultiplier;
    [SerializeField] private bool fwd;
    [SerializeField] private bool rwd;
    float finalStiffnessMultiplier = 1;

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

    [Header("Cameras")]
    [SerializeField] Camera lookBackCamera;
    Camera mainCamera;

    [Header("Car Properties")]
    public float carDefaultFrontalStiffness;
    public float carDefaultRearStiffness;

    [Header("Debug")]
    [SerializeField] public float kmph;
    [SerializeField] double steeringControl;
    [SerializeField] float velocity;
    [SerializeField] TerrainIdetifier carContactDetector;
    public Vector3 driftAngle;
    public Vector3 driftValue;
    public float accelFactor;

    public bool isFrontLeftWheelGrounded, isFrontRightWheelGrounded, isRearLeftWheelGrounded, isRearRightWheelGrounded;

    public bool isDrifting;
    public bool driftingIntensity;

    Rigidbody rb;
    GameManager gm;

    Collider[] colliders;

    WheelFrictionCurve rearLeftWheelColliderFriction;
    WheelFrictionCurve rearRightWheelColliderFriction;
    WheelFrictionCurve frontLeftWheelColliderFriction;
    WheelFrictionCurve frontRightWheelColliderFriction;

    Vector3 oldPosition;
    float speed, speedPerSec, constMaxSteer;

    Vector3 devTestPosition, devTestVelocity, devTestAngularVelocity;
    Quaternion devTestRotation;

    [SerializeField] AudioClip hornAudioClip;




    [SerializeField] CinemachineFreeLook cine;

    private void Start()
    {
        devTestPosition = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position; //DEV TESTING STUFF
        devTestRotation = GameObject.FindGameObjectWithTag("SpawnPoint").transform.rotation;

        carDefaultFrontalStiffness = frontLeftWheelCollider.sidewaysFriction.stiffness;
        carDefaultRearStiffness = rearLeftWheelCollider.sidewaysFriction.stiffness;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        rb = GetComponent<Rigidbody>();
        constMaxSteer = maxSteerAngle;
        rearLeftWheelColliderFriction = rearLeftWheelCollider.sidewaysFriction;
        rearRightWheelColliderFriction = rearRightWheelCollider.sidewaysFriction;
        frontLeftWheelColliderFriction = frontLeftWheelCollider.sidewaysFriction;
        frontRightWheelColliderFriction = frontRightWheelCollider.sidewaysFriction;

        rb.centerOfMass = centerOfMass.localPosition;

        //cine = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineFreeLook>();
        cine.Follow = transform;
        cine.LookAt = transform;

        if (GameObject.FindGameObjectWithTag("GameManager") != null)
        {
            gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
    }

    private void FixedUpdate()
    {
        DefineStiffnessMultiplier();

        CalculateAcceleration();
        CalculateSpeed();
        GroundCheck();
        SlideMeter();

        HandleMotor();
        HandleSteering();
        HandBrake();

        UpdateWheels();
    }

    private void Update()
    {
        GetInput();
        ResetCar();

        DevPositionCar();
        DevResetCar();

        Horn();

        LookBack();
    }

    private void LookBack()
    {
        if (Input.GetKey(KeyCode.C)) 
        {
            mainCamera.enabled = false;
            mainCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            lookBackCamera.enabled = true;
            lookBackCamera.gameObject.GetComponent<AudioListener>().enabled = true;
        } else
        {
            mainCamera.enabled = true;
            mainCamera.gameObject.GetComponent<AudioListener>().enabled = true;
            lookBackCamera.enabled = false;
            lookBackCamera.gameObject.GetComponent<AudioListener>().enabled = false;
        }
    }

    void DevPositionCar()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            devTestPosition = transform.position;
            devTestRotation = transform.rotation;
            devTestVelocity = rb.velocity;
            devTestAngularVelocity = rb.angularVelocity;
        }
    }

    void DevResetCar()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            transform.position = devTestPosition;
            transform.rotation = devTestRotation;
            rb.velocity = devTestVelocity;
            rb.angularVelocity = devTestAngularVelocity;
        }
    }

    void Horn()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            gameObject.GetComponent<AudioSource>().PlayOneShot(hornAudioClip, 1f);
        }
    }

    private void ResetCar()
    {
        if (Input.GetButtonDown("Reset") & gm != null & gm.lastGrabbedCheckpoint != null)
        {
            transform.position = gm.lastGrabbedCheckpoint.GetComponent<CheckpointInfo>().restorePoint.transform.position;
            transform.rotation = gm.lastGrabbedCheckpoint.GetComponent<CheckpointInfo>().restorePoint.transform.rotation;
            rb.velocity *= 0;
            rb.angularVelocity *= 0;

            GetComponent<LocalPlayerGameHandler>().SetIvulnerability(true, 3);
        }
    }

    void DefineStiffnessMultiplier()
    {
        switch(GetComponent<LocalPlayerGameHandler>().terrainTag)
        {
            case "Terrain":
                finalStiffnessMultiplier = offroadStiffnessMultiplier;
                break;
            default:
                finalStiffnessMultiplier = 1;
                break;
        }
        //ApplyStiffness();
    }

    void ApplyStiffness()
    {
        rearLeftWheelColliderFriction.stiffness = carDefaultRearStiffness * finalStiffnessMultiplier;
        frontLeftWheelColliderFriction.stiffness = carDefaultFrontalStiffness * finalStiffnessMultiplier;

        rearLeftWheelCollider.sidewaysFriction = rearLeftWheelColliderFriction;
        rearRightWheelCollider.sidewaysFriction = rearLeftWheelColliderFriction;
        frontLeftWheelCollider.sidewaysFriction = frontLeftWheelColliderFriction;
        frontRightWheelCollider.sidewaysFriction = frontLeftWheelColliderFriction;

    }

    void SlideMeter()
    {
        if (velocity > 100)
        {
            Vector3 forward = transform.forward;
            forward.y = 0.0f;
            Vector3 velocity = rb.velocity;
            velocity.y = 0.0f;
            float angleVelocityOffset = Vector3.Angle(forward, velocity);
            if (angleVelocityOffset > 12)
            {
                isDrifting = true;
            }
            else
            {
                isDrifting = false;
            }
        }
        else
        {
            isDrifting = false;
        }
    }

    void GroundCheck()
    {
        isFrontLeftWheelGrounded = frontLeftWheelCollider.isGrounded;
        isFrontRightWheelGrounded = frontLeftWheelCollider.isGrounded;
        isRearLeftWheelGrounded = frontLeftWheelCollider.isGrounded;
        isRearRightWheelGrounded = frontLeftWheelCollider.isGrounded;
    }

    private void GetInput()
    {
        horizontalInput = Mathf.Lerp(horizontalInput, Input.GetAxisRaw(HORIZONTAL), 30 * Time.deltaTime);
        verticalInput = Mathf.Lerp(verticalInput, Input.GetAxisRaw(VERTICAL), 30 * Time.deltaTime);
        handBrake = Input.GetButton("Handbrake");
        isBraking = false;
    }

    void CalculateAcceleration()
    {
        //accelFactor = Convert.ToSingle((-Math.Pow(kmph, 2) + 10000) * 0.0001);
        accelFactor = - (kmph / topSpeed) + 1;
        accelFactor = accelFactor <= 0 ? 0 : accelFactor;
    }

    void CalculateSpeed()
    {
        speedPerSec = Vector3.Distance(oldPosition, transform.position) / Time.deltaTime;
        speed = Vector3.Distance(oldPosition, transform.position);
        oldPosition = transform.position;

        kmph = speedPerSec * 3.6f;

        //steeringControl = (-(kmph / 3) + 100)/100;
        steeringControl = 0.3447271 + (1.000082 - 0.3447271) / Math.Pow((1 + (kmph / 153.857)), 1.750791);
        maxSteerAngle = constMaxSteer * Convert.ToSingle(steeringControl);


        //y = 0.3447271 + (1.000082 - 0.3447271)/(1 + (x/153.857)^1.750791) RC
        //y = 0.1342695 + (0.999345 - 0.1342695) / Math.Pow((1 + (kmph / 68.06818)), 2.964011) adapted
        //y = 0.05393652 + (0.9996346 - 0.05393652) / Math.Pow((1 + (speed / 46.74707)), 1.239347)
        //y = 0.1229678 + (0.969554 - 0.1229678) / Math.Pow((1 + (speed / 0.4534188)), 3.603186) SEGUND
        //y = -0.07962413 + (0.9894917 - -0.07962413) / (1 + (x / 0.5203518) ^ 2.598067) PRIMEIR
    }

    private void HandleMotor()
    {
        //motorForce *= fwd & rwd ? 0.5f : 1;

        if (fwd)
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce * accelFactor;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce * accelFactor;
        }

        if (rwd)
        {
            rearLeftWheelCollider.motorTorque = verticalInput * motorForce * accelFactor;
            rearRightWheelCollider.motorTorque = verticalInput * motorForce * accelFactor;
        }

        velocity = (frontLeftWheelCollider.rpm + frontRightWheelCollider.rpm + rearLeftWheelCollider.rpm + rearRightWheelCollider.rpm) / 4;

        if (verticalInput > 0 & velocity > 0)
        {
            isBraking = false;
        } else if (verticalInput < 0 & velocity > 0)
        {
            isBraking = true;
        }

        if (verticalInput < 0 & velocity < 0)
        {
            isBraking = false;
        }
        else if (verticalInput > 0 & velocity < 0)
        {
            isBraking = true;
        }

        currentbrakeForce = isBraking ? brakeForce : 0f;

        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbrakeForce / 3;
        frontLeftWheelCollider.brakeTorque = currentbrakeForce / 3;
        rearLeftWheelCollider.brakeTorque = currentbrakeForce;
        rearRightWheelCollider.brakeTorque = currentbrakeForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

        //rb.velocity
    }

    private void HandBrake()
    {
        currentHandBrakeForce = handBrake ? handBrakeForce : 0f;
        if (handBrake)
        {
            rearLeftWheelColliderFriction.stiffness = carDefaultRearStiffness * 0.57f * finalStiffnessMultiplier;
            rearRightWheelColliderFriction.stiffness = carDefaultRearStiffness * 0.57f * finalStiffnessMultiplier;

            rearLeftWheelCollider.sidewaysFriction = rearLeftWheelColliderFriction;
            rearRightWheelCollider.sidewaysFriction = rearRightWheelColliderFriction;

            rearLeftWheelCollider.brakeTorque = currentHandBrakeForce;
            rearRightWheelCollider.brakeTorque = currentHandBrakeForce;
        } else
        {
            rearLeftWheelColliderFriction.stiffness = carDefaultRearStiffness * finalStiffnessMultiplier;
            rearRightWheelColliderFriction.stiffness = carDefaultRearStiffness * finalStiffnessMultiplier;
            frontLeftWheelColliderFriction.stiffness = carDefaultFrontalStiffness * finalStiffnessMultiplier;
            frontRightWheelColliderFriction.stiffness = carDefaultFrontalStiffness * finalStiffnessMultiplier;

            rearLeftWheelCollider.sidewaysFriction = rearLeftWheelColliderFriction;
            rearRightWheelCollider.sidewaysFriction = rearRightWheelColliderFriction;
            frontLeftWheelCollider.sidewaysFriction = frontLeftWheelColliderFriction;
            frontRightWheelCollider.sidewaysFriction = frontRightWheelColliderFriction;
        }
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
}