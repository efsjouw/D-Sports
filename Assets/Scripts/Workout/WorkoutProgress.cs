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
    public Button playButton;
    public Button repeatButton;
    public Button stopButton;

    public Image backgroundImage;
    private Color backgroundColor; //For lerping color

    public Color readyColor;
    public Color workColor;
    public Color restColor;
    public Color finishedColor;

    public AudioSource secondBeep; //Beep sound for each countdown second
    public AudioSource roundBeep;  //Beep sound after each round

    public int countdownSeconds = 5;

    private WorkoutPanel.Workout currentWorkout;

    //Both true as the workout has not started yet on app start
    private bool paused = true;
    private bool cancelled = true;

    private void Awake()
    {
        getReadyText.text = "GET READY";
    }

    private void Update()
    {
        backgroundImage.color = Color.Lerp(backgroundImage.color, backgroundColor, Time.deltaTime * 1);
    }

    private void togglePausePlayButton()
    {
        pauseButton.gameObject.SetActive(!paused);
        pauseButton.interactable =!paused;
        playButton.gameObject.SetActive(paused);
        playButton.interactable = paused;
    }

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
        AppManager.Instance.backButtonPressed();
    }

    public void play()
    {
        paused = false;
        togglePausePlayButton();
    }

    public void pause()
    {
        paused = true;
        togglePausePlayButton();
    }

    public void restart()
    {
        startWorkout(currentWorkout);
    }

    public void startWorkout(WorkoutPanel.Workout workout)
    {
        paused = false;
        cancelled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        exerciseText.text = "";
        repeatButton.interactable = false;
        pauseButton.interactable = false;
        playButton.interactable = false;
        stopButton.interactable = true;
        currentWorkout = workout;
        StartCoroutine(workoutCountdown());
    }

    private IEnumerator workoutCountdown()
    {
        setText.gameObject.SetActive(false);
        exerciseText.gameObject.SetActive(false);
        getReadyText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);

        backgroundColor = readyColor;
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

            int exerciseCount = 0;
            foreach (KeyValuePair<string, ExerciseDataItem> pair in currentWorkout.exercises)
            {
                exerciseCount++;
                ExerciseDataItem exercise = pair.Value;

                exerciseText.gameObject.SetActive(true);
                exerciseText.text = exercise.name.ToUpper();

                int workoutSeconds = currentWorkout.globalWorkTime;
                backgroundColor = workColor;

                timerText.setTimeText(workoutSeconds);
                roundBeep.Play();
                yield return new WaitForSecondsRealtime(roundBeep.clip.length);

                while (workoutSeconds > 0)
                {
                    if(!pauseButton.interactable) pauseButton.interactable = true;
                    if(workoutSeconds <= 3) secondBeep.Play();

                    timerText.setTimeText(workoutSeconds);
                    yield return new WaitForSecondsRealtime(1.0f);
                    workoutSeconds--;

                    if (paused) yield return new WaitUntil(() => !paused);
                    if (!timerText.gameObject.activeSelf) timerText.gameObject.SetActive(true);                    
                }

                roundBeep.Play();
                yield return new WaitForSecondsRealtime(roundBeep.clip.length);

                if (exerciseCount != currentWorkout.exercises.Count)
                {
                    exerciseText.text = "Rest";
                    int restSeconds = currentWorkout.globalRestTime;
                    backgroundColor = restColor;

                    while (restSeconds > 0)
                    {
                        if (!pauseButton.interactable) pauseButton.interactable = true;
                        if (restSeconds <= 3) secondBeep.Play();

                        timerText.setTimeText(restSeconds);
                        yield return new WaitForSecondsRealtime(1.0f);
                        restSeconds--;

                        if (paused) yield return new WaitUntil(() => !paused);                        
                    }
                }
            }
        }

        workoutFinished();
    }

    private void workoutFinished()
    {
        backgroundColor = finishedColor;
        exerciseText.text = "Finished!";
        timerText.gameObject.SetActive(false);
        repeatButton.gameObject.SetActive(true);
        repeatButton.interactable = true;
        stopButton.interactable = false;
        playButton.interactable = false;
        pauseButton.interactable = false;
    }
}
