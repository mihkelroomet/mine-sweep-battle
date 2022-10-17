using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using JetBrains.Annotations;

public class Canvas : MonoBehaviour
{
    public static Canvas Instance;
    public TextMeshProUGUI ScoreText;
    public int initialOpen;
    public TextMeshProUGUI ScoreTextonPanel;
    // Panel
    public GameObject EndsGamePanel;
    // start time value
    [SerializeField] float startTime;

    // current Time
    float currentTime;

    // whether the timer started?
    bool timerStarted = false;

    // ref var for my TMP text component
    [SerializeField] TMP_Text timerText;

    void Start()
    {
        initialOpen = Cell.openNo;
        ScoreText.text = "Score: ";
        EndsGamePanel.SetActive(false);
        //resets the currentTime to the start Time 
        currentTime = startTime;
        //displays the UI with the currentTime
        timerText.text = currentTime.ToString();
        // starts the time -- comment this out if you don't want to automagically start
        timerStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        ScoreText.text = "Score:" + (Cell.openNo - initialOpen).ToString();
        if (timerStarted)
        {
            // subtracting the previous frame's duration
            currentTime -= Time.deltaTime;
            // logic current reached 0? if yes show message
            if (currentTime <= 0)
            {
                ShowMessage();
                timerStarted = false;
                currentTime = 0;
            }

            timerText.text = "Time " + currentTime.ToString("f1");

            //Show message when All cells are opened
            if (Cell.openNo == Grid.Instance.Columns * Grid.Instance.Rows)
            {
                ShowMessage();
            }

        }

    }
    public void RestartPressed()
    {
        PlayerController.Instance.Restart();
    }
    public void ShowMessage()
    {
        ScoreTextonPanel.text = "Your Score :"+(Cell.openNo-initialOpen).ToString();
        EndsGamePanel.SetActive(true);
    }

}
