using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameCameraFocus : MonoBehaviour
{
    GameObject cam;
    public Quaternion offset;
    Vector3 worldUp = Vector3.up;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("Camera");
    }

    private void Update()
    {
        transform.LookAt(cam.transform, worldUp /*cam.transform.up*/);
        transform.localRotation *= offset;
    }
}
