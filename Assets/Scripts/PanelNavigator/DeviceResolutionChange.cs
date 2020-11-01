using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DeviceResolutionChange : Singleton<DeviceResolutionChange>
{
    public class ResolutionChangeEvent : UnityEvent<Vector2> { };
    public ResolutionChangeEvent OnResolutionChange = new ResolutionChangeEvent();

    public class OrientationChangeEvent : UnityEvent<DeviceOrientation> { };
    public OrientationChangeEvent OnOrientationChange = new OrientationChangeEvent();

    public float CheckDelay = 0.5f;        // How long to wait until we check again.

    Vector2 resolution;                    // Current Resolution
    DeviceOrientation orientation;        // Current Device Orientation
    bool isAlive = true;                    // Keep this script running?

    void Start()
    {
        StartCoroutine(CheckForChange());
    }

    IEnumerator CheckForChange()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        orientation = Input.deviceOrientation;

        while (isAlive)
        {

            // Check for a Resolution Change
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                OnResolutionChange?.Invoke(resolution);
            }

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown: // Ignore
                case DeviceOrientation.FaceUp:  // Ignore
                case DeviceOrientation.FaceDown:    // Ignore
                    break;
                default:
                    if (orientation != Input.deviceOrientation)
                    {
                        orientation = Input.deviceOrientation;
                        if (OnOrientationChange != null) OnOrientationChange?.Invoke(orientation);
                    }
                    break;
            }

            yield return new WaitForSeconds(CheckDelay);
        }
    }

    void OnDestroy()
    {
        isAlive = false;
    }

}