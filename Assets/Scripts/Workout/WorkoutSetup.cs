using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basically had to make this as an intermediary script for onenable
/// TODO: Maybe move WorkoutPanel script to another object...
/// </summary>
public class WorkoutSetup : MonoBehaviour
{
    [SerializeField]
    private WorkoutPanel workoutPanel;

    private void OnEnable()
    {
        //TODO: Stop this from being called 2/3 times?
        //Something to do with WorkoutPanel
        workoutPanel.createWorkout();
    }
}
