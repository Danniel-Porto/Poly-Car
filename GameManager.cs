using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public List<List<GameObject>> checkpoints = new List<List<GameObject>>();
    public List<GameObject> activeCheckpoints = new List<GameObject>();

    [SerializeField] Text[] positionTextSlots;
    [SerializeField] Text lapText;

    GameObject startingLine;
    public GameObject lastGrabbedCheckpoint;
    public GameObject nextCheckpoint;

    [SerializeField] GameObject playerPrefab;
    
    [SerializeField] GameObject timerPanel;
    [SerializeField] GameObject timeTextPrefab;
    float timeBetweenCheckpoints, timeBetweenLaps;

    public GameObject localPlayerCar;

    public int lap = 1;
    public int totalLaps = 1;

    GameNetworkManager gnm;

    private void Start()
    {
        gnm = GetComponent<GameNetworkManager>();
        SortCheckpoints();
    }

    private void Update()
    {
        if (!gnm.isRaceFreezed)
        {
            CheckpointTimer();
        }

        UpdatePositionUI();
        UpdateLapUI();
    }

    private void UpdatePositionUI()
    {
        for (int i = 0; i < positionTextSlots.Length; i++)
        {
            if (i >= gnm.positions.Count)
            {
                positionTextSlots[i].text = "";
            }
            else
            {
                positionTextSlots[i].text = (i + 1 + ": " + gnm.positions[i].GetComponent<PlayerNetwork>().playerName.Value);
            }
        }
    }

    private void UpdateLapUI()
    {
        if (localPlayerCar != null)
            lapText.text = ("Lap: " + localPlayerCar.GetComponent<PlayerNetwork>().lap.Value + "/" + totalLaps);
    }

    private void CheckpointTimer()
    {
        timeBetweenCheckpoints += Time.deltaTime;
        timeBetweenLaps += Time.deltaTime;
    }

    void SortCheckpoints()
    {
        GameObject[] tempCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        int cpAmount = 0;
        foreach (GameObject cp in tempCheckpoints)
        {
            if (cp.GetComponent<CheckpointInfo>().number > cpAmount)
            {
                cpAmount = cp.GetComponent<CheckpointInfo>().number;
            }
        }

        for (int i = 0; i <= cpAmount; i++)
        {
            List<GameObject> foundCheckpoints = new List<GameObject>();
            foreach (GameObject cp in tempCheckpoints)
            {
                CheckpointInfo info = cp.GetComponent<CheckpointInfo>();
                if (info.number == i)
                {
                    foundCheckpoints.Add(cp);
                }
            }
            checkpoints.Add(foundCheckpoints);
        }
        startingLine = checkpoints[0][0];
        activeCheckpoints.Add(startingLine);
        lastGrabbedCheckpoint = startingLine;

        print(checkpoints.Count);
    }

    public void GetLocalPlayerCar()
    {
        GameObject[] allCars = GameObject.FindGameObjectsWithTag("PlayerCar");
        foreach (GameObject car in allCars)
        {
            if (car.GetComponent<LocalPlayerGameHandler>().enabled)
            {
                localPlayerCar = car;
                break;
            }
        }
    }

    List<GameObject> GetNextCheckpoint(int currentCheckpointNumber)
    {
        currentCheckpointNumber += 1;
        if (currentCheckpointNumber >= checkpoints.Count)
        {
            return checkpoints[0];
        }
        return checkpoints[currentCheckpointNumber];
    }

    public void CheckpointGrab(GameObject checkpoint)
    {
        lastGrabbedCheckpoint = checkpoint;

        //Gets info from the individual grabbed checkpoint from the car.
        CheckpointInfo info = checkpoint.GetComponent<CheckpointInfo>();

        if (checkpoint == startingLine & lap < totalLaps)
        {
            lap += 1;
            PrintTimeOnScreen(timeBetweenLaps, " seconds (LAP COMPLETION)");
            timeBetweenLaps = 0;
            //inGameUI.UpdateUILap(lap);
        } 
        else if (checkpoint == startingLine & lap == totalLaps)
        {
            PrintTimeOnScreen(timeBetweenLaps, " seconds (LAP COMPLETION)");
            timeBetweenLaps = 0;
            //inGameUI.Win();
        }

        //Disable all checkpoints inside the active checkpoints list.
        foreach (GameObject activeCheckpoint in activeCheckpoints)
        {
            activeCheckpoint.GetComponent<CheckpointInfo>().SetActiveCheckpoint(false);
        }

        //Clear the active checkpoints list.
        activeCheckpoints.Clear();

        info.PlayCheckpointConfetti();

        //Gets nextCheckpointsList then enable all of it inside it
        List<GameObject> nextCheckpointList = GetNextCheckpoint(info.number);

        foreach (GameObject nextCheckpoint in nextCheckpointList)
        {
            nextCheckpoint.GetComponent<CheckpointInfo>().SetActiveCheckpoint(true);
            activeCheckpoints.Add(nextCheckpoint);
        }

        PrintTimeOnScreen(timeBetweenCheckpoints, " seconds");
        timeBetweenCheckpoints = 0;
    }

    void PrintTimeOnScreen(float time, string text)
    {
        timerPanel.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 40f);
        timeTextPrefab.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0f, 40f);
        GameObject thisTime = Instantiate(timeTextPrefab, timeTextPrefab.transform.position, timeTextPrefab.transform.rotation);
        thisTime.transform.parent = timerPanel.gameObject.transform;
        thisTime.GetComponent<Text>().text = (time.ToString("F2") + text);
        thisTime.SetActive(true);
    }

}
