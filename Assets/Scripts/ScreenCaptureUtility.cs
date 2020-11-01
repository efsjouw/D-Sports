using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCaptureUtility : MonoBehaviour
{
    [SerializeField]
    string path = "";

    [ContextMenu("makeScreenshot")]
    public void makeScreenshot()
    {
        ScreenCapture.CaptureScreenshot(path);
    }
}
