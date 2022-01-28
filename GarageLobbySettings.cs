using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageLobbySettings : MonoBehaviour
{
    [SerializeField] GameObject[] vehicles;

    [Header("UI Serialization")]
    [SerializeField] RawImage preview;
    [SerializeField] Texture[] carImages;
    [SerializeField] Text carName;
    [SerializeField] Text colorName;
    [SerializeField] SliderSmoothing speedSlider;
    [SerializeField] SliderSmoothing accelSlider;
    [SerializeField] SliderSmoothing handlingSlider;
    [SerializeField] SliderSmoothing offroadSlider;

    PlayerManager localPlayer;

    GameObject actualCar;
    int selectedCar, selectedColor;

    private void Start()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<PlayerManager>().IsLocalPlayer)
            {
                localPlayer = player.GetComponent<PlayerManager>();
            }
        }

        selectedCar = PlayerPrefs.GetInt("CarModel");
        selectedColor = PlayerPrefs.GetInt("CarColor");
        actualCar = vehicles[selectedCar];

        UpdateValues();
    }

    public void Garage_NextCarButton()
    {
        selectedCar += 1;
        if (selectedCar >= vehicles.Length)
        {
            selectedCar = 0;
        }
        actualCar = vehicles[selectedCar];

        carName.text = actualCar.name;

        UpdateValues();

        PlayerPrefs.SetInt("CarModel", selectedCar);
    }

    public void Garage_PrevCarButton()
    {
        selectedCar -= 1;
        if (selectedCar < 0)
        {
            selectedCar = vehicles.Length - 1;
        }
        actualCar = vehicles[selectedCar];

        UpdateValues();

        PlayerPrefs.SetInt("CarModel", selectedCar);
    }

    public void Garage_NextColorButton()
    {
        selectedColor += 1;
        if (selectedColor >= actualCar.GetComponent<InactiveCarScript>().avaliableCarColors.Length)
        {
            selectedColor = 0;
        }
        colorName.text = actualCar.GetComponent<InactiveCarScript>().avaliableCarColors[selectedColor].name;
        PlayerPrefs.SetInt("CarColor", selectedColor);
    }

    public void Garage_PrevColorButton()
    {

        selectedColor -= 1;
        if (selectedColor < 0)
        {
            selectedColor = actualCar.GetComponent<InactiveCarScript>().avaliableCarColors.Length - 1;
        }
        colorName.text = actualCar.GetComponent<InactiveCarScript>().avaliableCarColors[selectedColor].name;
        PlayerPrefs.SetInt("CarColor", selectedColor);
    }

    void UpdateValues()
    {
        carName.text = actualCar.name;

        if (selectedColor >= actualCar.GetComponent<InactiveCarScript>().avaliableCarColors.Length)
        {
            selectedColor = actualCar.GetComponent<InactiveCarScript>().avaliableCarColors.Length - 1;
        }
        colorName.text = actualCar.GetComponent<InactiveCarScript>().avaliableCarColors[selectedColor].name;

        speedSlider.target = actualCar.GetComponent<InactiveCarScript>().speed;
        accelSlider.target = actualCar.GetComponent<InactiveCarScript>().acceleration;
        handlingSlider.target = actualCar.GetComponent<InactiveCarScript>().handling;
        offroadSlider.target = actualCar.GetComponent<InactiveCarScript>().offroad;

        preview.texture = carImages[selectedCar];

        localPlayer.UpdateValuesServerRpc(selectedCar, selectedColor);
    }

}
