using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Shows a dialog with dynamically added buttons
/// </summary>
[RequireComponent(typeof(Button))]
public class MobileDialog : Singleton<MobileDialog>
{
    public enum ButtonMode {
        AcceptDismiss,  //Show both buttons
        Accept,         //Show accept button
        Dismiss,        //Show dismiss button
        None            //Show no buttons
    };

    /// <summary>
    /// List of prefabs can be extended with your own prefabs
    /// Must contains the buttonPrefabKey key/prefab
    /// </summary>
    [Serializable]
    public class CustomPrefab
    {
        public string name;
        public GameObject prefab;
    }
    public List<CustomPrefab> customPrefabs;
    public Dictionary<string, GameObject> _customPrefabs;

    //Public

    [Header("Objects")]
    public GameObject dialogParent;
    public string buttonPrefabKey = "button";
    public string descriptionPrefabKey = "description";

    [Header("Title")]
    public TMP_Text titleText;    

    [Header("Background")]
    public Image backgroundImage;
    public Button backgroundButton;

    [Header("Layouts")]
    public HorizontalOrVerticalLayoutGroup contentLayout;
    public HorizontalOrVerticalLayoutGroup buttonLayout;

    //Default accept/dismiss strings
    [SerializeField] private string acceptDefault;
    [SerializeField] private string dismissDefault;

    [HideInInspector] public bool downloading;

    //Private

    private RectTransform buttonLayoutRectransform;
    private UnityWebRequest webRequest;

    void Start()
    {
        buttonLayoutRectransform = buttonLayout.GetComponent<RectTransform>();
        _customPrefabs = new Dictionary<string, GameObject>();
        foreach (CustomPrefab customPrefab in customPrefabs)
        {
            _customPrefabs.Add(customPrefab.name, customPrefab.prefab);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) abortAndClose();
    }

    //Show functions

    public MobileDialog show(string title = "", string description = "")
    {
        //Reset background image
        setBackground(null, 0);

        //Enable text elements
        titleText.text = title;
        setDescriptionContent(description);
        dialogParent.SetActive(true);
        return this;
    }

    public MobileDialog show(ButtonMode buttonMode, string title = "", string description = "", UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {
        //Reset background
        setBackground(null, 0);

        //Set button mode
        setButtonMode(buttonMode, positiveCallback, negativeCallback);

        //Disable and enable elements
        clearContent();
        titleText.text = title;
        titleText.gameObject.SetActive(true);
        setDescriptionContent(description);
        dialogParent.SetActive(true);

        return this;
    }

    public void show(string title, string description, UnityWebRequest request, bool abortable = true, UnityAction negativeCallback = null)
    {
        if (abortable) createCancelButton(dismissDefault, negativeCallback);
        else setButtonMode(ButtonMode.None);

        titleText.text = title;
        dialogParent.SetActive(true);
        setDescriptionContent(description);
        webRequest = request;
        //StartCoroutine(progressRoutine());
    }

    //Clear functions

    public MobileDialog clearButtons()
    {
        foreach (Transform child in buttonLayout.transform)
        {
            Destroy(child.gameObject);
        }
        return this;
    }

    public MobileDialog clearAll()
    {
        clearButtons();
        clearContent();
        return this;
    }

    private MobileDialog clearContent()
    {
        foreach (Transform child in contentLayout.transform)
        {
            child.gameObject.SetActive(false);
        }
        return this;
    }

    public MobileDialog setDescriptionContent(string description)
    {
        clearContent();
        GameObject buttonContainer = Instantiate(_customPrefabs[descriptionPrefabKey], contentLayout.transform);
        TMP_Text tmp = buttonContainer.GetComponentInChildren<TMP_Text>();
        tmp.text = description;
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonLayout.GetComponent<RectTransform>());
        return this;
    }

    public MobileDialog addContent(string prefabName = null, bool rebuild = true)
    {
        GameObject buttonContainer = Instantiate(_customPrefabs[prefabName], contentLayout.transform);
        if (rebuild) LayoutRebuilder.ForceRebuildLayoutImmediate(buttonLayout.GetComponent<RectTransform>());
        return this;
    }

    // Buttons

    public MobileDialog addButton(string text, UnityAction UnityAction, string prefabName = null, bool rebuild = true)
    {
        if (prefabName == null) prefabName = buttonPrefabKey;
        GameObject buttonContainer = Instantiate(_customPrefabs[prefabName], buttonLayout.transform);
        Button button = buttonContainer.GetComponentInChildren<Button>();
        buttonContainer.GetComponentInChildren<TMP_Text>().text = text;
        button.onClick.AddListener(UnityAction);
        buttonContainer.SetActive(true);
        if (rebuild) LayoutRebuilder.ForceRebuildLayoutImmediate(buttonLayout.GetComponent<RectTransform>());
        return this;
    }

    private void setButtonMode(ButtonMode buttonPreset, UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {
        setBackgroundCallback(negativeCallback);
        switch (buttonPreset)
        {
            case ButtonMode.AcceptDismiss:
                clearButtons()
                .addButton(acceptDefault, positiveCallback ?? abortAndClose)
                .addButton(dismissDefault, negativeCallback ?? abortAndClose);
                setBackgroundCallback(negativeCallback);
                break;
            case ButtonMode.Dismiss:
                UnityAction cancel = negativeCallback ?? abortAndClose;
                clearButtons().addButton(dismissDefault ?? dismissDefault, cancel);
                setBackgroundCallback(cancel);
                break;
            case ButtonMode.Accept: createCancelButton(acceptDefault, positiveCallback); break;
            case ButtonMode.None: clearButtons(); break;
        }
    }

    private void createCancelButton(string text = null, UnityAction cancelCallback = null)
    {
        UnityAction cancel = cancelCallback ?? abortAndClose;
        clearButtons()
        .addButton(text ?? dismissDefault, cancel);
        setBackgroundCallback(cancel);
    }

    // Callback

    private void setBackgroundCallback(UnityAction backgroundCallback)
    {
        UnityAction callback = backgroundCallback ?? abortAndClose;
        backgroundButton.onClick.RemoveAllListeners();
        backgroundButton.onClick.AddListener(callback);
    }

    // Background

    public void setBackground(Sprite sprite, float alpha = 0.33f)
    {
        backgroundImage.sprite = sprite;
    }

    // Request
    //TODO: Move of all of this into a seperate prefab thing that can be added to the dialog content

    //Loading graphics
    //private float progressBarHeight;
    //private Vector2 targetSizeDelta;
    //void Start()
    //{
    //    //progressBarHeight = progressImage.rectTransform.sizeDelta.y;
    //    //targetSizeDelta = progressImage.rectTransform.sizeDelta;
    //    private IEnumerator progressRoutine()
    //{
    //    downloading = true;
    //    while (tryGetDownloadProgress() < 1.0f)
    //    {
    //        setProgress(tryGetDownloadProgress());
    //        yield return null;
    //    }
    //    downloading = false;
    //    dialogParent.SetActive(false);
    //}
    //private float tryGetDownloadProgress()
    //{
    //    try { return webRequest.downloadProgress; }
    //    catch (ArgumentException e) { return 1.0f; }
    //}
    //private void setProgress(float downloadProgress)
    //{
    //    descriptionText.text = String.Format("{0}%", Math.Floor(downloadProgress * 100), 1);
    //    //TODO: progress bar
    //    //progressImage.rectTransform.sizeDelta = new Vector2((sizeDeltaTarget.x / 100) * downloadProgress, progressHeight);
    //}

    public void abortAndClose()
    {
        bool abort = downloading && webRequest != null;
        if (abort)
        {
            //When using Abort() UnityWebRequest will interpret this as a network error
            webRequest.Abort();
            downloading = webRequest.isDone;
        }
        toggle(false);
    }

    // Other

    public void toggle(bool force = false)
    {
        enabled = force ? force : !gameObject.activeSelf;
        dialogParent.SetActive(enabled);
    }

    public bool isEnabled()
    {
        return dialogParent.activeSelf;
    }
}
