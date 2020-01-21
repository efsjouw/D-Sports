using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkoutProgress : Singleton<WorkoutProgress>
{
    public TMP_Text setText;
    public TMP_Text exerciseText;
    public Timetext timerText;
    public TMP_Text getReadyText;
    public Button pauseButton;
    private TMP_Text pauseText;

    public Image backgroundImage;

    public Color readyColor;
    public Color workColor;
    public Color restColor;

    public AudioSource secondBeep; //Beep sound for each countdown second
    public AudioSource roundBeep;  //Beep sound after each round

    public int countdownSeconds = 5;

    private WorkoutPanel.Workout currentWorkout;

    //Both true as the workout has not started yet on app start
    private bool paused = true;
    private bool cancelled = true;

    private void Awake()
    {
        pauseButton.interactable = false;
        pauseText = pauseButton.GetComponentInChildren<TMP_Text>();
        getReadyText.text = "GET READY";
    }

    //private void OnEnable()
    //{
    //    paused = false;
    //}

    //private void OnDisable()
    //{
    //    paused = true;
    //}

    public bool isInProgress()
    {
        return !paused && !cancelled;
    }

    public void cancel()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        paused = false;
        cancelled = true;
        StopCoroutine("workoutLoop");
    }

    public void togglePause()
    {
        paused = !paused;
        pauseText.text = paused ? "Hervatten" : "Pauzeren";
    }

    public void startWorkout(WorkoutPanel.Workout workout)
    {
        paused = false;
        cancelled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        exerciseText.text = "";
        pauseButton.interactable = false;
        currentWorkout = workout;
        StartCoroutine(workoutCountdown());
    }

    private IEnumerator workoutCountdown()
    {
        setText.gameObject.SetActive(false);
        exerciseText.gameObject.SetActive(false);
        getReadyText.gameObject.SetActive(true);        

        backgroundImage.color = readyColor;        
        int seconds = countdownSeconds;
        while(seconds > 0)
        {
            secondBeep.Play();
            seconds--;
            timerText.setTimeText(seconds);
            yield return new WaitForSecondsRealtime(1.0f);
        }

        getReadyText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);

        StartCoroutine("workoutLoop");
    }

    private IEnumerator workoutLoop()
    {
        for (int i = 0; i < currentWorkout.globalSets; i++)
        {
            pauseButton.interactable = false;

            setText.gameObject.SetActive(true);
            setText.text = (i + 1).ToString() + " / " + currentWorkout.globalSets;

            foreach (KeyValuePair<string, ExerciseDataItem> pair in currentWorkout.exercises)
            {
                ExerciseDataItem exercise = pair.Value;

                exerciseText.gameObject.SetActive(true);
                exerciseText.text = exercise.name.ToUpper();

                int workoutSeconds = currentWorkout.globalWorkTime;
                backgroundImage.color = workColor;

                timerText.setTimeText(workoutSeconds);
                roundBeep.Play();
                yield return new WaitForSecondsRealtime(roundBeep.clip.length);

                while (workoutSeconds > 0)
                {
                    if(!pauseButton.interactable) pauseButton.interactable = true;                    
                    yield return new WaitForSecondsRealtime(1.0f);
                    workoutSeconds--;

                    if (paused) yield return new WaitUntil(() => !paused);
                    if (!timerText.gameObject.activeSelf) timerText.gameObject.SetActive(true);
                    timerText.setTimeText(workoutSeconds);
                }

                exerciseText.text = "Rest";
                int restSeconds = currentWorkout.globalRestTime;
                backgroundImage.color = restColor;

                timerText.setTimeText(restSeconds);
                roundBeep.Play();
                yield return new WaitForSecondsRealtime(roundBeep.clip.length);

                while (restSeconds > 0)
                {
                    if (!pauseButton.interactable) pauseButton.interactable = true;
                    yield return new WaitForSecondsRealtime(1.0f);
                    restSeconds--;

                    if (paused) yield return new WaitUntil(() => !paused);
                    timerText.setTimeText(restSeconds);                    
                }
            }            
        }

        PanelNavigator.Instance.goToPanel(0);
    }
}
