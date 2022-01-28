using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerCastParticles : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<ParticleSystem>().Play();
    }
}
