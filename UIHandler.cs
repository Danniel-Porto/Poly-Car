using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Header("Velosimetro")]
    [SerializeField] Text velText;
    [SerializeField] CarController car;

    [Header("Garage UI")]
    [SerializeField] GameObject garagePanel;
    [SerializeField] GameObject garageInstruction;
    [SerializeField] Button confirmButton, cancelButton;
    [SerializeField] Dropdown carColorDD, carTypeDD;
    [SerializeField] InputField playerNameInputField;

    [Header("Player Network")]
    [SerializeField] PlayerNetwork network;

    int carColor, carType;
    string playerName;

    #region Garage State
    int garageUIState;
    int inactive = 0;
    int triggered = 1;
    int active = 2;
    #endregion

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int kmphINT = Convert.ToInt32(car.kmph);
        velText.text = kmphINT.ToString();
    }

    private void Update()
    {
        OnGarageStateChanged();
    }

    private void OnGarageStateChanged()
    {
        garageInstruction.SetActive(garageUIState == triggered);

        if (garageUIState == triggered & Input.GetKeyDown(KeyCode.E))
        {
            garageUIState = active;
            garagePanel.SetActive(true);
            carColorDD.value = carColor;
            carTypeDD.value = carColor;
            playerNameInputField.text = playerName;
        }
    }

    public void ConfirmButton()
    {
        carColor = carColorDD.value;
        carType = carTypeDD.value;
        playerName = playerNameInputField.text;

        network.SetCarDefinitionsServerRpc(carColor, carType, playerName);

        garagePanel.SetActive(false);
        garageUIState = triggered;
    }

    public void CancelButton()
    {
        garagePanel.SetActive(false);
        garageUIState = triggered;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Garage"))
        {
            garageUIState = triggered;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Garage"))
        {
            garageUIState = inactive;
            garagePanel.SetActive(false);
        }
    }
}
