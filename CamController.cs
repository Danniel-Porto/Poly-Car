using UnityEngine;

public class CamController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Camera cam;
    public GameObject cameraCenter;
    public Vector3 offset;

    public float turningRate = 30f;
    private Quaternion _targetRotation = Quaternion.identity;
    float timer;
    bool cameraStill;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("Camera").GetComponentInChildren<Camera>();
        cameraCenter = GameObject.FindGameObjectWithTag("Camera");
    }

    public void SetBlendedEulerAngles(Vector3 angles)
    {
        _targetRotation = Quaternion.Euler(angles);
    }

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(cameraCenter.transform.position, desiredPosition, smoothSpeed);
        cameraCenter.transform.position = smoothedPosition;

        if (cameraStill)
        {
            SetBlendedEulerAngles(target.eulerAngles);
            cameraCenter.transform.rotation = Quaternion.RotateTowards(cameraCenter.transform.rotation, _targetRotation, turningRate * Time.deltaTime);
        }

        cameraCenter.transform.LookAt(target);
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse X") != 0)
        {
            cameraCenter.transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
            cameraStill = false;
            timer = 2f;
        }
        else
        {
            timer -= 0.02f;
            if (timer <= 0)
            {
                cameraStill = true;
            }
        }
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject cameraCenter;

    Vector3 offset = new Vector3(0f, 1.55f, -5.5f);
    Vector3 oldPosition;

    private void Start()
    {
        cam.transform.position = cameraCenter.transform.position + offset;
    }

    private void Update()
    {
        cameraCenter.transform.position = Vector3.Slerp(oldPosition, transform.position, 5 * Time.deltaTime );
        oldPosition = cameraCenter.transform.position;
    }

}*/
