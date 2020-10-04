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

    private MobileDialog mobileDialog;
    private PanelNavigator panelNavigator;
    private WorkoutProgress workoutProgress;

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
        if (mobileDialog == null)
        {
            mobileDialog = MobileDialog.Instance;
            workoutProgress = WorkoutProgress.Instance;
        }

        onBackButtonPressed.Invoke();

        if(mobileDialog.dialogObject.activeSelf)
        {
            mobileDialog.close();
        }
        else if (WorkoutProgress.Instance.isInProgress())
        {
            //TODO: Solve singleton abuse...
            WorkoutProgress.Instance.pause();
                mobileDialog.show(MobileDialog.ButtonMode.AcceptDismiss, "Stop Workout", "Are you sure you want to stop the current workout?", () =>
            {
                workoutProgress.cancel();
                mobileDialog.close();
            },
            () =>
            {
                workoutProgress.play();
                mobileDialog.close();
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
        if (panelNavigator == null) panelNavigator = PanelNavigator.Instance;

        bool previousPanel = false;
        if (panelNavigator.currentSubPanelNavigator != null)
        {
            //FIXME: Quick fix for list that is not really displayed in a next panel...
            if (panelNavigator.currentSubPanelNavigator.getCurrentIndex() == 0)
            {
                workoutPanel.createWorkout();
            }
            else
            {
                //Is a subpanel assigned and can we go back in history
                previousPanel = panelNavigator.currentSubPanelNavigator.goToPreviousInHistory();
            }
        }
        else
        {
            //If not sub panel history then check top panels
            if (!previousPanel) previousPanel = panelNavigator.goToPreviousInHistory();
            else Application.Quit();
        }
    }
}
