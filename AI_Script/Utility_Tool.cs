
using System;
using UnityEngine;

public class Utility_Tool : MonoBehaviour
{
    public App app;
    public String[] list_name_action;
    public String[] list_name_action_window;
    public String[] list_package_action;
#if UNITY_ANDROID
    private AndroidJavaClass javaObject;
    private AndroidJavaObject context;
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
#if UNITY_ANDROID && !UNITY_EDITOR
        javaObject = new AndroidJavaClass("com.myflashlight.flashlightlib.Flashlight");
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        this.context = activity.Call<AndroidJavaObject>("getApplicationContext");
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
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
            this.open_action_window_by_name(s_action);
        }
        else
        {
#if UNITY_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var intentObject = new AndroidJavaObject("android.content.Intent", s_action))
            {
                currentActivityObject.Call("startActivity", intentObject);
            }
        }
#endif
        }

    }

    public void OpenApp_by_bundleId(string bundleId)
    {
#if UNITY_ANDROID
        bool fail = false;
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject launchIntent = null;
        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            fail = true;
        }

        if (fail)
        {
            if (this.app.carrot.store_public ==Store.Google_Play) Application.OpenURL("https://play.google.com/store/apps/details?id=" + bundleId);
            if (this.app.carrot.store_public ==Store.Microsoft_Store) Application.OpenURL("ms-windows-store:navigate?appid=" + bundleId);
            if (this.app.carrot.store_public ==Store.Amazon_app_store) Application.OpenURL("amzn://apps/android?p=" + bundleId);
        }
        else
        {
            ca.Call("startActivity", launchIntent);
        }

        up.Dispose();
        ca.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
#endif
    }

    public void test_music()
    {
#if UNITY_ANDROID
        javaObject.CallStatic("play_music",this.context);
#endif
    }

    public void getAllAudioFromDevice()
    {
#if UNITY_ANDROID
        Carrot.Carrot_Box box_list = this.app.carrot.Create_Box("lis_audio");
        List<string> list_audio=javaObject.CallStatic<List<string>>("getAllAudioFromDevice", this.context);

        for (int i = 0; i < list_audio.Count; i++)
        {
            Carrot.Carrot_Box_Item item_file_audio = box_list.create_item("item_audio_" + i);
            item_file_audio.set_title(list_audio[i]);
            item_file_audio.set_tip(list_audio[i]);
        }
#endif
    }

    private void open_action_window_by_name(string s_id_name_act)
    {
        int index_func = this.get_index_window_func(s_id_name_act);
        if (index_func != -1)
        {
            string url_act_func = this.list_name_action_window[index_func];
            if(url_act_func!="") Application.OpenURL(url_act_func);
        }
    }

    private int get_index_window_func(string s_id_name_act)
    {
        for(int i = 0; i < this.list_name_action.Length; i++)
        {
            if (this.list_name_action[i] == s_id_name_act)
            {
                return i;
            }
        }
        return -1;
    }
}
