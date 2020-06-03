using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AppManager deals with all kinds of specific app behaviour
/// It's main purpose is to call other classes and functions
/// </summary>
public class AppManager : Singleton<AppManager>
{
    public WorkoutPanel workoutPanel;

    public UnityEvent onBackButtonPressed;
    public KeyCode backButton = KeyCode.Escape;

    //public Color androidNavBarColor;
    //public Color androidStatusBarColor;

    private void Awake()
    {
        Screen.fullScreen = false;
        //FIXME: Exception: Field currentActivity or type signature  not found
        //AndroidUtils.setNavigationBarColor(androidNavBarColor);
        //AndroidUtils.setStatusBarColor(androidStatusBarColor);
    }

    void Start()
    {
        //onBackButtonPressed
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        backButton = KeyCode.Escape;
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(backButton)) backButtonPressed();
    }

    public void backButtonPressed()
    {
        onBackButtonPressed.Invoke();
        if (WorkoutProgress.Instance.isInProgress())
        {
            //TODO: Solve singleton abuse...
            WorkoutProgress.Instance.pause();
            MobileDialog.Instance.show(MobileDialog.ButtonMode.AcceptDismiss, "Stop Workout", "Are you sure you want to stop the current workout?", () =>
            {
                WorkoutProgress.Instance.cancel();
                MobileDialog.Instance.close();
            },
            () =>
            {
                WorkoutProgress.Instance.play();
                MobileDialog.Instance.close();
            });
        }
        else
        {
            PanelNavigator.Instance.goToPreviousInHistory();
            //goBackInHistory();
        }        
    }

    public void goBackInHistory()
    {
        bool previousPanel = false;
        if (PanelNavigator.Instance.currentSubPanelNavigator != null)
        {
            //FIXME: Quick fix for list that is not really displayed in a next panel...
            if (PanelNavigator.Instance.currentSubPanelNavigator.getCurrentIndex() == 0)
            {
                workoutPanel.createWorkout();
            }
            else
            {
                //Is a subpanel assigned and can we go back in history
                previousPanel = PanelNavigator.Instance.currentSubPanelNavigator.goToPreviousInHistory();
            }
        }
        else
        {
            //If not sub panel history then check top panels
            if (!previousPanel) previousPanel = PanelNavigator.Instance.goToPreviousInHistory();
            else Application.Quit();
        }
    }
}
