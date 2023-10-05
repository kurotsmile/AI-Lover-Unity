using Carrot;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;
    public Image img_sound_voice_home;

    [Header("Asset Icon Setting")]
    public Sprite sp_icon_sys;
    public Sprite sp_icon_weather;
    public Sprite sp_icon_weather_pin;
    public Sprite sp_icon_user;
    public Sprite sp_icon_name_user;
    public Sprite sp_icon_sex_user;
    public Sprite sp_icon_sex_boy;
    public Sprite sp_icon_sex_girl;
    public Sprite sp_icon_sex_character;
    public Sprite sp_icon_select_color;
    public Sprite sp_icon_mix_color;
    public Sprite sp_icon_chat;
    public Sprite sp_icon_name_character;
    public Sprite sp_icon_voice;
    public Sprite sp_icon_voice_on;
    public Sprite sp_icon_voice_off;
    public Sprite sp_icon_voice_speed;
    public Sprite sp_icon_voice_type;
    public Sprite sp_icon_chat_limit;
    public Sprite sp_icon_chat_bubble;
    public Sprite sp_icon_shop;
    public Sprite sp_icon_shop_mp3;
    public Sprite sp_icon_shop_fahsion;
    public Sprite sp_icon_shop_data;
    public Sprite sp_icon_shop_all_func;
    public Sprite sp_icon_other;
    public Sprite sp_icon_on;
    public Sprite sp_icon_off;
    public Sprite sp_icon_ads;
    public Sprite sp_icon_buy;

    public AudioSource audio_source_setting;

    private string s_user_name;
    private string s_weather_pin;
    private string s_tts_type;
    private int limit_chat;
    private float voice_speech;
    private bool is_sound_voice;
    private bool is_bubble_icon;
    private Color32 color_select;

    private string character_sex;
    private string user_sex;
    private string s_color_select;

    private Carrot.Carrot_Box box_list;
    private Carrot.Carrot_Box box_setting;

    private Carrot.Carrot_Window_Input box_inp;
    private Carrot.Carrot_Box_Item item_name_user;
    private Carrot.Carrot_Box_Item item_name_npc;
    private Carrot.Carrot_Box_Item item_weather_pin;
    private Carrot.Carrot_Box_Item item_chat_limit;
    private Carrot.Carrot_Box_Item item_voice_speed;
    private Carrot.Carrot_Box_Item item_voice_type;

    private Carrot.Carrot_Box_Btn_Item btn_user_sex_boy;
    private Carrot.Carrot_Box_Btn_Item btn_user_sex_girl;
    private Carrot.Carrot_Box_Btn_Item btn_character_sex_boy;
    private Carrot.Carrot_Box_Btn_Item btn_character_sex_girl;
    private Carrot.Carrot_Box_Btn_Item btn_edit_sound;
    private Carrot.Carrot_Box_Btn_Item btn_edit_chat_bubble;

    private AudioClip[] audio_voice_sex_test = new AudioClip[2];

    private bool is_ads_rewarded_data = false;

    public void load_setting()
    {
        this.character_sex = PlayerPrefs.GetString("character_sex", "1");
        this.user_sex = PlayerPrefs.GetString("sex", "0");
        this.s_color_select = PlayerPrefs.GetString("color_select");
        this.s_tts_type= PlayerPrefs.GetString("tts_type","0");
        if (s_color_select != "")
        {
            Color newCol;
            if (ColorUtility.TryParseHtmlString("#" + s_color_select, out newCol))
            {
                this.color_select = newCol;
                this.app.carrot.set_color(this.color_select);
            }
            else
                this.color_select = this.app.carrot.color_highlight;

            this.app.update_color_select();
        }
        else
        {
            this.color_select = this.app.carrot.color_highlight;
            this.s_color_select = ColorUtility.ToHtmlStringRGBA(this.color_select);
            this.app.update_color_select();
        }

        this.limit_chat = PlayerPrefs.GetInt("setting_limit_chat", 3);
        this.voice_speech = PlayerPrefs.GetFloat("setting_voice_speech", 1.2f);

        if (PlayerPrefs.GetInt("is_bubble_icon", 1) == 1)
            this.is_bubble_icon = true;
        else
            this.is_bubble_icon = false;

        if (PlayerPrefs.GetInt("is_sound_voice", 1) == 1)
        {
            this.img_sound_voice_home.sprite = this.sp_icon_voice_on;
            this.is_sound_voice = true;
        }
        else
        {
            this.img_sound_voice_home.sprite = this.sp_icon_voice_off;
            this.is_sound_voice = false;
        }
            
    }

    public void show_setting()
    {
        string s_ten_user = PlayerPrefs.GetString("ten_user", "");
        s_weather_pin = PlayerPrefs.GetString("weather_pin");
        this.character_sex = PlayerPrefs.GetString("character_sex", "1");
        this.user_sex = PlayerPrefs.GetString("sex", "0");
        this.voice_speech = PlayerPrefs.GetFloat("setting_voice_speech", 1.2f);

        if (PlayerPrefs.GetInt("is_sound_voice", 1) == 1)
            this.is_sound_voice = true;
        else
            this.is_sound_voice = false;

        if (PlayerPrefs.GetInt("is_bubble_icon", 1) == 1)
            this.is_bubble_icon = true;
        else
            this.is_bubble_icon = false;

        if (s_ten_user == "")
        {
            if (this.app.carrot.user.get_id_user_login() != "") s_user_name = this.app.carrot.user.get_data_user_login("name");
            else s_user_name = "";
        }
        else
        {
            s_user_name = s_ten_user;
        }

        this.box_setting = this.app.carrot.Create_Setting();
        box_setting.set_act_before_closing(this.act_close_setting);

        Carrot.Carrot_Box_Item_group group_sys = box_setting.add_item_group("sys");
        group_sys.set_icon(this.sp_icon_sys);
        group_sys.transform.SetSiblingIndex(0);

        group_sys.add_item(box_setting.area_all_item.GetChild(1).GetComponent<Carrot.Carrot_Box_Item>());
        group_sys.add_item(box_setting.area_all_item.GetChild(2).GetComponent<Carrot.Carrot_Box_Item>());
        group_sys.add_item(box_setting.area_all_item.GetChild(3).GetComponent<Carrot.Carrot_Box_Item>());
        group_sys.add_item(box_setting.area_all_item.GetChild(4).GetComponent<Carrot.Carrot_Box_Item>());

        item_weather_pin = box_setting.create_item("weather_pin");
        item_weather_pin.set_icon(this.sp_icon_weather);
        item_weather_pin.set_title("Address to see the weather");
        item_weather_pin.set_tip("Update your location to use the weather view function (format: road, city, country)");
        item_weather_pin.set_lang_data("setting_weather_pin", "setting_weather_pin_tip");
        item_weather_pin.load_lang_data();

        item_weather_pin.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_weather_pin.load_lang_data();
        item_weather_pin.set_val(s_weather_pin);

        item_weather_pin.set_act(() => act_show_box_edit_weather_pin());
        Carrot.Carrot_Box_Btn_Item btn_edit_weather = item_weather_pin.create_item();
        btn_edit_weather.set_icon(this.app.carrot.user.icon_user_edit);
        btn_edit_weather.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_weather.GetComponent<Button>());

        Carrot.Carrot_Box_Btn_Item btn_pin_weather = item_weather_pin.create_item();
        btn_pin_weather.set_icon(this.sp_icon_weather_pin);
        btn_pin_weather.set_color(this.app.carrot.color_highlight);
        btn_pin_weather.set_act(() => this.act_weather_pin());

        group_sys.add_item(item_weather_pin);

        Carrot.Carrot_Box_Item item_color_select = box_setting.create_item("color_select");
        item_color_select.set_icon(this.sp_icon_select_color);
        item_color_select.set_title("Color Select");
        item_color_select.set_tip("The color of the selected item and the highlighted values");
        item_color_select.set_lang_data("setting_color", "setting_color_tip");
        item_color_select.load_lang_data();
        item_color_select.set_act(() => this.app.carrot.theme.show_list_color(this.act_select_color));
        item_color_select.transform.SetSiblingIndex(4);

        Carrot.Carrot_Box_Btn_Item btn_color_list = item_color_select.create_item();
        btn_color_list.set_icon(this.app.carrot.icon_carrot_all_category);
        btn_color_list.set_color(this.app.carrot.color_highlight);
        Destroy(btn_color_list.GetComponent<Button>());

        Carrot.Carrot_Box_Btn_Item btn_color_mix = item_color_select.create_item();
        btn_color_mix.set_icon(this.sp_icon_mix_color);
        btn_color_mix.set_color(this.app.carrot.color_highlight);
        btn_color_mix.set_act(() => this.app.carrot.theme.show_mix_color(this.color_select, this.act_select_color));
        group_sys.add_item(item_color_select);

        Carrot.Carrot_Box_Item_group group_user = box_setting.add_item_group("user_group");
        group_user.set_icon(this.sp_icon_user);

        item_name_user = box_setting.create_item("name_user");
        item_name_user.set_icon(this.sp_icon_name_user);
        item_name_user.set_title("Your name");
        item_name_user.set_tip("Enter your name for the character to name and use in several alternative app functions with the keyword {ten_user}");
        item_name_user.set_lang_data("setting_your_name", "setting_your_name_tip");
        if (s_user_name != "")
        {
            item_name_user.set_type(Carrot.Box_Item_Type.box_value_txt);
            item_name_user.set_val(s_user_name);
        }
        item_name_user.load_lang_data();
        item_name_user.set_act(() => this.act_show_box_edit_name_user());
        Carrot.Carrot_Box_Btn_Item btn_edit_name = item_name_user.create_item();
        btn_edit_name.set_icon(this.app.carrot.user.icon_user_edit);
        btn_edit_name.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_name.GetComponent<Button>());
        group_user.add_item(item_name_user);

        Carrot.Carrot_Box_Item item_user_sex = box_setting.create_item("user_sex");
        item_user_sex.set_icon(this.sp_icon_sex_user);
        item_user_sex.set_title("Your gender");
        item_user_sex.set_tip("Choose your gender so that the system recommends choosing the right character for you");
        item_user_sex.set_lang_data("setting_your_sex", "setting_your_sex_tip");
        item_user_sex.load_lang_data();
        item_user_sex.set_act(() => act_show_box_edit_weather_pin());
        this.btn_user_sex_boy = item_user_sex.create_item();
        this.btn_user_sex_boy.set_icon(this.sp_icon_sex_boy);
        this.btn_user_sex_boy.set_act(() => this.act_change_user_sex("0"));
        if (this.user_sex == "0")
            this.btn_user_sex_boy.set_color(this.app.carrot.color_highlight);
        else
            this.btn_user_sex_boy.set_color(Color.gray);

        this.btn_user_sex_girl = item_user_sex.create_item();
        this.btn_user_sex_girl.set_icon(this.sp_icon_sex_girl);
        this.btn_user_sex_girl.set_act(() => this.act_change_user_sex("1"));
        if (this.user_sex == "1")
            this.btn_user_sex_girl.set_color(this.app.carrot.color_highlight);
        else
            this.btn_user_sex_girl.set_color(Color.gray);

        group_user.add_item(item_user_sex);

        this.item_name_npc = this.box_setting.create_item("character_name");
        this.item_name_npc.set_icon(this.sp_icon_name_character);
        this.item_name_npc.set_title("Character name");
        this.item_name_npc.set_tip("You can rename the character here");
        this.item_name_npc.set_lang_data("character_name", "character_name_tip");
        this.item_name_npc.load_lang_data();
        this.item_name_npc.set_type(Carrot.Box_Item_Type.box_value_txt);
        this.item_name_npc.set_val(this.app.get_character().get_name_character());
        this.item_name_npc.set_act(() => this.act_show_box_edit_name_npc());
        Carrot.Carrot_Box_Btn_Item btn_edit_name_npc = item_name_npc.create_item();
        btn_edit_name_npc.set_icon(this.app.carrot.user.icon_user_edit);
        btn_edit_name_npc.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_name_npc.GetComponent<Button>());
        group_user.add_item(this.item_name_npc);

        Carrot.Carrot_Box_Item item_character_sex = box_setting.create_item("character_sex");
        item_character_sex.set_icon(this.sp_icon_sex_character);
        item_character_sex.set_title("Character gender");
        item_character_sex.set_tip("You can change your character's gender");
        item_character_sex.set_lang_data("setting_char_sex", "setting_char_sex_tip");
        item_character_sex.load_lang_data();
        this.btn_character_sex_boy = item_character_sex.create_item();
        this.btn_character_sex_boy.set_icon(this.sp_icon_sex_boy);
        if (this.character_sex == "0")
            this.btn_character_sex_boy.set_color(this.app.carrot.color_highlight);
        else
            this.btn_character_sex_boy.set_color(Color.gray);
        this.btn_character_sex_boy.set_act(() => this.act_change_character_sex("0"));

        this.btn_character_sex_girl = item_character_sex.create_item();
        this.btn_character_sex_girl.set_icon(this.sp_icon_sex_girl);
        if (this.character_sex == "1")
            this.btn_character_sex_girl.set_color(this.app.carrot.color_highlight);
        else
            this.btn_character_sex_girl.set_color(Color.gray);
        this.btn_character_sex_girl.set_act(() => this.act_change_character_sex("1"));

        group_user.add_item(item_character_sex);

        Carrot.Carrot_Box_Item_group group_chat = box_setting.add_item_group("group_chat");
        group_chat.set_icon(this.sp_icon_chat);

        Carrot.Carrot_Box_Item item_voice = this.box_setting.create_item("setting_sound");
        item_voice.set_icon(this.sp_icon_voice);
        item_voice.set_title("Chat audio");
        item_voice.set_tip("Toggle pronunciation mode, read chat content");
        item_voice.set_lang_data("setting_sound", "setting_sound_tip");
        item_voice.load_lang_data();
        item_voice.set_act(this.act_change_status_voice);

        btn_edit_sound = item_voice.create_item();
        if (this.is_sound_voice)
            btn_edit_sound.set_icon(this.sp_icon_on);
        else
            btn_edit_sound.set_icon(this.sp_icon_off);
        btn_edit_sound.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_sound.GetComponent<Button>());
        group_chat.add_item(item_voice);

        this.item_voice_speed = box_setting.create_item("item_voice_speed");
        item_voice_speed.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_voice_speed.set_icon(this.sp_icon_voice_speed);
        item_voice_speed.set_title("Pronunciation speed");
        item_voice_speed.set_tip("You can adjust the character's speech speed by adjusting the slider below");
        item_voice_speed.set_lang_data("voice_speed", "voice_speed_tip");
        item_voice_speed.set_val(this.voice_speech.ToString());
        item_voice_speed.set_act(act_show_edit_voice_speed);
        item_voice_speed.load_lang_data();

        Carrot.Carrot_Box_Btn_Item btn_edit_voice_speed = item_voice_speed.create_item();
        btn_edit_voice_speed.set_icon(this.app.carrot.user.icon_user_edit);
        btn_edit_voice_speed.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_voice_speed.GetComponent<Button>());
        group_chat.add_item(item_voice_speed);

        this.item_voice_type = box_setting.create_item("item_voice_type");
        item_voice_type.set_icon(this.sp_icon_voice_type);
        item_voice_type.set_title("Voice Type");
        item_voice_type.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        item_voice_type.set_tip("Change the voice pronunciation platform");
        item_voice_type.set_lang_data("voice_type", "voice_type_tip");
        item_voice_type.load_lang_data();
        item_voice_type.set_act(() => act_show_chat_limit());

        item_voice_type.dropdown_val.options.Clear();
        item_voice_type.dropdown_val.options.Add(new Dropdown.OptionData("voice_type_google_and_tts"));
        item_voice_type.dropdown_val.options.Add(new Dropdown.OptionData("voice_type_google"));
        item_voice_type.dropdown_val.options.Add(new Dropdown.OptionData("voice_type_tts"));

        item_voice_type.set_val(PlayerPrefs.GetString("tts_type", "0"));
        item_voice_type.dropdown_val.onValueChanged.AddListener(this.act_change_voice_type);

        Carrot_Box_Btn_Item btn_help_setting_voice_type = this.item_voice_type.create_item();
        btn_help_setting_voice_type.set_icon(this.app.command.icon_info_chat);
        btn_help_setting_voice_type.set_color(this.app.carrot.color_highlight);
        btn_help_setting_voice_type.set_act(() => this.act_open_setting_voice_type());
        group_chat.add_item(item_voice_type);

        this.item_chat_limit = box_setting.create_item("chat_limit");
        item_chat_limit.set_icon(this.sp_icon_chat_limit);
        item_chat_limit.set_title("Chat content limit");
        item_chat_limit.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_chat_limit.set_tip("Choose the level of vulgarity and sexuality of the content of the chat with the character");
        item_chat_limit.set_lang_data("chat_limit", "chat_limit_tip");
        item_chat_limit.load_lang_data();
        item_chat_limit.set_val(this.limit_chat.ToString());
        item_chat_limit.set_act(() => act_show_chat_limit());

        Carrot.Carrot_Box_Btn_Item btn_edit_limit_chat = item_chat_limit.create_item();
        btn_edit_limit_chat.set_icon(this.app.carrot.user.icon_user_edit);
        btn_edit_limit_chat.set_color(this.app.carrot.color_highlight);
        Destroy(btn_edit_limit_chat.GetComponent<Button>());
        group_chat.add_item(item_chat_limit);

        Carrot.Carrot_Box_Item item_chat_bubble = box_setting.create_item("chat_bubble");
        item_chat_bubble.set_icon(this.sp_icon_chat_bubble);
        item_chat_bubble.set_title("Bubble chat icon");
        item_chat_bubble.set_tip("The icon depicts the character's answer");
        item_chat_bubble.set_lang_data("setting_bubble_icon", "setting_bubble_icon_tip");
        item_chat_bubble.set_act(act_change_value_chat_bubble);
        item_chat_bubble.load_lang_data();

        this.btn_edit_chat_bubble = item_chat_bubble.create_item();
        if (this.is_bubble_icon)
            this.btn_edit_chat_bubble.set_icon(this.sp_icon_on);
        else
            this.btn_edit_chat_bubble.set_icon(this.sp_icon_off);
        this.btn_edit_chat_bubble.set_color(this.app.carrot.color_highlight);
        Destroy(this.btn_edit_chat_bubble.GetComponent<Button>());
        group_chat.add_item(item_chat_bubble);

        Carrot.Carrot_Box_Item_group shop_group = box_setting.add_item_group("shop_group");
        shop_group.set_icon(this.sp_icon_shop);

        Carrot.Carrot_Box_Item item_shop_mp3 = box_setting.create_item("shop_mp3");
        item_shop_mp3.set_icon(this.sp_icon_shop_mp3);
        item_shop_mp3.set_title("Download Mp3 music");
        item_shop_mp3.set_tip("Unlock mp3 music download function");
        item_shop_mp3.set_lang_data("shop_all_mp3", "shop_all_mp3_tip");
        item_shop_mp3.load_lang_data();
        item_shop_mp3.set_act(() => this.app.carrot.buy_product(7));
        Carrot.Carrot_Box_Btn_Item btn_buy_mp3 = item_shop_mp3.create_item();
        btn_buy_mp3.set_icon(this.sp_icon_buy);
        btn_buy_mp3.set_color(this.app.carrot.color_highlight);
        Destroy(btn_buy_mp3.GetComponent<Button>());
        if (this.check_buy_product(7)) btn_buy_mp3.set_icon(this.app.carrot.icon_carrot_done);

        shop_group.add_item(item_shop_mp3);

        Carrot.Carrot_Box_Item item_shop_fahsion = box_setting.create_item("shop_fahsion");
        item_shop_fahsion.set_icon(this.sp_icon_shop_fahsion);
        item_shop_fahsion.set_title("Use all costumes and characters");
        item_shop_fahsion.set_tip("Unlock and use all character's costumes");
        item_shop_fahsion.set_lang_data("shop_fahsion", "shop_fahsion_tip");
        item_shop_fahsion.load_lang_data();
        item_shop_fahsion.set_act(() => this.app.carrot.buy_product(6));
        Carrot.Carrot_Box_Btn_Item btn_buy_fahsion = item_shop_fahsion.create_item();
        btn_buy_fahsion.set_icon(this.sp_icon_buy);
        btn_buy_fahsion.set_color(this.app.carrot.color_highlight);
        Destroy(btn_buy_fahsion.GetComponent<Button>());
        shop_group.add_item(item_shop_fahsion);
        if (this.check_buy_product(6)) btn_buy_fahsion.set_icon(this.app.carrot.icon_carrot_done);

        Carrot.Carrot_Box_Item item_shop_data = box_setting.create_item("shop_data");
        item_shop_data.set_icon(this.sp_icon_shop_data);
        item_shop_data.set_title("Offline chat data");
        item_shop_data.set_tip("You can still chat with the application without a network connection");
        item_shop_data.set_lang_data("shop_data", "shop_data_tip");
        item_shop_data.set_act(() => this.app.carrot.buy_product(4));
        item_shop_data.load_lang_data();
        Carrot.Carrot_Box_Btn_Item btn_buy_data = item_shop_data.create_item();
        btn_buy_data.set_icon(this.sp_icon_buy);
        btn_buy_data.set_color(this.app.carrot.color_highlight);
        Destroy(btn_buy_data.GetComponent<Button>());

        if (this.check_buy_product(4))
        {
            btn_buy_data.set_icon(this.app.carrot.icon_carrot_done);
        }
        else
        {
            Carrot.Carrot_Box_Btn_Item btn_ads_data = item_shop_data.create_item();
            btn_ads_data.set_icon(this.sp_icon_ads);
            btn_ads_data.set_color(this.app.carrot.color_highlight);
            btn_ads_data.set_act(act_whatch_ads_rewarded_data);
        }
        shop_group.add_item(item_shop_data);

        Carrot.Carrot_Box_Item item_shop_all_func = box_setting.create_item("shop_all_fun");
        item_shop_all_func.set_icon(this.sp_icon_shop_all_func);
        item_shop_all_func.set_title("Full use of functions");
        item_shop_all_func.set_tip("Make full use of the functions listed above");
        item_shop_all_func.set_lang_data("shop_all_func", "shop_all_func_tip");
        item_shop_all_func.load_lang_data();
        item_shop_all_func.set_act(() => this.app.carrot.buy_product(0));
        Carrot.Carrot_Box_Btn_Item btn_buy_all_func = item_shop_all_func.create_item();
        btn_buy_all_func.set_icon(this.sp_icon_buy);
        btn_buy_all_func.set_color(this.app.carrot.color_highlight);
        Destroy(btn_buy_all_func.GetComponent<Button>());
        shop_group.add_item(item_shop_all_func);
        if (this.check_buy_product(0)) item_shop_all_func.gameObject.SetActive(false);

        if (this.check_buy_product(0))
        {
            btn_buy_all_func.set_icon(this.app.carrot.icon_carrot_done);
            btn_buy_data.set_icon(this.app.carrot.icon_carrot_done);
            btn_buy_fahsion.set_icon(this.app.carrot.icon_carrot_done);
            btn_buy_mp3.set_icon(this.app.carrot.icon_carrot_done);
            btn_buy_fahsion.set_icon(this.app.carrot.icon_carrot_done);
        }

        Carrot.Carrot_Box_Item_group other_group = box_setting.add_item_group("other_group");
        other_group.set_icon(this.sp_icon_other);

        box_setting.area_all_item.GetChild(5).SetSiblingIndex(24);
        box_setting.area_all_item.GetChild(7).SetSiblingIndex(30);
        box_setting.area_all_item.GetChild(8).SetSiblingIndex(31);
        box_setting.area_all_item.GetChild(6).SetSiblingIndex(32);
        box_setting.area_all_item.GetChild(6).SetSiblingIndex(33);
        box_setting.area_all_item.GetChild(6).SetSiblingIndex(34);

        Carrot.Carrot_Box_Item item_removeads = box_setting.area_all_item.GetChild(19).GetComponent<Carrot.Carrot_Box_Item>();
        item_removeads.set_lang_data("shop_ads", "shop_ads_tip");
        item_removeads.load_lang_data();
        Carrot.Carrot_Box_Btn_Item btn_by_ads = item_removeads.create_item();
        if(this.app.carrot.ads.get_status_ads())
            btn_by_ads.set_icon(this.sp_icon_buy);
        else
            btn_by_ads.set_icon(this.app.carrot.icon_carrot_done);
        btn_by_ads.set_color(this.app.carrot.color_highlight);
        Destroy(btn_by_ads.GetComponent<Button>());
        if (this.check_buy_product(2)) item_removeads.gameObject.SetActive(false);

        shop_group.add_item(item_removeads);
        Carrot.Carrot_Box_Item item_share = box_setting.area_all_item.GetChild(25).GetComponent<Carrot.Carrot_Box_Item>();
        other_group.add_item(item_share);

        Carrot.Carrot_Box_Item item_restore = box_setting.area_all_item.GetChild(26).GetComponent<Carrot.Carrot_Box_Item>();
        other_group.add_item(item_restore);

        Carrot.Carrot_Box_Item item_rate = box_setting.area_all_item.GetChild(27).GetComponent<Carrot.Carrot_Box_Item>();
        other_group.add_item(item_rate);

        other_group.add_item(box_setting.area_all_item.GetChild(28).GetComponent<Carrot.Carrot_Box_Item>());
        other_group.add_item(box_setting.area_all_item.GetChild(29).GetComponent<Carrot.Carrot_Box_Item>());

        group_sys.add_item(box_setting.area_all_item.GetChild(1).GetComponent<Carrot.Carrot_Box_Item>());
        group_sys.add_item(box_setting.area_all_item.GetChild(5).GetComponent<Carrot.Carrot_Box_Item>());

        this.box_setting.update_color_table_row();

        string setting_url_sound_test_sex0 = PlayerPrefs.GetString("setting_url_sound_test_sex0");
        string setting_url_sound_test_sex1 = PlayerPrefs.GetString("setting_url_sound_test_sex1");
        if(setting_url_sound_test_sex0!="") StartCoroutine(this.get_audio_setting_sex_test(setting_url_sound_test_sex0, 0));
        if(setting_url_sound_test_sex1!="") StartCoroutine(this.get_audio_setting_sex_test(setting_url_sound_test_sex1, 1));
    } 

    private void act_close_setting()
    {
        this.GetComponent<Voice_Command>().set_DetectionLanguage(PlayerPrefs.GetString("key_voice"));
        this.app.command_dev.check();
    }

    private void act_show_box_edit_weather_pin()
    {
        string s_title = PlayerPrefs.GetString("setting_weather_pin", "Address to see the weather");
        string s_tip = PlayerPrefs.GetString("setting_weather_pin_tip", "Update your location to use the weather view function (format: road, city, country)");
        this.box_inp = this.app.carrot.show_input(s_title,s_tip, this.s_weather_pin);
        this.box_inp.set_act_done(act_done_box_edit_weather_address);
    }

    private void act_weather_pin()
    {
        this.app.carrot.location.get_location(get_location_success);
    }

    private void get_location_success(LocationInfo l)
    {
        PlayerPrefs.SetFloat("weather_longitude", l.longitude);
        PlayerPrefs.SetFloat("weather_latitude", l.latitude);
        StartCoroutine(this.GetComponent<App>().get_weather_buy_lot_and_lat());
    }

    private void act_done_box_edit_weather_address(string s_address)
    {
        PlayerPrefs.SetString("weather_pin", s_address);
        if (this.box_inp != null) this.box_inp.close();
        this.item_weather_pin.set_val(s_address);
        StartCoroutine(this.GetComponent<App>().get_weather_buy_address());
    }

    private void act_show_box_edit_name_user()
    {
        string s_title = PlayerPrefs.GetString("setting_your_name", "Your name");
        string s_tip = PlayerPrefs.GetString("setting_your_name_tip", "Enter your name for the character to name and use in several alternative app functions with the keyword {ten_user}");
        this.box_inp = this.app.carrot.show_input(s_title, s_tip, this.s_user_name);
        this.box_inp.set_act_done(act_done_name_user_input);
    }

    private void act_done_name_user_input(string s_name)
    {
        PlayerPrefs.SetString("ten_user", s_name);
        if (this.box_inp != null) this.box_inp.close();
        this.item_name_user.set_val(s_name);
    }

    private void act_show_box_edit_name_npc()
    {
        string s_title = PlayerPrefs.GetString("character_name", "Character name");
        string s_tip = PlayerPrefs.GetString("character_name_tip", "You can rename the character here");
        this.box_inp = this.app.carrot.show_input(s_title, s_tip, this.app.get_character().get_name_character());
        this.box_inp.set_act_done(act_done_name_npc_input);
    }

    private void act_done_name_npc_input(string s_name)
    {
        PlayerPrefs.SetString("ten_user", s_name);
        if (this.box_inp != null) this.box_inp.close();
        this.app.get_character().set_character_name(s_name);
    }

    private void act_select_color(Color32 color_new)
    {
        this.color_select = color_new;
        string s_color_sys=ColorUtility.ToHtmlStringRGBA(color_new);
        PlayerPrefs.SetString("color_select", s_color_sys);
        this.box_setting.UI.set_theme(color_new);
        this.app.carrot.set_color(color_new);
        this.app.update_color_select();
        if (this.box_list != null) this.box_list.close();
        this.box_setting.update_color_table_row();
        this.change_color_all_item_setting(color_new);
    }

    private void change_color_all_item_setting(Color32 color_new)
    {
        foreach (Transform tr in this.box_setting.area_all_item)
        {
            if (tr.GetComponent<Carrot.Carrot_Box_Item>())
            {
                Carrot.Carrot_Box_Item item_setting = tr.GetComponent<Carrot.Carrot_Box_Item>();
                item_setting.txt_tip.color = color_new;

                if (item_setting.area_all_btn_extension.childCount > 0)
                {
                    foreach(Transform btn in item_setting.area_all_btn_extension)
                    {
                        btn.GetComponent<Carrot.Carrot_Box_Btn_Item>().set_color(color_new);
                    }
                }
            }
        }
    }

    private void act_change_character_sex(string s_index_sex) {
        PlayerPrefs.SetString("character_sex", s_index_sex);
        this.character_sex = s_index_sex;
        this.app.check_manager_character();
        string s_name_char = this.app.get_character().get_name_character();
        this.item_name_npc.set_val(s_name_char);
        this.btn_character_sex_boy.set_color(Color.grey);
        this.btn_character_sex_girl.set_color(Color.grey);
        if (s_index_sex == "0")
        {
            this.btn_character_sex_boy.set_color(this.app.carrot.color_highlight);
            this.audio_source_setting.clip = this.audio_voice_sex_test[1];
        }

        if (s_index_sex == "1") {
            this.btn_character_sex_girl.set_color(this.app.carrot.color_highlight);
            this.audio_source_setting.clip = this.audio_voice_sex_test[0];
        }
    }

    private void act_change_user_sex(string index_sex)
    {
        PlayerPrefs.SetString("sex", index_sex);
        this.user_sex = index_sex;
        this.btn_user_sex_boy.set_color(Color.grey);
        this.btn_user_sex_girl.set_color(Color.grey);
        if (index_sex == "0") this.btn_user_sex_boy.set_color(this.app.carrot.color_highlight);
        if (index_sex == "1") this.btn_user_sex_girl.set_color(this.app.carrot.color_highlight);
    }

    private void act_show_chat_limit()
    {
        string s_chat_limit_txt =this.get_label_limit(this.limit_chat);
        this.box_inp = this.app.carrot.show_input(PlayerPrefs.GetString("chat_limit", "Chat content limit"), s_chat_limit_txt, this.limit_chat.ToString());
        this.box_inp.set_inp_type(Carrot.Window_Input_value_Type.slider);
        this.box_inp.inp_slider.wholeNumbers = true;
        this.box_inp.inp_slider.minValue = 1;
        this.box_inp.inp_slider.maxValue = 4;
        this.box_inp.inp_slider.value = this.limit_chat;
        this.box_inp.inp_slider.onValueChanged.AddListener(act_change_value_limit);
        this.box_inp.set_act_done(act_done_change_limit_chat);
    }

    public string get_label_limit(int index_limit)
    {
        return PlayerPrefs.GetString("limit_chat_"+index_limit);
    }

    private void act_change_value_limit(float val_limit)
    {
        string s_chat_limit_txt = PlayerPrefs.GetString("limit_chat_" + int.Parse(val_limit.ToString()));
        this.box_inp.set_tip(s_chat_limit_txt);
    }

    private void act_done_change_limit_chat(string s_value)
    {
        string s_chat_limit_txt = PlayerPrefs.GetString("limit_chat_" + int.Parse(s_value.ToString()));
        this.limit_chat = int.Parse(s_value);
        this.item_chat_limit.set_val(s_chat_limit_txt);
        PlayerPrefs.SetInt("setting_limit_chat", int.Parse(s_value));
        if (this.box_inp != null) this.box_inp.close();
    }

    public void act_change_status_voice()
    {
        string s_title = PlayerPrefs.GetString("setting_sound", "Chat audio");
        if (this.is_sound_voice)
        {
            this.is_sound_voice = false;
            PlayerPrefs.SetInt("is_sound_voice",0);
            this.app.carrot.show_msg(s_title,PlayerPrefs.GetString("setting_off", "Turn off"));
            if(this.btn_edit_sound!=null) this.btn_edit_sound.set_icon(this.sp_icon_off);
            this.img_sound_voice_home.sprite = this.sp_icon_voice_off;
        }
        else
        {
            this.is_sound_voice = true;
            PlayerPrefs.SetInt("is_sound_voice",1);
            this.app.carrot.show_msg(s_title, PlayerPrefs.GetString("setting_on", "Turn on"));
            if (this.btn_edit_sound != null) this.btn_edit_sound.set_icon(this.sp_icon_on);
            this.img_sound_voice_home.sprite = this.sp_icon_voice_on;
        }
    }

    private void act_show_edit_voice_speed()
    {
        string s_title = PlayerPrefs.GetString("voice_speed", "Pronunciation speed");
        this.box_inp = this.app.carrot.show_input(s_title, this.voice_speech.ToString() + "/s", this.voice_speech.ToString(),Carrot.Window_Input_value_Type.slider);
        this.box_inp.set_icon(this.sp_icon_voice_speed);
        this.box_inp.inp_slider.wholeNumbers = false;
        this.box_inp.inp_slider.minValue = 0.8f;
        this.box_inp.inp_slider.maxValue = 1.8f;
        this.box_inp.inp_slider.value = this.voice_speech;
        this.box_inp.set_act_done(act_done_edit_voice_speed);
        this.box_inp.inp_slider.onValueChanged.AddListener(act_change_value_slider_voice_speed);
    }

    private void act_change_value_slider_voice_speed(float speed_voice)
    {
        this.audio_source_setting.pitch = speed_voice;
        this.audio_source_setting.Play();
        this.box_inp.set_tip(speed_voice.ToString() + "/s");
    }

    private void act_done_edit_voice_speed(string s_value)
    {
        this.voice_speech = float.Parse(s_value);
        PlayerPrefs.SetFloat("setting_voice_speech", this.voice_speech);
        this.item_voice_speed.set_val(this.voice_speech.ToString());
        if (this.box_inp != null) this.box_inp.close();
    }

    private void act_change_value_chat_bubble()
    {
        if (this.is_bubble_icon)
        {
            this.is_bubble_icon = false;
            this.btn_edit_chat_bubble.set_icon(this.sp_icon_off);
        }
        else
        {
            this.is_bubble_icon = true;
            this.btn_edit_chat_bubble.set_icon(this.sp_icon_on);
        }
        this.app.carrot.play_sound_click();
    }

    private void act_whatch_ads_rewarded_data()
    {
        this.app.carrot.ads.show_ads_Rewarded();
        this.is_ads_rewarded_data = true;
    }

    public void act_ads_rewarded_success()
    {
        if (this.is_ads_rewarded_data)
        {
            this.app.carrot.show_msg("Watch ads to receive rewards", "Get Success Rewards!", Carrot.Msg_Icon.Success);
            this.GetComponent<Command_storage>().download_command_shop();
            this.is_ads_rewarded_data = false;
        }
    }

    IEnumerator get_audio_setting_sex_test(string s_url_audio, int index)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log(www.error);
            else
                this.audio_voice_sex_test[index] = DownloadHandlerAudioClip.GetContent(www);

            if (this.character_sex == "1")
                this.audio_source_setting.clip = this.audio_voice_sex_test[0];
            else
                this.audio_source_setting.clip = this.audio_voice_sex_test[1];
        }
    }

    public int get_limit_chat()
    {
        return this.limit_chat;
    }

    public bool check_buy_product(int index)
    {
        if (PlayerPrefs.GetInt("is_buy_"+ index, 0) == 1)
            return true;
        else
            return false;
    }

    public void set_name_address_weather(string s_name)
    {
        if (this.item_weather_pin != null)
        {
            this.app.carrot.show_msg(this.item_weather_pin.txt_name.text, "Address confirmed:"+s_name);
            this.item_weather_pin.set_val(s_name);
        }
    }

    public void show_shop()
    {
        Carrot.Carrot_Box box_shop=this.app.carrot.Create_Box("box_shop");
        box_shop.set_title(PlayerPrefs.GetString("shop","Shop"));
        box_shop.set_icon(this.sp_icon_shop);

        Carrot.Carrot_Box_Item item_shop_ads = box_shop.create_item("shop_ads");
        item_shop_ads.set_icon(this.sp_icon_ads);
        item_shop_ads.set_title("Remove Ads");
        item_shop_ads.set_tip("Buy and remove advertising function, No ads in the app");
        item_shop_ads.set_lang_data("shop_ads", "shop_ads_tip");
        item_shop_ads.load_lang_data();
        item_shop_ads.set_act(() => this.app.carrot.buy_product(this.app.carrot.index_inapp_remove_ads));
        this.add_btn_buy(item_shop_ads, this.app.carrot.index_inapp_remove_ads);

        Carrot.Carrot_Box_Item item_shop_data = box_shop.create_item("shop_data");
        item_shop_data.set_icon(this.sp_icon_shop_data);
        item_shop_data.set_title("Offline chat data");
        item_shop_data.set_tip("You can still chat with the application without a network connection");
        item_shop_data.set_lang_data("shop_data", "shop_data_tip");
        item_shop_data.load_lang_data();
        item_shop_data.set_act(() => this.app.carrot.buy_product(4));
        this.add_btn_buy(item_shop_data,4);

        Carrot.Carrot_Box_Item item_shop_mp3 = box_shop.create_item("shop_mp3");
        item_shop_mp3.set_icon(this.sp_icon_shop_mp3);
        item_shop_mp3.set_title("Download Mp3 music");
        item_shop_mp3.set_tip("Unlock mp3 music download function");
        item_shop_mp3.set_lang_data("shop_all_mp3", "shop_all_mp3_tip");
        item_shop_mp3.load_lang_data();
        item_shop_mp3.set_act(() => this.app.carrot.buy_product(7));
        this.add_btn_buy(item_shop_mp3,7);

        Carrot.Carrot_Box_Item item_shop_fahsion = box_shop.create_item("shop_fahsion");
        item_shop_fahsion.set_icon(this.sp_icon_shop_fahsion);
        item_shop_fahsion.set_title("Use all costumes and characters");
        item_shop_fahsion.set_tip("Unlock and use all character's costumes");
        item_shop_fahsion.set_lang_data("shop_fahsion", "shop_fahsion_tip");
        item_shop_fahsion.load_lang_data();
        item_shop_fahsion.set_act(() => this.app.carrot.buy_product(6));
        this.add_btn_buy(item_shop_fahsion,6);

        Carrot.Carrot_Box_Item item_shop_all_func = box_shop.create_item("shop_all_fun");
        item_shop_all_func.set_icon(this.sp_icon_shop_all_func);
        item_shop_all_func.set_title("Full use of functions");
        item_shop_all_func.set_tip("Make full use of the functions listed above");
        item_shop_all_func.set_lang_data("shop_all_func", "shop_all_func_tip");
        item_shop_all_func.load_lang_data();
        item_shop_all_func.set_act(() => this.app.carrot.buy_product(0));
        this.add_btn_buy(item_shop_all_func, 0);
    }

    private void add_btn_buy(Carrot.Carrot_Box_Item item,int index_proudct)
    {
        Carrot.Carrot_Box_Btn_Item btn_buy = item.create_item();
        if (this.check_buy_product(0))
        {
            btn_buy.set_icon(this.app.carrot.icon_carrot_done);
        }
        else
        {
            if (this.check_buy_product(index_proudct))
                btn_buy.set_icon(this.app.carrot.icon_carrot_done);
            else
                btn_buy.set_icon(this.sp_icon_buy);
        }
        btn_buy.set_color(this.app.carrot.color_highlight);
        Destroy(btn_buy.GetComponent<Button>());
    }

    public float get_voice_speed()
    {
        return this.voice_speech;
    }

    public bool get_status_bubble_icon()
    {
        return this.is_bubble_icon;
    }

    public bool get_status_sound_voice()
    {
        return this.is_sound_voice;
    }

    public string get_character_sex()
    {
        return this.character_sex;
    }

    public string get_user_sex()
    {
        return this.user_sex;
    }

    public void act_open_setting_voice_type()
    {
        this.app.carrot.play_sound_click();
        Application.OpenURL("https://support.google.com/accessibility/answer/11221616?hl="+this.app.carrot.lang.get_key_lang());
    }

    private void act_change_voice_type(int index)
    {
        PlayerPrefs.SetString("tts_type",index.ToString());
        this.s_tts_type = index.ToString();
    }

    public string get_tts_type()
    {
        return this.s_tts_type;
    }
}
