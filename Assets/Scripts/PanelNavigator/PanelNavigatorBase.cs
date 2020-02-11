using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Abstract base class for a panel navigator
/// PanelNavigator      : singleton main panel navigator, only once in scene
/// SubPanelNavigator   : sub panel navigator, multiple in scene beneath panel navigator in hierarchy
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class PanelNavigatorBase : MonoBehaviour
{
    public SubPanelNavigator currentSubPanelNavigator;

    [Header("Properties")]
    public int startIndex = 0;
    public int maxHistorySize = 10;
    public bool enableEscapeKey = true;
    public ScreenOrientation defaultOrientation = ScreenOrientation.Portrait;
    public bool useDefaultOrientation = false;
    public KeyCode backButton = KeyCode.Escape;
    public bool goToStartIndex;

    [Header("Callbacks")]
    public UnityEvent onStartCallback;
    public UnityEvent onShowPanelGlobalCallback;
    public UnityEvent onHidePanelGlobalCallback;
    public Action<string> onHardLockedAction;

    private Canvas canvas;

    [System.Serializable]
    public class PanelEntry
    {
        public string name;
        public GameObject panel;
        public ScreenOrientation orientation = ScreenOrientation.Portrait;
        public bool includeInHistory = false;
        public bool backButtonLock = false;

        //https://learn.unity.com/tutorial/optimizing-unity-ui#5c7f8528edbc2a002053b59f
        public Canvas extraCanvas;
    }

    [Header("Panels")]
    public GameObject globalPanel;
    public PanelEntry[] panelEntries;

    //Privates
    private Stack<int> history;
    private int currentIndex = 0;
    private int previousIndex = 0;
    private bool isLocked = false;

    void Awake()
    {
        //Set the back button keycode on build otherwise it won't work properly
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        backButton = KeyCode.Escape;
#endif        

        canvas = GetComponent<Canvas>();
        if (useDefaultOrientation) Screen.orientation = defaultOrientation;
        if (startIndex > panelEntries.Length - 1)
        {
            Debug.LogWarningFormat("Starting panel index {0} is out of bounds!", startIndex);
            return;
        }

        history = new Stack<int>();
        if(goToStartIndex) goToPanel(startIndex);
        if (getPanel(currentIndex).includeInHistory) history.Push(startIndex);
    }

    /// <summary>
    /// Use this to set custom start behaviour
    /// For example navigating to a panel with extra functionality
    /// </summary>
    void Start()
    {
        onStartCallback.Invoke();
    }

    private void OnEnable()
    {
        if (canvas != null) canvas.enabled = true;
        if (history == null) history = new Stack<int>();
    }

    private void OnDisable()
    {
        if (canvas != null) canvas.enabled = false;
    }

    private void Update()
    {
        if (enableEscapeKey && Input.GetKeyDown(backButton)) goToPreviousInHistory();
    }

    /// <summary>
    /// Go to the previous panel in history
    /// </summary>
    public bool goToPreviousInHistory()
    {
        if (history.Count > 0 && history.Peek() > -1 && !isLocked)
        {
            int previousIndex = history.Pop();
            return goToPanel(previousIndex);
        }
        return false;
    }

    /// <summary>
    /// Go to panel by name
    /// </summary>
    /// <param name="name"></param>
    public void goToPanel(string name)
    {
        for (int i = 0; i < panelEntries.Length; i++)
        {
            if (panelEntries[i].name == name) goToPanel(i);
            else panelEntries[i].panel.SetActive(false);
        }
    }

    /// <summary>
    /// Go to previous panel, checks index and if locked
    /// </summary>
    public bool goToPrevious(bool unlock = false)
    {
        if (unlock) setLock(false);
        int index = (currentIndex - 1);
        if (index >= panelEntries.Length && !isLocked)
        {
            return goToPanel(index);
        }
        return false;
    }

    /// <summary>
    /// Go to next panel, checks index and if locked
    /// </summary>
    public bool goToNext(bool unlock = false)
    {
        if (unlock) setLock(false);
        int index = (currentIndex + 1);
        if (index <= panelEntries.Length && !isLocked)
        {
            return goToPanel(index);
        }
        return false;
    }

    /// <summary>
    /// Get current index
    /// </summary>
    /// <returns></returns>
    public int getCurrentIndex()
    {
        return currentIndex;
    }

    /// <summary>
    /// Get panel entry by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public PanelEntry getPanel(string name)
    {
        for (int i = 0; i < panelEntries.Length; i++)
        {
            //Debug.Log(panelEntries[i].name + " == " + name);
            if (panelEntries[i].name == name) return panelEntries[i];
        }
        return null;
    }

    /// <summary>
    /// Get the current panel entry
    /// </summary>
    /// <returns></returns>
    public PanelEntry getPanel(int index)
    {
        return panelEntries[index];
    }

    /// <summary>
    /// Apply current panel orientation
    /// </summary>
    private void applyPanelSettings(PanelEntry entry)
    {
        Screen.orientation = entry.orientation;
        if (entry.backButtonLock) isLocked = true;
    }

    /// <summary>
    /// Set 'soft' locked, can only be navigated away with goToPanel
    /// </summary>
    /// <param name="locked"></param>
    public void setLock(bool locked)
    {
        isLocked = locked;
    }

    /// <summary>
    /// Go to panel by index, does NOT check locked
    /// And will also reset locked status
    /// </summary>
    /// <param name="index"></param>
    public bool goToPanel(int index, bool disableOthers = true)
    {
        setLock(false);
        //Debug.Log(string.Format("Active panel at index {0}", index));

        //The previous panel, callbacks
        previousIndex = currentIndex;
        PanelEntry currentEntry = getPanel(previousIndex);
        if (onHidePanelGlobalCallback != null) onHidePanelGlobalCallback.Invoke();
        //if (currentEntry.onHideCallback != null) currentEntry.onHideCallback.Invoke();

        //Activating the panel here so coroutines can be started from onShowCallback etc.
        //Disable others and show next panel
        if (disableOthers) disableAllPanels();
        panelEntries[index].panel.SetActive(true);
        if (panelEntries[index].extraCanvas != null)
            panelEntries[index].extraCanvas.enabled = true;

        //The next panel, callbacks
        currentIndex = index;
        PanelEntry nextEntry = getPanel(currentIndex);
        //if (nextEntry.onShowCallback != null) nextEntry.onShowCallback.Invoke();
        if (onShowPanelGlobalCallback != null) onShowPanelGlobalCallback.Invoke();

        //Apply panel orientation/lock
        applyPanelSettings(nextEntry);
        if (currentEntry.includeInHistory && index != previousIndex && history.Count < maxHistorySize) history.Push(previousIndex);
        return true;
    }

    /// <summary>
    /// Disable all the panels
    /// </summary>
    private void disableAllPanels()
    {
        for (int i = 0; i < panelEntries.Length; i++)
        {
            panelEntries[i].panel.SetActive(false);
            if (panelEntries[i].extraCanvas != null) panelEntries[i].extraCanvas.enabled = false;
        }
    }
}
