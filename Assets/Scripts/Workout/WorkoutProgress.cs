using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkoutProgress : Singleton<WorkoutProgress>
{
    public TMP_Text setText;
    public TMP_Text exerciseText;
    public Timetext timerText;

    public Image nextImage;
    private Color nextImageColor; //For lerping color
    private Color nextImageOriginalColor; //For lerping color
    public TMP_Text nextExerciseText;

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

    private string getReadyString = "GET READY";

    private void OnEnable()
    {
        nextImageOriginalColor = nextImage.color;
    }

    private void Update()
    {
        backgroundImage.color = Color.Lerp(backgroundImage.color, backgroundColor, Time.deltaTime * 1);
        nextImage.color = Color.Lerp(backgroundImage.color, nextImageColor, Time.deltaTime * 1);
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
        setText.text = "";
        exerciseText.text = getReadyString;
        timerText.setText("");

        backgroundColor = readyColor;
        nextImageColor = readyColor;
        int seconds = countdownSeconds;
        while(seconds > 0)
        {
            secondBeep.Play();
            timerText.setTimeText(seconds);
            yield return new WaitForSecondsRealtime(1.0f);
            seconds--;
        }

        exerciseText.text = "";
        timerText.setText("");

        StartCoroutine("workoutLoop");
    }

    private IEnumerator workoutLoop()
    {
        List<KeyValuePair<string, ExerciseDataItem>> exerciseList = currentWorkout.exercises.ToList();
        for (int set = 0; set < currentWorkout.globalSets; set++)
        {
            pauseButton.interactable = false;             
            setText.text = (set + 1).ToString() + " / " + currentWorkout.globalSets;

            int exerciseCount = 0;
            bool setRest = set > 0; //Reset between sets if we passed set 0
            foreach (KeyValuePair<string, ExerciseDataItem> pair in currentWorkout.exercises)
            {
                bool notFirstExercise = exerciseCount > 0;
                bool notLastExercise = exerciseCount != currentWorkout.exercises.Count;
                bool roundRest = (notFirstExercise && notLastExercise);
               
                if (setRest || (notFirstExercise && notLastExercise))
                {
                    exerciseText.text = "Rest";
                    nextExerciseText.text = exerciseList[exerciseCount].Value.name;
                    int restSeconds = currentWorkout.globalRestTime;
                    backgroundColor = restColor;
                    nextImageColor = nextImageOriginalColor;

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

                nextExerciseText.text = "";

                exerciseCount++;
                ExerciseDataItem exercise = pair.Value;

                exerciseText.text = exercise.name.ToUpper();

                int workoutSeconds = currentWorkout.globalWorkTime;
                backgroundColor = workColor;
                nextImageColor = workColor;

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
                }

                roundBeep.Play();
                yield return new WaitForSecondsRealtime(roundBeep.clip.length);               
            }
        }

        workoutFinished();
    }

    private void workoutFinished()
    {
        //Images
        backgroundColor = finishedColor;
        nextImageColor = finishedColor;

        //Text
        exerciseText.text = "Finished!";
        timerText.setText("");

        //Buttons
        repeatButton.gameObject.SetActive(true);
        repeatButton.interactable = true;
        stopButton.interactable = false;
        playButton.interactable = false;
        pauseButton.interactable = false;
    }
}
