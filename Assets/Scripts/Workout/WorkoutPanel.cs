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

    public TMP_Text setsText;
    public Timetext workText;
    public Timetext restText;

    //Used for inspector button functions
    //TODO: uppercase enum (change inspector calls)
    public enum optionType { work, rest, sets };

    public enum setupState
    {
        Configuration,      //Set workout time and reps
        SelectExercises,    //Select exercises from list
        StartWorkout        //Navigate to workout panel and start
    }

    public setupState state;

    [Serializable]
    public class Workout {
        public string name = "Workout"; //Workout name

        //public List<ExerciseDataItem> exercises;

        public Dictionary<string, ExerciseDataItem> exercises; //Exercise data by exercise name
        public int globalWorkTime;  //Optional global time in seconds for each exercise
        public int globalRestTime;  //Optional global time in seconds for each exercise
        public int globalReps;      //Optional global reps for each exercise
        public int globalSets;      //Optional global reps for each exercise

        public Workout()
        {
            exercises = new Dictionary<string, ExerciseDataItem>();
        }
    }

    //Step increments
    public int workStep = 15;
    public int restStep = 15;
    public int repStep = 1;
    public int setStep = 1;

    public List<Workout> workouts;
    private Workout newWorkout;     //Current configurable workout

    private Color pressedButtonColor;
    private Color originalButtonColor;
    private bool originalButtonColorSet;

    private void OnEnable()
    {
        createWorkout();
    }

    /// <summary>
    /// Create new workout and start configuration
    /// </summary>
    public void createWorkout() {

        listController.hideList("exercises");
        state = setupState.Configuration;

        newWorkout = createNewWorkout();
        //TODO: Enable in the future?
        //workouts.Add(newWorkout);
        //subPanelNavigator.goToPanel("WorkoutSetup");
        workoutSetupContent.gameObject.SetActive(true);

        workText.setTimeText(newWorkout.globalWorkTime);
        restText.setTimeText(newWorkout.globalRestTime);
        setsText.text = newWorkout.globalSets.ToString();
    }

    /// <summary>
    /// Create new workout with default values
    /// </summary>
    /// <returns></returns>
    private Workout createNewWorkout()
    {
        Workout workout = new Workout();
        workout.globalWorkTime = workStep;
        workout.globalRestTime = restStep;
        workout.globalReps = repStep;
        workout.globalSets = setStep;
        return workout;
    }

    public void nextSetupOption()
    {
        //Switch to new state based on previous state
        switch(state)
        {
            case setupState.Configuration: state = setupState.SelectExercises; break;
            case setupState.SelectExercises: state = setupState.StartWorkout; break;
        }

        //Do new state function
        switch(state)
        {
            case setupState.Configuration: createNewWorkout(); break;
            case setupState.SelectExercises: selectExercises(); break;
            case setupState.StartWorkout:
                if (newWorkout.exercises.Count >= 1) startWorkout();
                else MobilePopup.Instance.toast.show("Selecteer minstens 1 oefening");
                break;
        }
    }

    private void selectExercises()
    {
        workoutSetupContent.gameObject.SetActive(false);
        listController.showList("exercises", onClickExerciseItem);
        //TODO: Doesnt appear to work, how to sync rect transform in other canvas?
        //TODO: For now the prefab is just scaled to fit the screen correctly...
        //listController.setListBoundaries("exercises", contentTransform);
    }

    private void startWorkout()
    {
        listController.hideList("exercises");
        subPanelNavigator.goToPanel("WorkoutProgress");
        WorkoutProgress.Instance.startWorkout(newWorkout);
    }

    public void plus(string type)
    {        
        optionType enumType = (optionType) Enum.Parse(typeof(optionType), type.ToLower());
        switch (enumType)
        {
            case optionType.work : workText.setTimeText(newWorkout.globalWorkTime += workStep); break;
            case optionType.rest: restText.setTimeText(newWorkout.globalRestTime += restStep); break;
            case optionType.sets: setsText.text = (newWorkout.globalSets += setStep).ToString(); break;
        }
    }

    public void min(string type)
    {        
        optionType enumType = (optionType)Enum.Parse(typeof(optionType), type.ToLower());
        switch (enumType)
        {
            case optionType.work: if ((newWorkout.globalWorkTime - workStep) >= workStep) workText.setTimeText(newWorkout.globalWorkTime -= workStep); break;
            case optionType.rest: if ((newWorkout.globalRestTime - restStep) >= restStep) restText.setTimeText(newWorkout.globalRestTime -= restStep); break;
            case optionType.sets: if ((newWorkout.globalSets - setStep) > 0 || (newWorkout.globalSets - setStep) == setStep) setsText.text = (newWorkout.globalSets -= setStep).ToString(); break;
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

        //MobilePopup.Instance.toast.show(message);       
    }
    
}
