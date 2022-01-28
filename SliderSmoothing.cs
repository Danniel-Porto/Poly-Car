using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderSmoothing : MonoBehaviour
{
    public float target;
    Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        slider.value = Mathf.Lerp(slider.value, target, 10 * Time.deltaTime);
    }
}
