using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] Text fpsText;
    int count = 0;
    private void Update()
    {
        if (count >= 15)
        {
            fpsText.text = Convert.ToInt32(1 / Time.deltaTime).ToString();
            count = 0;
        } 
        else
        {
            count += 1;
        }

    }
}
