using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AppManager deals with all kinds of specific app behaviour
/// It's main purpose is to call other classes and functions
/// </summary>
public class AppManager : Singleton<AppManager>
{
    public UnityEvent onBackButtonPressed;
    public KeyCode backButton = KeyCode.Escape;

    //private void Awake()
    //{
    //    Screen.fullScreen = false;
    //}

    void Start()
    {
        //onBackButtonPressed
    }

    void Update()
    {
        if (Input.GetKeyDown(backButton)) backButtonPressed();
    }

    public void backButtonPressed()
    {
        onBackButtonPressed.Invoke();
        if (WorkoutProgress.Instance.isInProgress()) WorkoutProgress.Instance.cancel();

        bool previousPanel = false;
        if (PanelNavigator.Instance.currentSubPanelNavigator != null)
        {
            //Is a subpanel assigned and can we go back in history
            previousPanel = PanelNavigator.Instance.currentSubPanelNavigator.goToPreviousInHistory();
        }
        else
        {
            //If not sub panel history then check top panels
            if (!previousPanel) previousPanel = PanelNavigator.Instance.goToPreviousInHistory();
            else Application.Quit();
        }
    }
}
