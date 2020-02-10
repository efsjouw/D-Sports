using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Shows a dialog with dynamically added buttons
/// </summary>
[RequireComponent(typeof(Button))]
public class MobileDialog : MonoBehaviour
{

    public enum ButtonPreset { CancelOk, Cancel, Ok, None };
    public enum InputMode { Text, None };

    public GameObject dialogObject;
    public Transform dialogContent;

    public TMP_Text titleText;
    public TMP_Text messageText;

    public InputField textInput;
    public Image backgroundImage;

    [Header("Loading Graphics")]
    public Image progressImage;

    //Loading graphics values (disabled)
    private float progressHeight;
    private Vector2 sizeDeltaTarget;

    [Header("Buttons")]
    public Button backgroundButton;
    public GameObject buttonPrefab;
    public HorizontalLayoutGroup buttonLayout;
    private RectTransform buttonLayoutRect;

    //Current blocking web request, optional
    private UnityWebRequest blockingRequest;

    [HideInInspector]
    public bool downloading;

    //Seperate strings for translation support
    private string okString = "Ok";
    private string cancelString = "Cancel";

    /// <summary>
    /// Enable the dialog in the scene first!
    /// </summary>
    void Start()
    {
        progressHeight = progressImage.rectTransform.sizeDelta.y;
        sizeDeltaTarget = progressImage.rectTransform.sizeDelta;
        buttonLayoutRect = buttonLayout.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) cancelAndClose();
    }

    /// <summary>
    /// Clear all buttons
    /// </summary>
    /// <returns></returns>
    public MobileDialog clearButtons()
    {
        foreach (Transform child in buttonLayout.transform)
        {
            Destroy(child.gameObject);
        }
        return this;
    }

    /// <summary>
    /// Clear all buttons and elements
    /// </summary>
    /// <returns></returns>
    public MobileDialog clear()
    {
        clearButtons();
        disableContents(true);
        return this;
    }

    /// <summary>
    /// Add a button to the layout group
    /// </summary>
    /// <param name="text"></param>
    /// <param name="UnityAction"></param>
    /// <param name="rebuildLayout"></param>
    /// <returns></returns>
    public MobileDialog addButton(string text, UnityAction UnityAction, bool rebuildLayout = true)
    {
        GameObject buttonContainer = Instantiate(buttonPrefab, buttonLayout.transform);
        buttonContainer.SetActive(true); //In case prefab was disabled
        Button button = buttonContainer.GetComponentInChildren<Button>();
        buttonContainer.GetComponentInChildren<TMP_Text>().text = text;
        button.onClick.AddListener(UnityAction);
        if (rebuildLayout) LayoutRebuilder.ForceRebuildLayoutImmediate(buttonLayout.GetComponent<RectTransform>());
        return this;
    }

    /// <summary>
    /// Show the dialog
    /// TODO: Merge/reuse with preset show()?
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public MobileDialog show(string title = "", string message = "")
    {
        //Reset background image
        setBackgroundImage(null, 0);

        //Enable text elements
        titleText.text = title;
        messageText.text = message;
        dialogObject.SetActive(true);
        return this;
    }

    /// <summary>
    /// Disables all contents so you can enable your own
    /// </summary>
    /// <returns></returns>
    private MobileDialog disableContents(bool clear = true)
    {
        foreach (Transform child in dialogContent)
        {
            child.gameObject.SetActive(false);
        }
        return this;
    }

    /// <summary>
    /// Show the dialog with a button preset and callbacks
    /// </summary>
    /// <param name="buttonPreset"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="positiveCallback"></param>
    /// <param name="negativeCallback"></param>
    /// <returns></returns>
    public MobileDialog show(ButtonPreset buttonPreset, string title = "", string message = "", UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {

        //Reset background image
        setBackgroundImage(null, 0);

        //Apply buttons
        applyButtonPreset(buttonPreset, positiveCallback, negativeCallback);

        //Disable and enable elements
        disableContents();
        titleText.text = title;
        titleText.gameObject.SetActive(true);
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        dialogObject.SetActive(true);

        return this;
    }

    /// <summary>
    /// Sets the disabling background callback UnityAction
    /// </summary>
    /// <param name="backgroundCallback"></param>
    private void setBackgroundCallback(UnityAction backgroundCallback)
    {
        UnityAction callback = backgroundCallback ?? cancelAndClose;
        backgroundButton.onClick.RemoveAllListeners();
        backgroundButton.onClick.AddListener(callback);
    }

    /// <summary>
    /// Apply a button preset with callbacks
    /// </summary>
    /// <param name="buttonPreset"></param>
    /// <param name="positiveCallback"></param>
    /// <param name="negativeCallback"></param>
    private void applyButtonPreset(ButtonPreset buttonPreset, UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {
        setBackgroundCallback(negativeCallback);
        switch (buttonPreset)
        {
            case ButtonPreset.CancelOk:
                setUpOKCancelButtons(okString, cancelString, positiveCallback, negativeCallback);
                break;
            case ButtonPreset.Cancel:
                setUpCancelButton(cancelString, positiveCallback);
                break;
            case ButtonPreset.Ok:
                setUpCancelButton(okString, positiveCallback);
                break;
            case ButtonPreset.None:
                clearButtons();
                break;
        }
    }

    /// <summary>
    /// TODO: Finish this idea
    /// </summary>
    /// <param name="inputMode"></param>
    private void setInputMode(InputMode inputMode)
    {
        //disable inputs
        switch (inputMode)
        {
            case InputMode.None: break;
            case InputMode.Text:
                textInput.gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Show the dialog with progress of a webrequest
    /// </summary>
    /// <param name="title"></param>
    /// <param name="request"></param>
    public void showRequestProgress(string title, string description, UnityWebRequest request, bool showCancel = true, UnityAction negativeCallback = null)
    {

        if (showCancel) setUpCancelButton(cancelString, negativeCallback);
        else applyButtonPreset(ButtonPreset.None);

        titleText.text = title;
        dialogObject.SetActive(true);
        messageText.gameObject.SetActive(true);
        blockingRequest = request;
        StartCoroutine(progressRoutine());
    }

    /// <summary>
    /// Show request progress when not done
    /// </summary>
    /// <returns></returns>
    private IEnumerator progressRoutine()
    {
        downloading = true;
        while (tryGetDownloadProgress() < 1.0f)
        {
            setProgress(tryGetDownloadProgress());
            yield return null;
        }
        downloading = false;
        dialogObject.SetActive(false);
    }

    private float tryGetDownloadProgress()
    {
        try { return blockingRequest.downloadProgress; }
        catch (ArgumentException e) { return 1.0f; }
    }

    /// <summary>
    /// Set the current request progress
    /// </summary>
    /// <param name="downloadProgress"></param>
    private void setProgress(float downloadProgress)
    {
        messageText.text = String.Format("{0}%", Math.Floor(downloadProgress * 100), 1);
        //TODO: progress bar
        //progressImage.rectTransform.sizeDelta = new Vector2((sizeDeltaTarget.x / 100) * downloadProgress, progressHeight);
    }

    /// <summary>
    /// Toggle the dialog on/off
    /// </summary>
    public void toggle(bool force = false)
    {
        enabled = force ? force : !gameObject.activeSelf;
        dialogObject.SetActive(enabled);
    }

    /// <summary>
    /// Create ok/cancel buttons
    /// </summary>
    /// <param name="positiveText"></param>
    /// <param name="negativeText"></param>
    /// <param name="positiveCallback"></param>
    /// <param name="negativeCallback"></param>
    private void setUpOKCancelButtons(string positiveText, string negativeText, UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {
        UnityAction positive = positiveCallback ?? cancelAndClose;
        UnityAction negative = negativeCallback ?? cancelAndClose;
        clearButtons()
        .addButton(positiveText, positive)
        .addButton(negativeText, negative);
        setBackgroundCallback(negative);
    }

    /// <summary>
    /// Setup a cancel button, give it a callback for a custom cancel UnityAction
    /// Use the returned button to add callbacks on click
    /// </summary>
    /// <param name="callback"></param>
    private void setUpCancelButton(string text = null, UnityAction cancelCallback = null)
    {
        UnityAction cancel = cancelCallback ?? cancelAndClose;
        clearButtons()
        .addButton(text ?? cancelString, cancel);
        setBackgroundCallback(cancel);
    }

    /// <summary>
    /// Show dialog with input field
    /// </summary>
    /// <param name="title"></param>
    /// <param name="positiveCallback"></param>
    /// <param name="negativeCallback"></param>
    public void showInputDialog(string title, string inputText = "", UnityAction positiveCallback = null, UnityAction negativeCallback = null)
    {
        disableContents();
        titleText.text = title;
        titleText.gameObject.SetActive(true);
        textInput.text = inputText;
        textInput.gameObject.SetActive(true);
        setUpOKCancelButtons(okString, cancelString, positiveCallback, negativeCallback);
        toggle(true);
    }

    /// <summary>
    /// Show fullscreen text dialog
    /// </summary>
    /// <param name="text"></param>
    /// <param name="positiveCallback"></param>
    /// <param name="negativeCallback"></param>
    //public void showTextDialog(string text, bool clear = true, UnityAction positiveCallback = null) {
    //    disableContents(clear);
    //    addFullTextObject(text);
    //    textScrollRect.gameObject.SetActive(true);
    //    setUpCancelButton("Close", positiveCallback);
    //    toggle(true);
    //}

    /// <summary>
    /// Cancel and close dialog, abort request
    /// </summary>
    public void cancelAndClose()
    {
        bool shouldAbort = downloading && blockingRequest != null;
        if (shouldAbort)
        {
            //When using Abort() UnityWebRequest will interpret this as a network error
            blockingRequest.Abort();
            downloading = false;
        }
        if (dialogObject.activeSelf) dialogObject.SetActive(false);
    }

    public bool isVisible()
    {
        return dialogObject.activeSelf;
    }

    /// <summary>
    /// Set background image but keep it transparent so the text is still readable
    /// Note: the image is reset on showing the dialog!
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="alpha"></param>
    public void setBackgroundImage(Sprite sprite, float alpha = 0.33f)
    {
        backgroundImage.sprite = sprite;
    }
}
