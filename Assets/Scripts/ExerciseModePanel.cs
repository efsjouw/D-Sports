using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExerciseModePanel : MonoBehaviour
{
    public WorkoutPanel workoutPanel;
    public WorkoutPanel.ExerciseMode exerciseModeDefault = WorkoutPanel.ExerciseMode.Rounds;

    [Header("Mode buttons")]
    public Button randomButton;
    public Button selectionButton;
    public Button roundsButton;

    [Header("Mode icons")]
    [SerializeField] Image iconImage;
    [SerializeField] Sprite randomIcon;
    [SerializeField] Sprite selectionIcon;
    [SerializeField] Sprite roundsIcon;

    [Header("Exercise mode")]
    [SerializeField] Transform animationTransform;
    public TMP_Text exerciseModeTitle;
    public TMP_Text exerciseModeText;
    
    private Color originalButtonColor;

    private bool selectionInit;

    private void Awake()
    {
        Image buttonImage = randomButton.GetComponent<Image>();
        originalButtonColor = buttonImage.color;
    }

    private void OnEnable()
    {
        //TODO: Load last set mode from player prefs
        if (!selectionInit) setExerciseMode(exerciseModeDefault);
    }

    public void setExerciseMode(string type)
    {
        WorkoutPanel.ExerciseMode enumType = (WorkoutPanel.ExerciseMode)Enum.Parse(typeof(WorkoutPanel.ExerciseMode), type);
        setExerciseMode(enumType);
    }

    public void setExerciseMode(WorkoutPanel.ExerciseMode exerciseMode)
    {
        setButtonColor(randomButton, false);
        setButtonColor(selectionButton, false);
        setButtonColor(roundsButton, false);

        workoutPanel.newWorkout.exerciseMode = exerciseMode;
        switch (workoutPanel.newWorkout.exerciseMode)
        {
            case WorkoutPanel.ExerciseMode.Random:      setRandomMode(); break;
            case WorkoutPanel.ExerciseMode.Selection:   setSelectionMode(); break;
            case WorkoutPanel.ExerciseMode.Rounds:      setRoundsMode(); break;
        }
    }

    private void setButtonColor(Button button, bool toggle)
    {
        Image buttonImage = button.GetComponent<Image>();
        Color buttonColor = default;
        if (toggle)
        {
            buttonColor = new Color(
            originalButtonColor.r / 2,
            originalButtonColor.g / 2,
            originalButtonColor.b / 2,
            originalButtonColor.a);
        }
        else
        {
            buttonColor = originalButtonColor;
        }
        buttonImage.color = buttonColor;
    }

    private void setRandomMode()
    {
        iconImage.sprite = randomIcon;
        setButtonColor(randomButton, true);
        exerciseModeTitle.text = "Random";
        exerciseModeText.text = "Each next exercise will be picked at random.";
        modeChangeAnimation();
    }

    private void setSelectionMode()
    {
        iconImage.sprite = selectionIcon;
        setButtonColor(selectionButton, true);
        exerciseModeTitle.text = "Selection";
        exerciseModeText.text = "Select exercises from a list by yourself.";
        modeChangeAnimation();
    }

    private void setRoundsMode()
    {
        iconImage.sprite = roundsIcon;
        setButtonColor(roundsButton, true);
        exerciseModeTitle.text = "Rounds";
        exerciseModeText.text = "Give in a number of rounds without specific exercises.";
        modeChangeAnimation();
    }

    private void modeChangeAnimation()
    {
        animationTransform.DOPunchScale(new Vector3(0.15f, 0.15f), 0.25f);
    }
}
