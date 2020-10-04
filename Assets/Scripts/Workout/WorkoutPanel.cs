using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkoutPanel : MonoBehaviour
{
    public RectTransform mainPanelInterface;
    public SubPanelNavigator subPanelNavigator;
    public ListController listController;
    public RectTransform contentTransform;
    public RectTransform workoutSetupContent;

    public HorizontalOrVerticalLayoutGroup optionsLayout;

    public IncrementOption optionPrefab;
    private IncrementOption incrementOption;

    public TMP_Text versionText;

    //TODO Get the text components from the options gameobjects
    [Header("Number Fields")]
    public TMP_Text setsText;
    public TMP_Text repsText;
    
    [Header("Time Fields")]
    public Timetext workText;
    public Timetext restText;

    //TODO make an options script / prefab?
    [Header("Options")]
    public GameObject setsOption;
    public GameObject workOption;
    public GameObject restOption;
    public GameObject repsOption;

    [Header("Buttons")]
    public Button timeModeButton;
    public Button repsModeButton;

    public GameObject totalTimeParent;  //Parent object
    public Timetext totalTimeText;      //Total round time    

    //Used for inspector button functions
    //TODO: uppercase enum (change inspector calls)
    public enum optionType {
        work,   //Work time
        rest,   //Rest time
        sets,   //Set amount
        reps    //Reps amount
    };

    public enum setupState
    {
        Configuration,          //Set workout time and reps
        SelectExerciseMode,     //Set exercise mode
        SelectExercises,        //Select exercises from list
        EvaluateExerciseMode,    //Decide what to do with selected mode
        StartWorkout            //Navigate to workout panel and start
    }
    [HideInInspector]
    public setupState workoutSetupState;

    public enum WorkoutMode
    {
        time,
        reps
    }
    [HideInInspector]
    public WorkoutMode workoutMode;

    public enum ExerciseMode
    {
        Random,     //Random exercise on each next exercise
        Selection,  //Make a selection of exercises yourself
        Rounds      //Only define a number of rounds per set
    }

    [Serializable]
    public class Workout {
        public string name = "Workout"; //Workout name
        public ExerciseMode exerciseMode;
        public WorkoutMode workoutMode;

        //public List<ExerciseDataItem> exercises;

        public Dictionary<string, ExerciseDataItem> exercises; //Exercise data by exercise name
        public int globalWorkTime;  //Global time in seconds for each exercise
        public int globalRestTime;  //Global time in seconds for each exercise
        public int globalSets;      //Sets

        public int globalReps;      //Optional global reps for each exercise
        public int roundsPerSet;    //Optional rounds per set when no exercises specified

        public Workout(WorkoutMode mode)
        {
            exercises = new Dictionary<string, ExerciseDataItem>();
            this.workoutMode = mode;
        }

        public int getTotalWorkoutTimeInSeconds(int? exerciseCount = null)
        {
            //An intermediary value so you can base the returned value on some other number
            int exerciseNumber = exerciseCount ?? exercises.Count;

            //We can just add work and rest time together as each exercise will have rest with multiple sets
            //But we need to substract one rest time as the last set will not have rest, it will finish
            int subtractTime = 0;
            if (exerciseNumber > 0) subtractTime = globalRestTime;
            return (((globalWorkTime + globalRestTime) * exerciseNumber) * globalSets ) - subtractTime;
        }
    }
    
    [Header("Step Increments")]
    public int workStep = 15;
    public int restStep = 15;
    public int repStep = 1;
    public int setStep = 1;

    public List<Workout> workouts;
    [HideInInspector] public Workout newWorkout;     //Current configurable workout

    private Color pressedButtonColor;
    private Color originalButtonColor;
    private bool originalButtonColorSet;

    private void Start()
    {
        versionText.text = "v" + Application.version;
    }

    private void OnEnable()
    {
        createWorkout();
    }

    /// <summary>
    /// Create new workout and start configuration
    /// </summary>
    public void createWorkout() {

        totalTimeParent.gameObject.SetActive(false);
        listController.hideList("exercises");

        workoutSetupState = setupState.Configuration;

        newWorkout = createNewWorkout();
        //TODO: Enable in the future?
        //workouts.Add(newWorkout);
        //subPanelNavigator.goToPanel("WorkoutSetup");
        workoutSetupContent.gameObject.SetActive(true);

        workText.setTimeText(newWorkout.globalWorkTime);
        restText.setTimeText(newWorkout.globalRestTime);
        setsText.text = newWorkout.globalSets.ToString();
        repsText.text = newWorkout.globalReps.ToString();

        setWorkoutMode(workoutMode);
    }

    public void resetWorkout()
    {
        MobileDialog.Instance.show(MobileDialog.ButtonMode.AcceptDismiss, "Reset Workout", "Are you sure you want to reset the workout values?", () =>
        {
            MobileDialog.Instance.abortAndClose();
            createWorkout();

            doValueAnimation(workText.transform);
            doValueAnimation(restText.transform);
            doValueAnimation(setsText.transform);
            doValueAnimation(repsText.transform);

        });
    }

    /// <summary>
    /// Function for use in inspector
    /// </summary>
    /// <param name="type"></param>
    public void setWorkoutMode(string type)
    {
        WorkoutMode enumType = (WorkoutMode)Enum.Parse(typeof(WorkoutMode), type.ToLower());
        setWorkoutMode(enumType);        
    }

    private void setWorkoutMode(WorkoutMode mode)
    {
        workoutMode = mode;
        newWorkout.workoutMode = workoutMode;

        optionsLayout.transform.localScale = new Vector2(1, 0);

        switch (workoutMode)
        {
            case WorkoutMode.time:                
                timeModeButton.gameObject.SetActive(false);
                repsModeButton.gameObject.SetActive(true);
                workOption.gameObject.SetActive(true);
                restOption.gameObject.SetActive(true);
                repsOption.gameObject.SetActive(false);
                break;
            case WorkoutMode.reps:
                timeModeButton.gameObject.SetActive(true);
                repsModeButton.gameObject.SetActive(false);
                workOption.gameObject.SetActive(false);
                restOption.gameObject.SetActive(false);
                repsOption.gameObject.SetActive(true);
                break;
        }

        optionsLayout.transform.DOScaleY(1, 0.25f);
    }

    /// <summary>
    /// Create new workout with default values
    /// </summary>
    /// <returns></returns>
    private Workout createNewWorkout()
    {
        Workout workout = new Workout(workoutMode);
        workout.globalWorkTime = workStep;
        workout.globalRestTime = restStep;
        workout.globalReps = repStep;
        workout.globalSets = setStep;
        return workout;
    }

    public void nextSetupOption()
    {
        //Switch to new state based on previous state
        switch(workoutSetupState)
        {
            case setupState.Configuration: workoutSetupState        = setupState.SelectExerciseMode; break;
            case setupState.SelectExerciseMode: workoutSetupState   = setupState.EvaluateExerciseMode; break;
            case setupState.SelectExercises: workoutSetupState      = setupState.StartWorkout; break;
        }   

        //Do new state function
        switch(workoutSetupState)
        {
            case setupState.Configuration:          createNewWorkout(); break;
            case setupState.SelectExerciseMode:     selectExerciseMode(); break;
            case setupState.EvaluateExerciseMode:

                switch(newWorkout.exerciseMode)
                {
                    case ExerciseMode.Random:       showSelectRoundsDialog(); break;
                    case ExerciseMode.Rounds:       showSelectRoundsDialog(); break;
                    case ExerciseMode.Selection:    selectExercises(); break;
                }

                break;

            case setupState.StartWorkout:
                if (newWorkout.exercises.Count >= 1) startWorkout();
                else
                {
                    MobilePopup.Instance.toast.show("Select at least 1 exercise");
                }
                break;
        }
    }

    private void showSelectRoundsDialog()
    {
        GameObject instiatedobject = Instantiate(optionPrefab.gameObject);
        MobileDialog.Instance.show(MobileDialog.ButtonMode.AcceptDismiss, "Select Rounds", () =>
        {
            startWorkout();
        })
        .clearContent()
        .addContent(instiatedobject);

        IncrementOption IncrementOption = instiatedobject.GetComponent<IncrementOption>();
        IncrementOption.onValueChanged.AddListener(onIncrementChanged);
        IncrementOption.init("rounds per set", 2);
    }

    private void onIncrementChanged(int value)
    {       
        newWorkout.roundsPerSet = value;
    }

    private void selectExerciseMode()
    {
        PanelNavigator.Instance.currentSubPanelNavigator.goToPanel("ExerciseMode");
    }

    private void selectExercises()
    {
        workoutSetupState = setupState.SelectExercises;
        PanelNavigator.Instance.currentSubPanelNavigator.goToPanel("WorkoutExercises");
        
        totalTimeParent.gameObject.SetActive(true);
        totalTimeText.setTimeText(0);
        workoutSetupContent.gameObject.SetActive(false);
        listController.showList("exercises", onClickExerciseItem);
        //TODO: Doesnt appear to work, how to sync rect transform in other canvas?
        //TODO: For now the prefab is just scaled to fit the screen correctly...
        //listController.setListBoundaries("exercises", contentTransform);
    }

    private void startWorkout()
    {
        //TODO: make dialog close itself?
        MobileDialog.Instance.close();

        listController.hideList("exercises");
        subPanelNavigator.goToPanel("WorkoutProgress");
        WorkoutProgress.Instance.startWorkout(newWorkout);
    }

    public void plus(string type)
    {
        optionType enumType = (optionType) Enum.Parse(typeof(optionType), type.ToLower());
        switch (enumType)
        {
            case optionType.work:                
                workText.setTimeText(newWorkout.globalWorkTime += workStep);
                doValueAnimation(workText.transform);
                break;
            case optionType.rest: 
                restText.setTimeText(newWorkout.globalRestTime += restStep);
                doValueAnimation(restText.transform);
                break;
            case optionType.sets: 
                setsText.text = (newWorkout.globalSets += setStep).ToString();
                doValueAnimation(setsText.transform);
                break;
            case optionType.reps: 
                repsText.text = (newWorkout.globalReps += repStep).ToString();
                doValueAnimation(repsText.transform);
                break;
        }
    }

    public void min(string type)
    {
        optionType enumType = (optionType)Enum.Parse(typeof(optionType), type.ToLower());
        switch (enumType)
        {
            case optionType.work:
                if ((newWorkout.globalWorkTime - workStep) >= workStep)
                {
                    workText.setTimeText(newWorkout.globalWorkTime -= workStep);
                    doValueAnimation(workText.transform);
                }
                break;
            case optionType.rest:
                if ((newWorkout.globalRestTime - restStep) >= restStep)
                {
                    restText.setTimeText(newWorkout.globalRestTime -= restStep);
                    doValueAnimation(restText.transform);
                }
                break;
            case optionType.sets:
                if ((newWorkout.globalSets - setStep) > 0 || (newWorkout.globalSets - setStep) == setStep)
                {
                    setsText.text = (newWorkout.globalSets -= setStep).ToString();
                    doValueAnimation(setsText.transform);
                }
                break;
            case optionType.reps:
                if ((newWorkout.globalReps - repStep) > 0 || (newWorkout.globalReps - repStep) == repStep)
                {
                    repsText.text = (newWorkout.globalReps -= repStep).ToString();
                    doValueAnimation(repsText.transform);
                }
                break;
        }
    }

    /// <summary>
    /// Toggle adding or removing exercises to the new workout
    /// </summary>
    /// <param name="item"></param>
    private void onClickExerciseItem(ListViewItem item)
    {
        Image buttonImage = item.button.GetComponent<Image>();
        if (!originalButtonColorSet)
        {
            originalButtonColor = buttonImage.color;
            pressedButtonColor = new Color(
                originalButtonColor.r / 2,
                originalButtonColor.g / 2,
                originalButtonColor.b / 2, 
                originalButtonColor.a);
            originalButtonColorSet = true;
        }

        ExerciseItem exerciseItem = (ExerciseItem)item;
        bool itemRemoved = newWorkout.exercises.ContainsKey(exerciseItem.data.name);
        if(itemRemoved)
        {
            newWorkout.exercises.Remove(exerciseItem.data.name);
            buttonImage.color = originalButtonColor;
        }
        else
        {
            newWorkout.exercises[exerciseItem.data.name] = exerciseItem.data;
            buttonImage.color = pressedButtonColor;
        }
        
        totalTimeText.setTimeText(newWorkout.getTotalWorkoutTimeInSeconds());
        doValueAnimation(totalTimeText.transform);
        //MobilePopup.Instance.toast.show(message);
    }

    private void doValueAnimation(Transform transform)
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector2(0.25f, 0.25f), 0.15f);
    }
    
}
