using System;
using System.Collections;
using TextSpeech;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum OrderBy_Type { date_asc, date_desc, name_asc, name_desc }

public class App : MonoBehaviour
{
    [Header("Main Obj")]
    public Carrot.Carrot carrot;
    public Setting setting;
    public Player_music player_music;
    public Report report;
    public Environment view;
    public Command_storage command_storage;
    public Command_Dev command_dev;
    public Command command;
    public Command_Live live;
    public Voice_Command command_voice;
    public TextToSpeech textToSpeech;
    public Icon icon;
    public OpenAIChatbot open_AI;

    [Header("App obj")]
    public bool is_radio_func = true;
    public Image img_bt_sel_lang;
    public Color32 color_nomal;

    public GameObject panel_main;
    public GameObject panel_chat_msg;
    public GameObject panel_chat_func;
    public GameObject panel_inp_msg;
    public GameObject panel_inp_func;
    public GameObject panel_inp_command_test;
    public GameObject panel_menu_right;
    public GameObject panel_menu_right_act;
    public GameObject panel_menu_right_music;

    public Color32 color_bk_default;

    public Sprite icon_pc_chat_boy;
    public Sprite icon_pc_chat_girl;

    private Item_command_chat item_cur_log_chat;
    public GameObject button_randio;
    private string link_deep_app;

    [Header("Character")]
    public character_manager[] character;
    private int sel_character_sex = 0;

    [Header("Manager panel function")]
    public GameObject[] panel_fnc_box;
    public GameObject[] btn_fnc_menu;
    public Transform area_body_fnc_menu;
    private int sel_func = 0;

    [Header("Setting app")]
    public Image img_icon_setting_app_inp;
    public Image img_icon_setting_app_menu_right;
    public Text[] txt_change_color;
    public Image[] img_change_color;

    [Header("Weather")]
    public string[] key_api_weather;
    private string s_weather_temp_min;
    private string s_weather_temp_max;
    private string s_weather_temp_feels_like;
    public Text txt_weather_description;
    public Text txt_weather_tip;
    public Text txt_weather_temp;
    public Text txt_sunrise;
    public Text txt_sunset;
    public Text txt_visibility;
    public Text txt_wind_speed;
    public Text txt_wind_deg;
    public Text txt_pressur;
    public Text txt_clouds;
    public Text txt_humidity;
    public Image img_weather_icon;

    [Header("Contacts")]
    public Sprite icon_contacts;

    [Header("Scene")]
    public Transform tr_scene_main;
    public Transform area_scene_main_portait;
    public Transform area_scene_main_landspace;
    public Transform tr_scene_menu_right;
    public Transform area_scene_menu_right_portait;
    public Transform area_scene_menu_right_landspace;
    public Image img_menu_bottom_border;
    public Transform tr_scene_music_mini;
    public Transform area_scene_music_mini_portait;
    public Transform area_scene_music_mini_landspace;
    public Transform tr_scene_environment;
    public Transform area_scene_environment_portait;
    public Transform area_scene_environment_landspace;
    public Transform tr_scene_character;
    public Transform area_scene_charactert_portait;
    public Transform area_scene_character_landspace;

    [Header("Sound")]
    public AudioSource[] audio_sound;

    [Header("S Data temp")]
    public string s_data_json_head_temp;
    public string s_data_json_costumes_temp;

    private Carrot.Carrot_Box box_list;
    private int count_hit = -1;

    void Start()
    {
        this.link_deep_app = Application.absoluteURL;
        Application.deepLinkActivated += onDeepLinkActivated;

        this.panel_main.SetActive(false);
        this.panel_chat_msg.SetActive(true);
        this.panel_chat_func.SetActive(false);
        this.panel_inp_func.SetActive(false);
        this.panel_inp_msg.SetActive(true);
        this.panel_inp_command_test.SetActive(false);

        this.carrot.Load_Carrot(this.check_app_exit);
        this.carrot.shop.onCarrotPaySuccess += this.onBuySuccessCarrotPay;
        this.carrot.shop.onCarrotRestoreSuccess += this.onRestoreSuccessCarrotPay;
        this.carrot.ads.onRewardedSuccess += this.setting.act_ads_rewarded_success;
        this.carrot.act_after_delete_all_data = this.delete_all_data;
        this.carrot.act_after_close_all_box += this.close_all_box;

        this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().check_show_emp_by_resolution();
        this.act_change_Scene_Rotation();

        this.view.on_start();
        this.setting.load_setting();
        this.panel_main.SetActive(true);
        this.player_music.panel_player_mini.SetActive(false);
        this.show_chat_function();
        this.check_manager_character();

        this.command.load();
        this.command_storage.check_load_command_storage();
        this.command.sound_command.pitch = this.setting.get_voice_speed();
        this.command_dev.check();

        this.icon.load();
    }

    private void close_all_box()
    {
        this.command_dev.close_all_box();;
    }

    public void load_app_where_online()
    {
        if (PlayerPrefs.GetString("lang", "") == "")
        {
            this.carrot.show_list_lang(this.load);
            this.get_character().gameObject.SetActive(false);
        }
        else
        {
            this.load("");
        }
    }

    public void load_app_where_offline()
    {
        this.load_weather();
        this.get_character().gameObject.SetActive(true);
        this.sel_menu_func_app(0);
        this.panel_main.SetActive(true);
    }

    public void load(string s_data_lang)
    {
        this.panel_main.SetActive(true);
        this.load_weather();
        this.get_character().gameObject.SetActive(true);
        this.sel_menu_func_app(0);
        this.GetComponent<Voice_Command>().set_DetectionLanguage(PlayerPrefs.GetString("key_voice", "en"));
        DateTime currentTime = DateTime.Now;
        int hour = currentTime.Hour;
        this.command.send_chat("hi_" + hour);
        if (!this.is_radio_func) this.button_randio.SetActive(false);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) this.carrot.delay_function(3f, this.check_link_deep_app);
    }

    private void onDeepLinkActivated(string url)
    {
        this.link_deep_app = url;
        if (this.carrot != null) this.carrot.delay_function(1f, this.check_link_deep_app);
    }

    private void act_deep_link_handle(string s_data)
    {
        if (s_data != "")
        {
            IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
            this.GetComponent<Command>().act_chat(data_chat);
        }
        this.link_deep_app = "";
    }

    public void check_link_deep_app()
    {
        if (this.link_deep_app.Trim() != "")
        {
            if (this.carrot.is_online())
            {
                if (this.link_deep_app.Contains("ailover:"))
                {
                    string data_link = this.link_deep_app.Split("?"[0])[1];
                    data_link = UnityWebRequest.UnEscapeURL(data_link);
                    this.link_deep_app = "";
                }

                if (this.link_deep_app.Contains("music:"))
                {
                    string data_link = this.link_deep_app.Replace("music://show/", "");
                    string[] paramet_music = data_link.Split('/');
                    this.link_deep_app = "";
                }

                if (this.link_deep_app.Contains("flower:"))
                {
                    string data_link = this.link_deep_app.Replace("flower://show/", "");
                    string[] paramet_music = data_link.Split('/');
                    this.link_deep_app = "";
                }

                if (this.link_deep_app.Contains("contactstore:"))
                {
                    string data_link = this.link_deep_app.Replace("contactstore://show/", "");
                    string[] paramet_contact = data_link.Split('/');
                    this.carrot.user.show_user_by_id(paramet_contact[0], paramet_contact[1]);
                    this.link_deep_app = "";
                }
            }
        }
    }

    [ContextMenu("Test Link")]
    public void test_link()
    {
        //this.onDeepLinkActivated("ailover://data?%7B%22function%22%3A%22show_chat%22%2C%22lang%22%3A%22vi%22%2C%22id%22%3A%2277018%22%2C%22type%22%3A%22chat%22%2C%22host%22%3A%22carrotstore.com%22%7D");
        //this.onDeepLinkActivated("music://show/57416/vi");
        //this.onDeepLinkActivated("flower://show/74501/vi");
        this.onDeepLinkActivated("contactstore://show/a920b4f61c211172e6e621a69abe7299/vi");
    }

    public void check_manager_character()
    {
        this.sel_character_sex = int.Parse(PlayerPrefs.GetString("character_sex", "1"));
        this.character[0].gameObject.SetActive(false);
        this.character[1].gameObject.SetActive(false);
        this.character[this.sel_character_sex].gameObject.SetActive(true);
        this.character[this.sel_character_sex].load_character();
    }

    public character_manager get_character()
    {
        return this.character[this.sel_character_sex];
    }

    public character_manager get_character_by_sex(int index_sex)
    {
        return this.character[index_sex];
    }

    void check_app_exit()
    {
        if (this.panel_chat_func.activeInHierarchy)
        {
            this.show_chat_function();
            this.carrot.set_no_check_exit_app();
        }
    }

    public void btn_show_google_weather()
    {
        Application.OpenURL("https://www.google.com/search?q=google+weather");
    }

    public void update_color_select()
    {
        this.txt_weather_tip.color = this.carrot.color_highlight;
        this.txt_visibility.color = this.carrot.color_highlight;
        this.txt_wind_deg.color = this.carrot.color_highlight;
        this.txt_wind_speed.color = this.carrot.color_highlight;
        this.txt_clouds.color = this.carrot.color_highlight;
        this.txt_pressur.color = this.carrot.color_highlight;
        this.txt_humidity.color = this.carrot.color_highlight;
        this.txt_weather_temp.color = this.carrot.color_highlight;
        this.txt_sunrise.color = this.carrot.color_highlight;
        this.txt_sunset.color = this.carrot.color_highlight;
        this.player_music.txt_name_song_full.color = this.carrot.color_highlight;
        this.sel_func_app();

        for (int i = 0; i < this.txt_change_color.Length; i++) this.txt_change_color[i].color = this.carrot.color_highlight;
        for (int i = 0; i < this.img_change_color.Length; i++) this.img_change_color[i].color = this.carrot.color_highlight;
    }

    public Sprite get_icon_sex_user()
    {
        if (PlayerPrefs.GetString("sex", "0") == "0")
            return this.icon_pc_chat_boy;
        else
            return this.icon_pc_chat_girl;
    }

    public void exit_app()
    {
        if (this.carrot.model_app == Carrot.ModelApp.Develope) Debug.Log("Exit app... bye bye!");
        Application.Quit();
    }

    [ContextMenu("Chat switchfunction")]
    public void btn_chat_switch_function()
    {
        if (this.panel_chat_msg.activeInHierarchy)
        {
            this.show_func_function();
        }
        else
        {
            this.show_chat_function();
        }
    }

    public void show_chat_function()
    {
        this.panel_chat_msg.SetActive(true);
        this.panel_chat_func.SetActive(false);
        this.panel_inp_msg.SetActive(true);
        this.panel_inp_func.SetActive(false);
        this.panel_inp_command_test.SetActive(false);
        if (this.player_music.sound_music.isPlaying) this.player_music.panel_player_mini.SetActive(true);
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait()) this.panel_menu_right.SetActive(false);
    }

    public void show_func_function()
    {
        this.panel_chat_msg.SetActive(false);
        this.panel_chat_func.SetActive(true);
        this.panel_inp_msg.SetActive(false);
        this.panel_inp_func.SetActive(true);
        this.panel_menu_right.SetActive(true);
        this.panel_inp_command_test.SetActive(false);
    }

    public IEnumerator get_weather_buy_address()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.openweathermap.org/data/2.5/weather?q=" + PlayerPrefs.GetString("weather_pin", "london,us") + "&appid=" + this.get_key_weather_api() + "&lang=" + PlayerPrefs.GetString("lang", "vi") + "&mode=json&units=metric&cnt=3"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                PlayerPrefs.SetString("weather_" + System.DateTime.Now.ToString("dd_MM_yyyy") + "_" + PlayerPrefs.GetString("lang", "vi"), www.downloadHandler.text);
                this.load_info_weather(www.downloadHandler.text);
            }
        }
    }

    public IEnumerator get_weather_buy_lot_and_lat(float weather_longitude,float weather_latitude)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.openweathermap.org/data/2.5/onecall?lat="+ weather_longitude + "&lon="+ weather_latitude + "&exclude=hourly,daily&appid=" + this.get_key_weather_api() + "&lang=" + PlayerPrefs.GetString("lang", "vi") + "&mode=json&units=metric&cnt=3"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                PlayerPrefs.SetString("weather_" + System.DateTime.Now.ToString("dd_MM_yyyy") + "_" + PlayerPrefs.GetString("lang", "vi"), www.downloadHandler.text);
                this.load_info_weather(www.downloadHandler.text);
                this.setting.set_text_weather_pin(weather_longitude+","+ weather_latitude);
            }
        }
    }

    public void load_weather()
    {
        string s_data_waether = PlayerPrefs.GetString("weather_" + System.DateTime.Now.ToString("dd_MM_yyyy") + "_" + PlayerPrefs.GetString("lang", "vi"));
        if (s_data_waether == "")
        {
            if (this.carrot.is_online()) StartCoroutine(get_weather_buy_address());
        }
        else
        {
            load_info_weather(s_data_waether);
        }
    }

    private void load_info_weather(string s)
    {
        IDictionary data = (IDictionary)Carrot.Json.Deserialize(s);
        IDictionary current = (IDictionary)data["current"];
        if (data["weather"] == null)
        {
            current["lat"] = data["lat"];
            current["lon"] = data["lon"];
            data = current;
        }
        IList list_weather = (IList)data["weather"];
        IDictionary weather = (IDictionary)list_weather[0];
        IDictionary main = (IDictionary)data["main"];
        IDictionary sys = (IDictionary)data["sys"];
        IDictionary wind = (IDictionary)data["wind"];

        string s_clouds = "";
        if (data["clouds"]!= null)
        {
            try
            {
                IDictionary clouds = (IDictionary)data["clouds"];
                if(clouds["all"]!=null) s_clouds = clouds["all"].ToString();
            }catch(Exception)
            {
                s_clouds = data["clouds"].ToString();
            }
        }

        string s_weather_temp = "";
        string s_sunrise = "";
        string s_sunset = "";
        string s_speed = "";
        string s_deg = "";
        string s_pressure = "";
        string s_humidity = "";
        string s_visibility = "";

        if (data["temp"] != null)
        {
            s_weather_temp = data["temp"].ToString();
            if(data["sunrise"]!=null) s_sunrise = data["sunrise"].ToString();
            if(data["sunset"]!=null) s_sunset = data["sunset"].ToString();
            if(data["wind_speed"]!=null) s_speed = data["wind_speed"].ToString();
            if(data["wind_deg"]!=null) s_deg = data["wind_deg"].ToString();
            if(data["pressure"]!=null) s_pressure= data["pressure"].ToString();
            if(data["humidity"]!=null) s_humidity = data["humidity"].ToString();
            this.s_weather_temp_min = "0.0";
            this.s_weather_temp_max = "0.0";
            if(data["feels_like"]!=null) this.s_weather_temp_feels_like = data["feels_like"].ToString();
        }
        else
        {
            if(main["temp"]!=null) s_weather_temp = main["temp"].ToString();
            if(sys["sunrise"]!=null) s_sunrise = sys["sunrise"].ToString();
            if(sys["sunset"]!=null) s_sunset = sys["sunset"].ToString();
            if(wind["speed"]!=null) s_speed = wind["speed"].ToString();
            if(wind["deg"]!=null) s_deg = wind["deg"].ToString();
            if(main["pressure"]!=null) s_pressure = main["pressure"].ToString();
            if(main["humidity"] != null) s_humidity = main["humidity"].ToString();
            if(main["temp_min"]!=null) this.s_weather_temp_min = main["temp_min"].ToString();
            if(main["temp_max"]!=null) this.s_weather_temp_max = main["temp_max"].ToString();
            if(main["feels_like"]!=null) this.s_weather_temp_feels_like = main["feels_like"].ToString();
        }

        if(data["visibility"]!=null) s_visibility = data["visibility"].ToString();

        this.txt_weather_temp.text = s_weather_temp + "°C";
        if(weather["description"]!=null) this.txt_weather_description.text = weather["description"].ToString();
        this.txt_sunrise.text = UnixTimeStampToDateTime(long.Parse(s_sunrise)).ToString("dd/MM hh:ss tt");
        this.txt_sunset.text = UnixTimeStampToDateTime(long.Parse(s_sunset)).ToString("dd/MM hh:ss tt");
        this.txt_wind_speed.text = s_speed + "m/s";
        this.txt_wind_deg.text =s_deg + "°";
        this.txt_visibility.text = s_visibility + "m";
        this.txt_pressur.text = s_pressure+"hPa";
        this.txt_clouds.text = s_clouds + "%";
        this.txt_humidity.text = s_humidity + "%";

        if (data["name"]!=null)
            this.setting.set_name_address_weather(data["name"].ToString());
        else
            this.setting.set_name_address_weather(data["lat"].ToString()+","+ data["lon"].ToString());

        if (weather["icon"] != null)
        {
            string id_icon_weather = weather["icon"].ToString();
            Sprite sp_icon_weather = this.carrot.get_tool().get_sprite_to_playerPrefs("w" + id_icon_weather);
            if (sp_icon_weather != null)
            {
                this.img_weather_icon.sprite = sp_icon_weather;
                this.img_weather_icon.color = Color.white;
            }
            else
            {
                this.carrot.get_img_and_save_playerPrefs("https://openweathermap.org/img/wn/" + id_icon_weather + "@2x.png", this.img_weather_icon, "w" + id_icon_weather);
            }
        }

        this.show_tip_weather();
    }

    public void show_tip_weather()
    {
        if (this.s_weather_temp_max == this.s_weather_temp_min)
            this.txt_weather_tip.text = string.Format(PlayerPrefs.GetString("weather_tip", "Average temperature {0}, lowest from {1}"), s_weather_temp_feels_like + "°C", s_weather_temp_min + "°C");
        else
            this.txt_weather_tip.text = string.Format(PlayerPrefs.GetString("weather_tip_2", "Average temperature {0}, lowest from {1} to {2}"), s_weather_temp_feels_like + "°C", s_weather_temp_min + "°C", s_weather_temp_max + "°C");
    }

    private static System.DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
        return dtDateTime;
    }

    public void sel_menu_func_app(int index)
    {
        this.sel_func = index;
        sel_func_app();
        this.show_func_function();
    }

    public void sel_menu_with_sound(int index)
    {
        this.sel_menu_func_app(index);
        this.play_sound();
    }

    private void sel_func_app()
    {
        for (int i = 0; i < this.panel_fnc_box.Length; i++)
        {
            this.panel_fnc_box[i].SetActive(false);
            if (this.btn_fnc_menu[i].transform.childCount > 0)
            {
                this.btn_fnc_menu[i].GetComponent<Image>().color = this.color_nomal;
                this.btn_fnc_menu[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                this.btn_fnc_menu[i].transform.GetChild(1).GetComponent<Text>().color = Color.white;
            }
            else
            {
                this.btn_fnc_menu[i].GetComponent<Image>().color = Color.white;
            }
        }
        this.panel_fnc_box[this.sel_func].SetActive(true);
        if (this.btn_fnc_menu[this.sel_func].transform.childCount > 0)
        {
            this.btn_fnc_menu[this.sel_func].GetComponent<Image>().color = this.carrot.color_highlight;
            this.btn_fnc_menu[this.sel_func].transform.GetChild(0).GetComponent<Image>().color = Color.black;
            this.btn_fnc_menu[this.sel_func].transform.GetChild(1).GetComponent<Text>().color = Color.black;
        }
        else
        {
            this.btn_fnc_menu[this.sel_func].GetComponent<Image>().color = this.carrot.color_highlight;
        }
        this.player_music.check_show_mini_player_where_music(this.sel_func);

        this.panel_menu_right_act.SetActive(true);
        this.panel_menu_right_music.SetActive(false);

        if (this.sel_func == 2)
        {
            this.panel_menu_right_act.SetActive(false);
            this.panel_menu_right_music.SetActive(true);
        }

        if (this.sel_func == 3)
        {
            this.get_character().reload_ui_character();
            if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait())
                this.panel_menu_right_act.SetActive(false);
            else
                this.panel_menu_right_act.SetActive(true);
        }

        if (this.sel_func == 4)
        {
            this.view.on_load();
            if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait())
                this.panel_menu_right_act.SetActive(false);
            else
                this.panel_menu_right_act.SetActive(true);
        }
    }

    public int get_index_sel_func()
    {
        return this.sel_func;
    }

    public void rate_app()
    {
        this.carrot.show_rate();
    }

    public void share_app()
    {
        this.carrot.show_share();
    }

    public void btn_show_login()
    {
        this.carrot.user.show_login(this.event_where_after_login);
    }

    public void event_where_after_login()
    {
        string s_name_user = this.carrot.user.get_data_user_login("name");
        PlayerPrefs.SetString("ten_user", s_name_user);
        this.command_dev.check();
    }

    public void btn_show_list_lang()
    {
        this.carrot.show_list_lang(act_after_load_lang);
    }

    private void act_after_load_lang(string s_data)
    {
        if (PlayerPrefs.GetString("app_function_sex_character", "0") == "0")
        {
            if (PlayerPrefs.GetString("sex", "0") == "0")
            {
                PlayerPrefs.SetString("character_sex", "1");
                PlayerPrefs.SetString("sex", "0");
            }
            else
            {
                PlayerPrefs.SetString("character_sex", "0");
                PlayerPrefs.SetString("sex", "1");
            }
        }
        this.check_manager_character();
        this.load("");
        this.command.clear_log_chat();
        this.command_voice.set_DetectionLanguage(PlayerPrefs.GetString("key_voice"));
    }

    public void open_sys(string s_action= "android.settings.SETTINGS")
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

    [ContextMenu("Delete all data app")]
    public void delete_all_data()
    {
        PlayerPrefs.DeleteAll();
        this.view.delete_background_image();
        this.carrot.delay_function(1f, this.Start);
    }

    public void set_item_cur_log_chat(Item_command_chat item_log)
    {
        this.item_cur_log_chat = item_log;
    }

    public void play_sound(int index_sound=0)
    {
        if (this.carrot.get_status_sound()) this.audio_sound[index_sound].Play();
    }

    public void show_report()
    {
        this.report.show(this.item_cur_log_chat);
    }

    private string get_key_weather_api()
    {
        int rand_index = UnityEngine.Random.Range(0, this.key_api_weather.Length);
        return this.key_api_weather[rand_index];
    }

    public void show_list_app_carrot()
    {
        this.carrot.show_list_carrot_app();
    }

    public void show_list_contact(IList list_contact)
    {
        if (list_contact.Count == 1)
        {
            IDictionary data_contacts = (IDictionary)list_contact[0];
            this.show_acc_by_id(data_contacts["id_device"].ToString());
        }
        else if (list_contact.Count > 1)
        {
            this.box_list = this.carrot.Create_Box(PlayerPrefs.GetString("acc_info", "Account information"), this.icon_contacts);
            for (int i = 0; i < list_contact.Count; i++)
            {
                IDictionary data_contacts = (IDictionary)list_contact[i];
                Carrot.Carrot_Box_Item item_contact = this.box_list.create_item("contact_item");
                string s_id = data_contacts["id_device"].ToString();
                item_contact.set_act(() => show_acc_by_id(s_id));
                item_contact.set_title(data_contacts["name"].ToString());
                item_contact.set_tip(data_contacts["sdt"].ToString() + " - " + data_contacts["address"].ToString());
                item_contact.set_icon(this.icon_contacts);
                if (data_contacts["avatar"] != null)
                {
                    this.carrot.get_img(data_contacts["avatar"].ToString(), item_contact.img_icon);
                    item_contact.img_icon.color = Color.white;
                }
            }

            this.box_list.update_color_table_row();
        }
    }

    public void show_acc_by_id(string s_id_user)
    {
        this.carrot.user.show_user_by_id(s_id_user, PlayerPrefs.GetString("lang", "en"));
    }

    public void hide_banner_ads()
    {
        this.carrot.ads.Destroy_Banner_Ad();
    }
    public string s_rep_key(string s_chat)
    {
        s_chat = s_chat.Replace("{ten_user}", PlayerPrefs.GetString("ten_user") + " ");
        s_chat = s_chat.Replace("{ten_nv}", this.get_character().get_name_character());
        s_chat = s_chat.Replace("{gio}", DateTime.Now.Hour.ToString());
        s_chat = s_chat.Replace("{phut}", DateTime.Now.Minute.ToString());
        s_chat = s_chat.Replace("{thu}", DateTime.Now.DayOfWeek.ToString());
        s_chat = s_chat.Replace("{ngay}", DateTime.Now.Date.ToString());
        s_chat = s_chat.Replace("{thang}", DateTime.Now.Month.ToString());
        s_chat = s_chat.Replace("{nam}", DateTime.Now.Year.ToString());
        s_chat = s_chat.Replace("{key_chat}", this.command.get_s_command_chat_last());
        return s_chat;
    }

    public string s_rep_run(string s_input, string s_key_word, string s_run)
    {
        return s_run.Replace("{key_word}", s_input.Replace(s_key_word, ""));
    }

    public WWWForm frm_act(string s_func)
    {
        WWWForm frm = this.carrot.frm_act(s_func);
        frm.AddField("sex", PlayerPrefs.GetString("sex", "0"));
        frm.AddField("limit_chat", PlayerPrefs.GetInt("setting_limit_chat", 3));
        frm.AddField("limit_day", DateTime.Now.DayOfWeek.ToString());
        frm.AddField("limit_date", DateTime.Now.Date.ToString());
        frm.AddField("limit_month", DateTime.Now.Month.ToString());
        frm.AddField("character_sex", PlayerPrefs.GetString("character_sex", "1"));
        if (this.carrot.user.get_id_user_login() != "") frm.AddField("user_id", this.carrot.user.get_id_user_login());
        return frm;
    }

    public void show_setting() { this.setting.show_setting(); }

    public void on_hit()
    {
        this.count_hit++;
        if (this.count_hit >= this.get_character().get_length_ani_hit())
        {
            this.count_hit = 0;
            this.carrot.play_vibrate();
            this.command.get_msg_hit();
        }
        this.get_character().play_ani_hit(this.count_hit);
        this.btn_chat_switch_function();
        this.play_sound(1);
        this.get_character().get_npc().transform.localPosition = Vector3.zero;
        this.view.mouseOrbit_Improved.Reset_pos();
        this.command_voice.on_input_mode_chat();
        this.live.off_live();
    }

    #region Scene Rotation
    public void act_change_Scene_Rotation()
    {
        this.carrot.delay_function(1.1f, this.check_Scene_Rotation);
    }

    public void check_Scene_Rotation()
    {
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait())
        {
            this.tr_scene_main.SetParent(this.area_scene_main_portait);
            this.tr_scene_menu_right.SetParent(this.area_scene_menu_right_portait);
            this.tr_scene_music_mini.SetParent(this.area_scene_music_mini_portait);
            this.tr_scene_environment.SetParent(this.area_scene_environment_portait);
            this.tr_scene_character.SetParent(this.area_scene_charactert_portait);
            this.view.change_scene_rotation(true);
            this.panel_inp_func.GetComponent<Image>().enabled = true;
            this.img_menu_bottom_border.enabled = true;
        }
        else
        {
            this.tr_scene_main.SetParent(this.area_scene_main_landspace);
            this.tr_scene_menu_right.SetParent(this.area_scene_menu_right_landspace);
            this.tr_scene_music_mini.SetParent(this.area_scene_music_mini_landspace);
            this.tr_scene_environment.SetParent(this.area_scene_environment_landspace);
            this.tr_scene_character.SetParent(this.area_scene_character_landspace);
            this.view.change_scene_rotation(false);
            this.panel_menu_right.SetActive(true);
            this.panel_inp_func.GetComponent<Image>().enabled = false;
            this.img_menu_bottom_border.enabled = false;
        }
        this.set_rect_main_tr(this.tr_scene_main);
        this.set_rect_main_tr(this.tr_scene_menu_right);
        this.set_rect_main_tr(this.tr_scene_music_mini);
        this.set_rect_main_tr(this.tr_scene_character);
    }

    private void set_rect_main_tr(Transform tr_area)
    {
        RectTransform rectTransform = tr_area.GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
    }
    #endregion

    #region shop_in_app
    /// <summary>Shop in app all product</summary>
    public void onBuySuccessCarrotPay(string id_product)
    {
        if (id_product == this.carrot.shop.get_id_by_index(2))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop_ads", "Remove ads"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            this.act_inapp_removeAds();
        }

        if (id_product == this.carrot.shop.get_id_by_index(7))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop_all_mp3", "Allows downloading all mp3 music files"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            this.act_inapp_unlock_downloadmp3();
        }

        if (id_product == this.carrot.shop.get_id_by_index(6))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop_fahsion", "Open all outfits"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            this.get_character().check_buy_success_character();
            this.act_inapp_unlock_all_character();
        }

        if (id_product == this.carrot.shop.get_id_by_index(4))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop_data", "Use chat data offline"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            PlayerPrefs.SetInt("is_buy_4", 1);
           this.command_storage.download_command_shop();
        }

        if (id_product == this.carrot.shop.get_id_by_index(0))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop_all_func", "Activate all functions"), PlayerPrefs.GetString("shop_buy_success", "Purchase successful! the function you purchased has been activated. Please restart the application to use it"));
            this.act_inapp_unlock_all_func();
        }

        if (id_product == this.carrot.shop.get_id_by_index(1))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop", "Shop"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            this.player_music.act_download_mp3_form_shop();
            this.player_music.playlist.on_pay_success();
        }

        if (id_product == this.carrot.shop.get_id_by_index(3))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop", "Shop"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            PlayerPrefs.SetInt("is_buy_3", 1);
            this.get_character().check_buy_success_character();
        }

        if (id_product == this.carrot.shop.get_id_by_index(5))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop", "Shop"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            PlayerPrefs.SetInt("is_buy_5", 1);
            this.get_character().check_buy_success_character();
        }

        if (id_product == this.carrot.shop.get_id_by_index(this.icon.index_buy_category_icon))
        {
            this.carrot.show_msg(PlayerPrefs.GetString("shop", "Shop"), PlayerPrefs.GetString("buy_inapp_success", "Payment success! you can now use the purchased function"));
            this.icon.act_buy_category_success();
        }
    }

    private void onRestoreSuccessCarrotPay(string[] arr_id)
    {
        for (int i = 0; i < arr_id.Length; i++)
        {
            string id_p = arr_id[i];
            if (id_p == this.carrot.shop.get_id_by_index(0)) this.act_inapp_unlock_all_func();
            if (id_p == this.carrot.shop.get_id_by_index(2)) this.act_inapp_removeAds();
            if (id_p == this.carrot.shop.get_id_by_index(6)) this.act_inapp_unlock_all_character();
            if (id_p == this.carrot.shop.get_id_by_index(7)) this.act_inapp_unlock_downloadmp3();
        }
    }

    public void buy_product(int index_p)
    {
        this.carrot.shop.buy_product(index_p);
    }

    public void restore_product()
    {
        this.carrot.show_loading();
        this.carrot.shop.restore_product();
    }

    private void act_inapp_unlock_all_func()
    {
        PlayerPrefs.SetInt("is_buy_0", 1);
        PlayerPrefs.SetInt("is_buy_1", 1);
        PlayerPrefs.SetInt("is_buy_2", 1);
        PlayerPrefs.SetInt("is_buy_3", 1);
        PlayerPrefs.SetInt("is_buy_4", 1);
        PlayerPrefs.SetInt("is_buy_5", 1);
        PlayerPrefs.SetInt("is_buy_6", 1);
        PlayerPrefs.SetInt("is_buy_7", 1);
        this.hide_banner_ads();
    }

    private void act_inapp_removeAds()
    {
        PlayerPrefs.SetInt("is_buy_2", 1);
        GameObject.Find("app").GetComponent<App>().hide_banner_ads();
    }

    private void act_inapp_unlock_downloadmp3()
    {
        PlayerPrefs.SetInt("is_buy_7", 1);
    }

    public void act_inapp_unlock_all_character()
    {
        PlayerPrefs.SetInt("is_buy_6", 1);
        this.get_character().check_buy_success_character();
    }
    #endregion
}
