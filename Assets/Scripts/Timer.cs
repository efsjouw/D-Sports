using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public TMP_Text timerText;

    public Button startStopButton;
    public Button resetLapButton;

    public TMP_Text startStopTmp;
    public TMP_Text resetLapTmp;

    public ScrollRect lapList;

    private bool running = false;

    private int hours;
    private int minutes; 
    private int seconds; 
    private int deciseconds;

    public void clickStartStopButton()
    {
        if (running) running = false;
        else StartCoroutine(timerRoutine());        
    }

    public void clickResetLapButton()
    {
        if (running) Debug.Log("lap");
        else resetTimer();
    }

    private void toggleButtons()
    {
        startStopTmp.text = running ? "Stop" : "Start";
        resetLapTmp.text = running ? "Lap" : "Reset";
    }

    private IEnumerator timerRoutine()
    {
        running = true;
        toggleButtons();
        while (running)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            deciseconds++;
           
            updateTimer();
            if (deciseconds >= 9)
            {
                deciseconds = 0;
                seconds++;
            }
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
            }
            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
            }
        }
        yield return null;
        toggleButtons();
    }

    private void updateTimer()
    {
        timerText.text = String.Format("{0} : {1} : {2} : {3}",
            hours,
            minutes < 10 ? minutes : minutes,
            seconds < 10 ? seconds : seconds,
            deciseconds);
    }

    private void resetTimer()
    {
        hours = 0;
        minutes = 0;
        seconds = 0;
        deciseconds = 0;
        updateTimer();
    }

}
