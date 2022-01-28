using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsBehaviour : MonoBehaviour
{
    public Quaternion newPosition = Quaternion.identity;
    public float smooth;
    public Transform secondObject;

    public float timeCount = 0.0f;

    private void Update()
    {
        transform.rotation = Quaternion.LerpUnclamped(transform.rotation, secondObject.rotation, smooth * Time.deltaTime);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, secondObject.rotation, smooth * Time.deltaTime);
        /*
        transform.rotation = Quaternion.Slerp(transform.rotation, secondObject.rotation, timeCount);
        timeCount = timeCount + Time.deltaTime;
        */
    }
}
