//#undef UNITY_EDITOR // Lets you edit Android code easily with formatting, comment out before going back to editor.
#if UNITY_ANDROID && !UNITY_EDITOR  // stop auto formatter removing unused using.
using UnityEngine;
#endif

public class AndroidDetectAutoRotation
{

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass androidUnityActivity = null;

    /// <summary>    
    /// <para> Gets the current UnityActivity used on Android. </para>
    /// It will store the AndroidJavaClass for later use ensuring it is not creating a new
    /// class in memory every call.
    /// </summary>
    /// <returns> The AndroidActivity with the UnityPlayer running in it. </returns>
    public static AndroidJavaObject GetUnityActivity() {  // Worth noting I have separated this out into a class for common Android calls :/
        if (androidUnityActivity == null) {
            androidUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        }
        return androidUnityActivity.GetStatic<AndroidJavaObject>("currentActivity");
    }
#endif
        
    /// <summary>
    /// Checks if Android Device has auto rotation enabled.
    /// </summary>
    /// <returns> True if auto-rotate is enabled, is not Android, or it fails. </returns>
    public static bool IsAutoRotateEnabled()
    {
        bool isAutoRotateEnabled = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
          // Uses $ as System is subclass of Settings
          AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
          using (AndroidJavaClass systemSettings = new AndroidJavaClass("android.provider.Settings$System"))
          using (AndroidJavaObject contentResolver = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getContentResolver")) {
            isAutoRotateEnabled = (systemSettings.CallStatic<int>("getInt", contentResolver, "accelerometer_rotation") == 1);
          }
        } catch (System.Exception e) {
          Debug.LogError(e);
        }
#endif
        return isAutoRotateEnabled;
    }
}