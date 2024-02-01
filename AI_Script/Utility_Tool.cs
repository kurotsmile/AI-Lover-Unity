using System;
using UnityEngine;
using UnityEngine.Android;

public class Utility_Tool : MonoBehaviour
{
    public String[] list_name_action;
#if UNITY_ANDROID
    public AndroidJavaClass javaObject;
#endif

    public void on_Flashlight()
    {
#if UNITY_ANDROID
        javaObject.CallStatic("on", GetUnityActivity());
#endif
    }

    public void off_Flashlight()
    {
#if UNITY_ANDROID
        javaObject.CallStatic("off", GetUnityActivity());
#endif
    }

    public void on_load()
    {
#if UNITY_ANDROID
        javaObject = new AndroidJavaClass("com.myflashlight.flashlightlib.Flashlight");
#endif
    }

#if UNITY_ANDROID
    AndroidJavaObject GetUnityActivity()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public void open_content_Intent(string s_action = "android.settings.SETTINGS")
    {
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var intentObject = new AndroidJavaObject("android.content.Intent", s_action))
            {
                currentActivityObject.Call("startActivity", intentObject);
            }
        }
    }

    public void OpenApp_by_bundleId(string bundleId)
    {
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        ca.Call("startActivity", launchIntent);
        up.Dispose();
        ca.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }

}
