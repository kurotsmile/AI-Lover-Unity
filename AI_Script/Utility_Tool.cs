
using Carrot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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

    public string Get_user_display_name()
    {
        string s_name = PlayerPrefs.GetString("ten_user", "").Trim();
        if (s_name != "") return s_name;

        if (this.app != null && this.app.carrot != null && this.app.carrot.lang != null)
        {
            string s_lang = this.app.carrot.lang.Get_key_lang();
            if (s_lang.StartsWith("vi")) return "ban";
        }

        return "you";
    }

    public string Get_virtual_assistant_name()
    {
        if (this.app != null && this.app.get_character() != null)
        {
            string s_name = this.app.get_character().get_name_character();
            if (!string.IsNullOrWhiteSpace(s_name)) return s_name.Trim();
        }

        return "AI Assistant";
    }

    public string Get_ai_assistant_system_prompt()
    {
        string s_user_name = this.Get_user_display_name();
        string s_assistant_name = this.Get_virtual_assistant_name();
        string s_lang = this.app != null && this.app.carrot != null && this.app.carrot.lang != null ? this.app.carrot.lang.Get_key_lang() : "en";
        string s_os = this.app != null && this.app.carrot != null ? this.app.carrot.os_app.ToString() : "Unknown";

        return "You are a virtual assistant inside a Unity mobile and desktop app. " +
               "Your assistant name is \"" + s_assistant_name + "\". " +
               "The user name is \"" + s_user_name + "\". " +
               "Reply in the current app language when possible. The current language code is \"" + s_lang + "\". " +
               "Act like a short, helpful assistant and address the user by name when suitable. " +
               "Refer to yourself as \"" + s_assistant_name + "\" when helpful. " +
               "Return ONLY one compact JSON object with these keys: " +
               "msg, func, link, face, action, act, color. " +
               "Use func \"0\" for a normal reply. " +
               "Use func \"16\" only when the user clearly asks to open a system setting, and set link to a valid Android settings action like " +
               "android.settings.SETTINGS, android.settings.WIFI_SETTINGS, android.settings.BLUETOOTH_SETTINGS, android.settings.DISPLAY_SETTINGS, " +
               "android.settings.SOUND_SETTINGS, android.settings.LOCATION_SOURCE_SETTINGS, android.settings.VOICE_INPUT_SETTINGS, android.settings.PRIVACY_SETTINGS, " +
               "android.media.action.VIDEO_CAMERA, android.intent.action.VIEW_DOWNLOADS, android.search.action.SEARCH_SETTINGS, android.intent.action.SET_WALLPAPER. " +
               "Use func \"19\" only for Android or mobile app opening when the user clearly asks to open an app and you know the exact app package id, such as " +
               "com.zing.zalo, com.instagram.android, org.telegram.messenger, com.spotify.music, com.discord, com.whatsapp, us.zoom.videomeetings, com.netflix.mediaclient. " +
               "On Windows, prefer func \"16\" with supported system actions, and if you do not know a supported launch action then use func \"0\". " +
               "If you are unsure, use func \"0\" and keep link empty. " +
               "Keep msg short and assistant-like for \"" + s_user_name + "\" on " + s_os + ".";
    }

    public IDictionary Try_build_virtual_assistant_chat(string userMessage)
    {
        string s_query_norm = this.Normalize_assistant_text(userMessage);
        if (s_query_norm == "") return null;

        if (!this.Is_open_request(s_query_norm) &&
            !this.Query_has_phrase(s_query_norm, "cai dat", "setting", "settings", "ung dung", "application", "app"))
        {
            return null;
        }

        string s_link;
        string s_target_name;
        if (this.Try_resolve_setting_action(s_query_norm, out s_link, out s_target_name))
            return this.Build_assistant_action_chat(userMessage, "16", s_link, s_target_name);

        if (this.Try_resolve_package_id(s_query_norm, out s_link, out s_target_name))
            return this.Build_assistant_action_chat(userMessage, "19", s_link, s_target_name);

        return null;
    }

    public IDictionary Parse_ai_response_to_chat_data(string responseText, string userMessage, string s_ai_name)
    {
        IDictionary chat_ai = this.Build_ai_chat_template(userMessage, s_ai_name);
        IDictionary data_json = this.Try_parse_json_object(responseText);

        if (data_json != null)
        {
            this.Apply_ai_response_value(chat_ai, data_json, "msg");
            this.Apply_ai_response_value(chat_ai, data_json, "text");
            this.Apply_ai_response_value(chat_ai, data_json, "message");
            this.Apply_ai_response_func(chat_ai, data_json);
            this.Apply_ai_response_link(chat_ai, data_json);
            this.Apply_ai_response_value(chat_ai, data_json, "face");
            this.Apply_ai_response_action(chat_ai, data_json);
            this.Apply_ai_response_value(chat_ai, data_json, "act");
            this.Apply_ai_response_value(chat_ai, data_json, "color");
        }
        else
        {
            chat_ai["msg"] = responseText != null ? responseText.Trim() : "";
        }

        this.Normalize_ai_action_command(chat_ai, userMessage);

        if (chat_ai["msg"] == null || chat_ai["msg"].ToString().Trim() == "")
        {
            string s_func = chat_ai["func"] != null ? chat_ai["func"].ToString() : "0";
            string s_link = chat_ai["link"] != null ? chat_ai["link"].ToString() : "";
            if (s_func == "16")
                chat_ai["msg"] = this.Build_assistant_action_message(this.Get_setting_display_name(s_link), true);
            else if (s_func == "19")
                chat_ai["msg"] = this.Build_assistant_action_message(this.Get_package_display_name(s_link), false);
            else
                chat_ai["msg"] = this.Build_default_assistant_message();
        }

        return chat_ai;
    }

    public bool Is_direct_url(string s_link)
    {
        if (string.IsNullOrWhiteSpace(s_link)) return false;

        string s_link_trim = s_link.Trim().ToLowerInvariant();
        return s_link_trim.StartsWith("http://") ||
               s_link_trim.StartsWith("https://") ||
               s_link_trim.StartsWith("mailto:") ||
               s_link_trim.StartsWith("tel:") ||
               s_link_trim.StartsWith("market:") ||
               s_link_trim.StartsWith("amzn:") ||
               s_link_trim.StartsWith("ms-settings:") ||
               s_link_trim.StartsWith("ms-windows-store:") ||
               s_link_trim.StartsWith("microsoft.");
    }

    private IDictionary Build_ai_chat_template(string userMessage, string s_ai_name)
    {
        IDictionary chat_ai = (IDictionary)Json.Deserialize("{}");
        chat_ai["id"] = "chat" + this.app.carrot.generateID();
        chat_ai["func"] = "0";
        chat_ai["status"] = "pending";
        chat_ai["key"] = userMessage;
        chat_ai["msg"] = "";
        chat_ai["face"] = UnityEngine.Random.Range(0, 18).ToString();

        if (this.app != null && this.app.action != null && this.app.action.list_anim_act_defalt != null && this.app.action.list_anim_act_defalt.Length > 0)
            chat_ai["action"] = UnityEngine.Random.Range(0, this.app.action.list_anim_act_defalt.Length).ToString();
        else
            chat_ai["action"] = "0";

        Color color_icon = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        chat_ai["color"] = "#" + ColorUtility.ToHtmlStringRGBA(color_icon);
        chat_ai["sex_user"] = this.app.setting.get_user_sex();
        chat_ai["sex_character"] = this.app.setting.get_character_sex();
        chat_ai["date_create"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        chat_ai["link"] = "";
        chat_ai["lang"] = this.app.carrot.lang.Get_key_lang();
        chat_ai["icon"] = "";
        chat_ai["pater"] = "";
        chat_ai["mp3"] = "";
        chat_ai["user"] = null;
        chat_ai["ai"] = s_ai_name;
        return chat_ai;
    }

    private IDictionary Build_assistant_action_chat(string userMessage, string s_func, string s_link, string s_target_name)
    {
        IDictionary chat_ai = this.Build_ai_chat_template(userMessage, "Assistant");
        chat_ai["func"] = s_func;
        chat_ai["link"] = s_link;
        chat_ai["msg"] = this.Build_assistant_action_message(s_target_name, s_func == "16");
        return chat_ai;
    }

    private string Build_assistant_action_message(string s_target_name, bool is_setting)
    {
        string s_lang = this.app != null && this.app.carrot != null && this.app.carrot.lang != null ? this.app.carrot.lang.Get_key_lang() : "en";
        string s_user_name = this.Get_user_display_name();
        string s_assistant_name = this.Get_virtual_assistant_name();

        if (string.IsNullOrWhiteSpace(s_target_name))
            s_target_name = is_setting ? "settings" : "app";

        if (s_lang.StartsWith("vi"))
        {
            if (is_setting)
                return s_assistant_name + " dang mo " + s_target_name + " cho " + s_user_name + ".";
            return s_assistant_name + " dang mo ung dung " + s_target_name + " cho " + s_user_name + ".";
        }

        if (is_setting)
            return s_assistant_name + " is opening " + s_target_name + " for " + s_user_name + ".";

        return s_assistant_name + " is opening the " + s_target_name + " app for " + s_user_name + ".";
    }

    private string Build_default_assistant_message()
    {
        string s_lang = this.app != null && this.app.carrot != null && this.app.carrot.lang != null ? this.app.carrot.lang.Get_key_lang() : "en";
        string s_user_name = this.Get_user_display_name();
        string s_assistant_name = this.Get_virtual_assistant_name();

        if (s_lang.StartsWith("vi"))
            return s_assistant_name + " dang lang nghe " + s_user_name + ".";

        return s_assistant_name + " is listening, " + s_user_name + ".";
    }

    private void Apply_ai_response_value(IDictionary chat_ai, IDictionary data_json, string s_key)
    {
        if (data_json == null || !data_json.Contains(s_key) || data_json[s_key] == null) return;

        string s_val = data_json[s_key].ToString().Trim();
        if (s_val == "") return;

        if (s_key == "text" || s_key == "message")
        {
            if (chat_ai["msg"] == null || chat_ai["msg"].ToString().Trim() == "") chat_ai["msg"] = s_val;
            return;
        }

        chat_ai[s_key] = s_val;
    }

    private void Apply_ai_response_action(IDictionary chat_ai, IDictionary data_json)
    {
        if (data_json == null) return;

        if (data_json.Contains("action") && data_json["action"] != null)
        {
            string s_action = data_json["action"].ToString().Trim();
            int action_index;
            if (int.TryParse(s_action, out action_index))
                chat_ai["action"] = s_action;
            else if (s_action != "" && (!data_json.Contains("act") || data_json["act"] == null || data_json["act"].ToString().Trim() == ""))
                chat_ai["act"] = s_action;
        }
    }

    private void Apply_ai_response_func(IDictionary chat_ai, IDictionary data_json)
    {
        if (data_json == null) return;

        string s_func = "";
        if (data_json.Contains("func") && data_json["func"] != null) s_func = data_json["func"].ToString().Trim();
        if (s_func == "" && data_json.Contains("function") && data_json["function"] != null) s_func = data_json["function"].ToString().Trim();

        if (s_func == "16" || s_func == "19")
            chat_ai["func"] = s_func;
        else
            chat_ai["func"] = "0";
    }

    private void Apply_ai_response_link(IDictionary chat_ai, IDictionary data_json)
    {
        if (data_json == null) return;

        string s_link = "";
        if (data_json.Contains("link") && data_json["link"] != null) s_link = data_json["link"].ToString().Trim();
        if (s_link == "" && data_json.Contains("url") && data_json["url"] != null) s_link = data_json["url"].ToString().Trim();
        if (s_link == "" && data_json.Contains("package") && data_json["package"] != null) s_link = data_json["package"].ToString().Trim();
        if (s_link == "" && data_json.Contains("intent") && data_json["intent"] != null) s_link = data_json["intent"].ToString().Trim();

        chat_ai["link"] = s_link;
    }

    private void Normalize_ai_action_command(IDictionary chat_ai, string userMessage)
    {
        string s_func = chat_ai["func"] != null ? chat_ai["func"].ToString().Trim() : "0";
        string s_link = chat_ai["link"] != null ? chat_ai["link"].ToString().Trim() : "";
        string s_query_norm = this.Normalize_assistant_text(userMessage);

        if (s_func == "16")
        {
            if (!this.Is_known_setting_action(s_link))
            {
                string s_target_name;
                if (this.Try_resolve_setting_action(s_query_norm, out s_link, out s_target_name))
                {
                    chat_ai["link"] = s_link;
                    if (chat_ai["msg"] == null || chat_ai["msg"].ToString().Trim() == "")
                        chat_ai["msg"] = this.Build_assistant_action_message(s_target_name, true);
                }
                else
                {
                    chat_ai["func"] = "0";
                    chat_ai["link"] = "";
                }
            }
        }
        else if (s_func == "19")
        {
            if (!this.Is_known_package_id(s_link))
            {
                string s_target_name;
                if (this.Try_resolve_package_id(s_query_norm, out s_link, out s_target_name))
                {
                    chat_ai["link"] = s_link;
                    if (chat_ai["msg"] == null || chat_ai["msg"].ToString().Trim() == "")
                        chat_ai["msg"] = this.Build_assistant_action_message(s_target_name, false);
                }
                else
                {
                    chat_ai["func"] = "0";
                    chat_ai["link"] = "";
                }
            }
        }
        else if (!this.Is_direct_url(s_link))
        {
            chat_ai["link"] = "";
        }
    }

    private IDictionary Try_parse_json_object(string s_text)
    {
        if (string.IsNullOrWhiteSpace(s_text)) return null;

        string s_json = s_text.Trim();
        if (s_json.StartsWith("```"))
        {
            int firstNewLine = s_json.IndexOf('\n');
            if (firstNewLine >= 0) s_json = s_json.Substring(firstNewLine + 1).Trim();
            if (s_json.EndsWith("```")) s_json = s_json.Substring(0, s_json.Length - 3).Trim();
        }

        int index_start = s_json.IndexOf('{');
        int index_end = s_json.LastIndexOf('}');
        if (index_start < 0 || index_end <= index_start) return null;

        s_json = s_json.Substring(index_start, index_end - index_start + 1);
        try
        {
            object data_raw = Json.Deserialize(s_json);
            return data_raw as IDictionary;
        }
        catch (Exception ex)
        {
            Debug.Log("AI JSON parse failed: " + ex.Message);
            return null;
        }
    }

    private bool Is_known_setting_action(string s_action)
    {
        if (string.IsNullOrWhiteSpace(s_action)) return false;

        for (int i = 0; i < this.list_name_action.Length; i++)
        {
            if (this.list_name_action[i] == s_action) return true;
        }
        return false;
    }

    private bool Is_known_package_id(string s_package_id)
    {
        if (string.IsNullOrWhiteSpace(s_package_id)) return false;

        for (int i = 0; i < this.list_package_action.Length; i++)
        {
            if (this.list_package_action[i] == s_package_id) return true;
        }
        return false;
    }

    private bool Try_resolve_setting_action(string s_query_norm, out string s_action, out string s_target_name)
    {
        s_action = "";
        s_target_name = "";

        bool has_setting_context = this.Query_has_phrase(s_query_norm, "cai dat", "setting", "settings", "he thong", "system");
        if (!has_setting_context && !this.Is_open_request(s_query_norm)) return false;

        if (this.Query_has_phrase(s_query_norm, "wifi", "wi fi", "wireless lan"))
        {
            s_action = "android.settings.WIFI_SETTINGS";
            s_target_name = "Wi-Fi";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "bluetooth"))
        {
            s_action = "android.settings.BLUETOOTH_SETTINGS";
            s_target_name = "Bluetooth";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "am thanh", "sound", "volume", "loa", "audio"))
        {
            s_action = "android.settings.SOUND_SETTINGS";
            s_target_name = "sound settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "man hinh", "display", "screen", "hien thi"))
        {
            s_action = "android.settings.DISPLAY_SETTINGS";
            s_target_name = "display settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "vi tri", "location", "gps"))
        {
            s_action = "android.settings.LOCATION_SOURCE_SETTINGS";
            s_target_name = "location settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "giong noi", "voice", "speech", "tts"))
        {
            s_action = "android.settings.VOICE_INPUT_SETTINGS";
            s_target_name = "voice settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "ngon ngu", "language", "locale"))
        {
            s_action = "android.settings.LOCALE_SETTINGS";
            s_target_name = "language settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "rieng tu", "privacy", "quyen rieng tu"))
        {
            s_action = "android.settings.PRIVACY_SETTINGS";
            s_target_name = "privacy settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "bao mat", "security"))
        {
            s_action = "android.settings.SECURITY_SETTINGS";
            s_target_name = "security settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "thong bao", "notification", "notifications"))
        {
            s_action = "android.settings.APP_NOTIFICATION_SETTINGS";
            s_target_name = "notification settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "pin", "battery"))
        {
            s_action = "android.settings.BATTERY_SAVER_SETTINGS";
            s_target_name = "battery settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "luu tru", "storage", "bo nho", "memory"))
        {
            s_action = "android.settings.INTERNAL_STORAGE_SETTINGS";
            s_target_name = "storage settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "ban phim", "keyboard", "input"))
        {
            s_action = "android.settings.INPUT_METHOD_SETTINGS";
            s_target_name = "keyboard settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "ung dung", "application", "applications", "apps"))
        {
            s_action = "android.settings.MANAGE_APPLICATIONS_SETTINGS";
            s_target_name = "app settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "vpn"))
        {
            s_action = "android.settings.VPN_SETTINGS";
            s_target_name = "VPN settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "may bay", "airplane"))
        {
            s_action = "android.settings.AIRPLANE_MODE_SETTINGS";
            s_target_name = "airplane mode settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "tro nang", "accessibility"))
        {
            s_action = "android.settings.ACCESSIBILITY_SETTINGS";
            s_target_name = "accessibility settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "ngay gio", "date", "time", "gio"))
        {
            s_action = "android.settings.DATE_SETTINGS";
            s_target_name = "date and time settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "developer", "nha phat trien"))
        {
            s_action = "android.settings.APPLICATION_DEVELOPMENT_SETTINGS";
            s_target_name = "developer settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "camera", "may anh"))
        {
            s_action = "android.media.action.VIDEO_CAMERA";
            s_target_name = "camera";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "download", "tai xuong", "tap tin tai"))
        {
            s_action = "android.intent.action.VIEW_DOWNLOADS";
            s_target_name = "downloads";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "tim kiem", "search"))
        {
            s_action = "android.search.action.SEARCH_SETTINGS";
            s_target_name = "search settings";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "hinh nen", "wallpaper", "background"))
        {
            s_action = "android.intent.action.SET_WALLPAPER";
            s_target_name = "wallpaper";
            return true;
        }

        if (this.Query_has_phrase(s_query_norm, "cai dat", "setting", "settings", "he thong", "system"))
        {
            s_action = "android.settings.SETTINGS";
            s_target_name = "settings";
            return true;
        }

        return false;
    }

    private bool Try_resolve_package_id(string s_query_norm, out string s_package_id, out string s_target_name)
    {
        s_package_id = "";
        s_target_name = "";
        if (this.app != null && this.app.carrot != null && this.app.carrot.os_app == Carrot.OS.Window) return false;
        if (!this.Is_open_request(s_query_norm)) return false;

        int best_score = 0;
        string best_package = "";
        for (int i = 0; i < this.list_package_action.Length; i++)
        {
            string s_package_cur = this.list_package_action[i];
            int score = this.Get_package_match_score(s_query_norm, s_package_cur);
            if (score > best_score)
            {
                best_score = score;
                best_package = s_package_cur;
            }
        }

        if (best_score < 500 || best_package == "") return false;

        s_package_id = best_package;
        s_target_name = this.Get_package_display_name(best_package);
        return true;
    }

    private int Get_package_match_score(string s_query_norm, string s_package_id)
    {
        if (string.IsNullOrWhiteSpace(s_package_id)) return 0;

        int best_score = 0;
        string s_package_norm = this.Normalize_assistant_text(s_package_id.Replace(".", " "));
        if (s_package_norm != "" && s_query_norm.Contains(s_package_norm)) best_score = 2000;

        string[] list_alias = this.Get_package_aliases(s_package_id);
        for (int i = 0; i < list_alias.Length; i++)
        {
            string s_alias_norm = this.Normalize_assistant_text(list_alias[i]);
            if (s_alias_norm != "" && s_query_norm.Contains(s_alias_norm))
                best_score = Mathf.Max(best_score, 1200 + s_alias_norm.Length);
        }

        string[] arr_token = s_package_norm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arr_token.Length; i++)
        {
            string s_token = arr_token[i];
            if (s_token.Length < 4) continue;
            if (s_token == "android" || s_token == "mobile" || s_token == "client" || s_token == "app" || s_token == "lite" || s_token == "customer") continue;

            if (s_query_norm.Contains(s_token))
                best_score = Mathf.Max(best_score, 500 + s_token.Length);
        }

        return best_score;
    }

    private string[] Get_package_aliases(string s_package_id)
    {
        if (s_package_id == "com.zing.zalo") return new[] { "zalo" };
        if (s_package_id == "com.ss.android.ugc.trill" || s_package_id == "com.zhiliaoapp.musically.go") return new[] { "tiktok", "tik tok" };
        if (s_package_id == "com.facebook.orca") return new[] { "messenger", "facebook messenger" };
        if (s_package_id == "com.zing.mp3") return new[] { "zing mp3", "zmp3" };
        if (s_package_id == "org.telegram.messenger") return new[] { "telegram" };
        if (s_package_id == "com.grabtaxi.passenger") return new[] { "grab" };
        if (s_package_id == "com.instagram.android") return new[] { "instagram", "insta" };
        if (s_package_id == "com.cloudflare.onedotonedotonedotone") return new[] { "1.1.1.1", "warp", "cloudflare" };
        if (s_package_id == "sg.bigo.live") return new[] { "bigo", "bigo live" };
        if (s_package_id == "com.duolingo") return new[] { "duolingo" };
        if (s_package_id == "com.twitter.android") return new[] { "twitter", "x" };
        if (s_package_id == "com.discord") return new[] { "discord" };
        if (s_package_id == "com.valvesoftware.android.steam.community") return new[] { "steam" };
        if (s_package_id == "jp.naver.line.android") return new[] { "line" };
        if (s_package_id == "com.tencent.mm") return new[] { "wechat", "we chat" };
        if (s_package_id == "com.spotify.music" || s_package_id == "com.spotify.lite") return new[] { "spotify" };
        if (s_package_id == "us.zoom.videomeetings") return new[] { "zoom" };
        if (s_package_id == "com.netflix.mediaclient") return new[] { "netflix" };
        if (s_package_id == "com.snapchat.android") return new[] { "snapchat" };
        if (s_package_id == "com.whatsapp") return new[] { "whatsapp", "what app" };
        if (s_package_id == "com.truecaller") return new[] { "truecaller" };
        if (s_package_id == "com.lazada.android") return new[] { "lazada" };
        if (s_package_id == "com.shopee.vn") return new[] { "shopee" };
        if (s_package_id == "com.mservice.momotransfer") return new[] { "momo" };
        if (s_package_id == "com.vnid") return new[] { "vnid", "vneid" };
        if (s_package_id == "com.mbmobile") return new[] { "mb bank", "mbbank" };
        if (s_package_id == "com.VCB") return new[] { "vietcombank", "vcb" };
        if (s_package_id == "com.vietinbank.ipay") return new[] { "vietinbank" };
        if (s_package_id == "vn.com.techcombank.bb.app") return new[] { "techcombank", "tcb" };
        if (s_package_id == "mobile.acb.com.vn") return new[] { "acb" };
        if (s_package_id == "com.supercell.clashofclans") return new[] { "clash of clans", "coc" };
        if (s_package_id == "com.dts.freefireth") return new[] { "free fire" };
        if (s_package_id == "com.riotgames.league.wildrift") return new[] { "wild rift" };
        if (s_package_id == "com.roblox.client") return new[] { "roblox" };
        if (s_package_id == "com.garena.game.kgvn") return new[] { "lien quan", "arena of valor" };
        if (s_package_id == "com.outfit7.mytalkingtomfree") return new[] { "talking tom" };
        if (s_package_id == "com.moonactive.coinmaster") return new[] { "coin master" };
        if (s_package_id == "com.vng.pubgmobile") return new[] { "pubg", "pubg mobile" };
        if (s_package_id == "com.miHoYo.GenshinImpact.vn") return new[] { "genshin", "genshin impact" };
        if (s_package_id == "com.king.candycrushsaga") return new[] { "candy crush" };
        if (s_package_id == "com.innersloth.spacemafia") return new[] { "among us" };
        if (s_package_id == "com.vng.codmvn") return new[] { "call of duty", "codm", "call of duty mobile" };

        return new string[0];
    }

    private string Get_package_display_name(string s_package_id)
    {
        string[] list_alias = this.Get_package_aliases(s_package_id);
        if (list_alias.Length > 0) return list_alias[0];

        string[] arr_token = s_package_id.Split('.');
        if (arr_token.Length > 0)
        {
            string s_last = arr_token[arr_token.Length - 1];
            if (s_last != "") return s_last;
        }

        return s_package_id;
    }

    private string Get_setting_display_name(string s_action)
    {
        if (s_action == "android.settings.WIFI_SETTINGS") return "Wi-Fi settings";
        if (s_action == "android.settings.BLUETOOTH_SETTINGS") return "Bluetooth settings";
        if (s_action == "android.settings.SOUND_SETTINGS") return "sound settings";
        if (s_action == "android.settings.DISPLAY_SETTINGS") return "display settings";
        if (s_action == "android.settings.LOCATION_SOURCE_SETTINGS") return "location settings";
        if (s_action == "android.settings.VOICE_INPUT_SETTINGS") return "voice settings";
        if (s_action == "android.settings.LOCALE_SETTINGS") return "language settings";
        if (s_action == "android.settings.PRIVACY_SETTINGS") return "privacy settings";
        if (s_action == "android.settings.SECURITY_SETTINGS") return "security settings";
        if (s_action == "android.settings.APP_NOTIFICATION_SETTINGS") return "notification settings";
        if (s_action == "android.settings.BATTERY_SAVER_SETTINGS") return "battery settings";
        if (s_action == "android.settings.INTERNAL_STORAGE_SETTINGS") return "storage settings";
        if (s_action == "android.settings.INPUT_METHOD_SETTINGS") return "keyboard settings";
        if (s_action == "android.settings.MANAGE_APPLICATIONS_SETTINGS") return "app settings";
        if (s_action == "android.settings.VPN_SETTINGS") return "VPN settings";
        if (s_action == "android.settings.AIRPLANE_MODE_SETTINGS") return "airplane mode settings";
        if (s_action == "android.settings.ACCESSIBILITY_SETTINGS") return "accessibility settings";
        if (s_action == "android.settings.DATE_SETTINGS") return "date and time settings";
        if (s_action == "android.settings.APPLICATION_DEVELOPMENT_SETTINGS") return "developer settings";
        if (s_action == "android.media.action.VIDEO_CAMERA") return "camera";
        if (s_action == "android.intent.action.VIEW_DOWNLOADS") return "downloads";
        if (s_action == "android.search.action.SEARCH_SETTINGS") return "search settings";
        if (s_action == "android.intent.action.SET_WALLPAPER") return "wallpaper";
        if (s_action == "android.settings.SETTINGS") return "settings";
        return "settings";
    }

    private bool Is_open_request(string s_query_norm)
    {
        return this.Query_has_phrase(s_query_norm,
            "mo", "bat", "vao", "truy cap", "khoi dong", "run", "start", "open", "launch", "go to", "show");
    }

    private bool Query_has_phrase(string s_query_norm, params string[] list_phrase)
    {
        if (s_query_norm == "") return false;

        for (int i = 0; i < list_phrase.Length; i++)
        {
            string s_phrase_norm = this.Normalize_assistant_text(list_phrase[i]);
            if (s_phrase_norm == "") continue;
            if (s_query_norm == s_phrase_norm) return true;
            if (s_query_norm.StartsWith(s_phrase_norm + " ")) return true;
            if (s_query_norm.EndsWith(" " + s_phrase_norm)) return true;
            if (s_query_norm.Contains(" " + s_phrase_norm + " ")) return true;
        }

        return false;
    }

    private string Normalize_assistant_text(string s_text)
    {
        if (string.IsNullOrWhiteSpace(s_text)) return "";

        string s_lower = s_text.Trim().ToLowerInvariant().Replace('đ', 'd');
        string s_form_d = s_lower.Normalize(NormalizationForm.FormD);
        StringBuilder s_builder = new StringBuilder();
        bool is_last_space = false;

        for (int i = 0; i < s_form_d.Length; i++)
        {
            char c = s_form_d[i];
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(c))
            {
                s_builder.Append(c);
                is_last_space = false;
            }
            else if (!is_last_space && s_builder.Length > 0)
            {
                s_builder.Append(' ');
                is_last_space = true;
            }
        }

        return s_builder.ToString().Trim();
    }

    private void open_action_window_by_name(string s_id_name_act)
    {
        int index_func = this.Get_index_window_func(s_id_name_act);
        if (index_func != -1)
        {
            string url_act_func = this.list_name_action_window[index_func];
            if(url_act_func!="") Application.OpenURL(url_act_func);
        }
    }

    private int Get_index_window_func(string s_id_name_act)
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
