using System;
using UnityEngine;

public class Utility_Tool : MonoBehaviour
{
    public String[] list_name_action;

    private AndroidJavaObject camera1;

    public void on_Flashlight()
    {
        AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
        WebCamDevice[] devices = WebCamTexture.devices;

        int camID = 0;
        camera1 = cameraClass.CallStatic<AndroidJavaObject>("open", camID);

        if (camera1 != null)
        {
            AndroidJavaObject cameraParameters = camera1.Call<AndroidJavaObject>("getParameters");
            cameraParameters.Call("setFlashMode", "torch");
            camera1.Call("setParameters", cameraParameters);
            camera1.Call("startPreview");
        }
        else
        {
            Debug.LogError("[CameraParametersAndroid] Camera not available");
        }
    }

    public void off_Flashlight()
    { 
        if (camera1 != null)
        {
            camera1.Call("stopPreview");
            camera1.Call("release");
        }
        else
        {
            Debug.LogError("[CameraParametersAndroid] Camera not available");
        }
    }



    public void on_load()
    {

    }

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
}
