using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class SyncWithPhysics : NetworkBehaviour
{
    private NetworkVariable<Vector3> syncVelocity = new NetworkVariable<Vector3>(new Vector3(0f, 0f, 0f));
    private NetworkVariable<Vector3> syncPosition = new NetworkVariable<Vector3>(new Vector3(0f, 0f, 0f));
    private NetworkVariable<Quaternion> syncRotation = new NetworkVariable<Quaternion>(new Quaternion(0f, 0f, 0f, 0f));
    private NetworkVariable<Vector3[]> syncWheelPosition = new NetworkVariable<Vector3[]>(new Vector3[4]);
    private NetworkVariable<Quaternion[]> syncWheelRotation = new NetworkVariable<Quaternion[]>(new Quaternion[4]);
    Vector3 tempPosition;
    Quaternion tempRotation;
    Vector3 tempVelocity;
    Vector3 movementPrediction;
    Quaternion rotationPrediction;

    [SerializeField]
    Transform[] wheel;
    Quaternion[] wheelRotation = new Quaternion[4];
    Vector3[] wheelPosition = new Vector3[4];
    CarController car;

    Rigidbody rb;

    public float smoothing = 0.02f;
    public float maxDegreesDelta = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        syncPosition.OnValueChanged = OnSyncPositionChanged;
        syncRotation.OnValueChanged = OnSyncRotationChanged;
    }

    private void Update()
    {
        if (IsOwner)
        {
            for (int i = 0; i < wheel.Length; i++)
            {
                wheelRotation[i] = wheel[i].localRotation;
                wheelPosition[i] = wheel[i].localPosition;
            }
            UpdatePositionServerRpc(rb.velocity, transform.position, transform.rotation, wheelPosition, wheelRotation);
        }

        if (!IsOwner)
        {
            Lerp(tempVelocity, tempPosition, tempRotation);
        }
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 velocity, Vector3 position, Quaternion rotation, Vector3[] wheelPosition, Quaternion[] wheelRotation)
    {
        if (IsServer)
        {
            syncVelocity.Value = velocity;
            syncPosition.Value = position;
            syncRotation.Value = rotation;
            for (int i = 0; i < wheel.Length; i++)
            {
                syncWheelRotation.Value[i] = wheelRotation[i];
                syncWheelPosition.Value[i] = wheelPosition[i];
            }
            UpdatePositionClientRpc(velocity, position, rotation, syncWheelPosition.Value, syncWheelRotation.Value);
        }
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 velocity, Vector3 position, Quaternion rotation, Vector3[] wheelPosition, Quaternion[] wheelRotation)
    {
        if (!IsLocalPlayer)
        {
            tempVelocity = velocity;
            tempPosition = position;
            tempRotation = rotation;
            for (int i = 0; i < wheel.Length; i++)
            {
                wheel[i].localRotation = wheelRotation[i];
                wheel[i].localPosition = wheelPosition[i];
            }
        }
    }

    void OnSyncPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        movementPrediction = oldPosition - newPosition;
    }

    void OnSyncRotationChanged(Quaternion oldRotation, Quaternion newRotation)
    {
        rotationPrediction.eulerAngles = oldRotation.eulerAngles - newRotation.eulerAngles;
    }

    //Cereja do bolo
    private void Lerp(Vector3 velocity, Vector3 position, Quaternion rotation)
    {
        rb.velocity = velocity;

        transform.position = Vector3.Lerp(transform.position, position, smoothing * Time.deltaTime);
        transform.rotation = Quaternion.LerpUnclamped(transform.rotation, rotation, 12 * Time.deltaTime);
        rb.rotation = transform.rotation;
        rb.angularVelocity = Vector3.zero;

        transform.position += (movementPrediction / 128f) * Time.deltaTime;
    }
}
