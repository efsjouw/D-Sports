using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AndroidUtils
{
    public static int getSDKInt()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }

    public static void setNavigationBarColor(Color color)
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                using (var window = activity.Call<AndroidJavaObject>("getWindow")) {
                    using (var androidColor = activity.Call<AndroidJavaObject>("Color"))
                    {
                        int parsedColor = androidColor.Call<int>("parseColor", ColorUtility.ToHtmlStringRGB(color));
                        window.Call("setNavigationBarColor", parsedColor);
                    }
                }
            }
        }
    }

    public static void setStatusBarColor(Color color)
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                using (var window = activity.Call<AndroidJavaObject>("getWindow")) {
                    using (var androidColor = activity.Call<AndroidJavaObject>("Color"))
                    {
                        int parsedColor = androidColor.Call<int>("parseColor", ColorUtility.ToHtmlStringRGB(color));
                        window.Call("setStatusBarColor", parsedColor);
                    }
                }
            }
        }
    }
}
