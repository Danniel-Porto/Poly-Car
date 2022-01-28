using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointInfo : MonoBehaviour
{
    public int number = 0;
    public bool isActive = false;
    [SerializeField] public GameObject pointer;
    [SerializeField] public Transform restorePoint;
    [SerializeField] ParticleSystem[] confettis;

    private void Start()
    {
        
    }

    public void SetActiveCheckpoint(bool status)
    {
        isActive = status;
        pointer.SetActive(status);
        GetComponent<Collider>().enabled = status;
    }

    public void PlayCheckpointConfetti()
    {
        foreach (ParticleSystem confetti in confettis)
        {
            confetti.Play();
        }
    }
}
