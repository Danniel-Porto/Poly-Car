using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleWallHandler : MonoBehaviour
{
    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
