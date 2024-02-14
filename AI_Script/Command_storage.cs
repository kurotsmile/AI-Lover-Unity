using Carrot;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum Command_Type_Act
{
    add_command,
    edit_command,
    edit_pass,
    edit_live,
    edit_pending_to_pass,
    edit_command_from_log
}

[FirestoreData]
public struct chat
{
    public string id { get; set; }
    [FirestoreProperty]
    public string key { get; set; }
    [FirestoreProperty]
    public string msg { get; set; }
    [FirestoreProperty]
    public string act { get; set; }
    [FirestoreProperty]
    public string face { get; set; }
    [FirestoreProperty]
    public string limit { get; set; }
    [FirestoreProperty]
    public string mp3 { get; set; }
    [FirestoreProperty]
    public string sex_user { get; set; }
    [FirestoreProperty]
    public string sex_character { get; set; }
    [FirestoreProperty]
    public string status { get; set; }
    [FirestoreProperty]
    public string pater { get; set; }
    [FirestoreProperty]
    public string link { get; set; }
    [FirestoreProperty]
    public string color { get; set; }
    [FirestoreProperty]
    public string icon { get; set; }
    [FirestoreProperty]
    public Carrot_Rate_user_data user { get; set; }
    [FirestoreProperty]
    public string func { get; set; }
    [FirestoreProperty]
    public string date_create { get; set; }
}

[FirestoreData]
public struct Chat_Log
{
    public string id { get; set; }
    [FirestoreProperty]
    public string key { get; set; }
    [FirestoreProperty]
    public string pater { get; set; }
    [FirestoreProperty]
    public string lang { get; set; }
    [FirestoreProperty]
    public Carrot_Rate_user_data user { get; set; }
    [FirestoreProperty]
    public string date_create { get; set; }
}

public class Command_storage : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    [Header("Panel Add Command")]
    public Sprite icon_list_command;
    public string[] list_parameter_tag_name;
    public string[] list_parameter_tag_val;

    private int length;

    private Carrot_Box_Item item_command_edit_temp;
    private Item_command_chat item_command_log_temp;
    private int index_cm_update = -1;
    private bool is_cm_mode_nomal = true;

    private string s_id = "";
    private string s_status = "pending";
    private string s_pater_id = "";
    private string s_pater_msg = "";
    private string s_sex_user = "";
    private string s_sex_character = "";

    private string s_color = "#FFFFFF";
    private string s_id_icon = "";

    private string s_user_id = "";
    private string s_user_name = "";
    private string s_user_lang = "";
    private string s_user_avatar = "";

    [Header("Asset Func App")]
    public string[] func_app_name;

    [Header("Mode Test Command")]
    public GameObject obj_button_next_command_test;
    public GameObject obj_button_prev_command_test;
    public GameObject obj_button_prev_command_replay;
    private int index_command_test_play = -1;

    [Header("Asset Icon")]
    public Sprite sp_icon_add_chat;
    public Sprite sp_icon_patert;
    public Sprite sp_icon_func;
    public Sprite sp_icon_key;
    public Sprite sp_icon_msg;
    public Sprite sp_icon_action;
    public Sprite sp_icon_face;
    public Sprite sp_icon_run;
    public Sprite sp_icon_run_control;
    public Sprite sp_icon_icons;
    public Sprite sp_icon_limit;
    public Sprite sp_icon_colo_sel;
    public Sprite sp_icon_duplicated;
    public Sprite sp_icon_delete;
    public Sprite sp_icon_command_pending;
    public Sprite sp_icon_command_teach;
    public Sprite sp_icon_command_purchased;
    public Sprite sp_icon_parameter_tag;
    public Sprite sp_icon_key_same;
    public Sprite sp_icon_father;
    public Sprite sp_icon_random;
    public Sprite icon_command_pass;
    public Sprite sp_icon_translation;

    private Carrot.Carrot_Box box_add_chat;
    private Carrot.Carrot_Box_Item item_patert;
    private Carrot.Carrot_Box_Item item_msg;
    private Carrot.Carrot_Box_Item item_run_control;
    private Carrot.Carrot_Box_Item item_run_cmd;
    private Carrot.Carrot_Box_Item item_icon;
    private Carrot.Carrot_Box_Item item_keyword;
    private Carrot.Carrot_Box_Item item_action;
    private Carrot.Carrot_Box_Item item_face;
    private Carrot.Carrot_Box_Item item_user_create;
    private Carrot.Carrot_Box_Item item_user_sex;
    private Carrot.Carrot_Box_Item item_character_sex;

    private Carrot.Carrot_Box_Btn_Item btn_model_nomal;
    private Carrot.Carrot_Box_Btn_Item btn_model_advanced;
    private Carrot.Carrot_Button_Item obj_btn_test;

    private Carrot.Carrot_Box_Btn_Item btn_run_control_tags;

    private Command_Type_Act type_act = Command_Type_Act.add_command;

    private IList list_key_block;
    private IDictionary data_chat_test;

    private Carrot_Box box_parameter_tag;
    private Carrot_Box box_sex_sel_chat;

    private List<Carrot_Box_Item> list_item_key_query = null;

    private int c_length_face = 18;

    private void reset_all_s_data()
    {
        this.s_id = "";
        this.s_status = "pending";
        this.s_pater_id = "";
        this.s_pater_msg = "";
        this.s_id_icon = "";
        this.s_color = "#FFFFFF";
        this.s_user_id = "";
        this.s_user_lang = "";
        this.s_user_name = "";
        this.s_user_avatar = "";
        this.index_cm_update = -1;
        this.s_sex_character = "";
        this.s_sex_user = "";
    }

    public void check_load_command_storage()
    {
        this.length = PlayerPrefs.GetInt("cm_length", 0);
        if (PlayerPrefs.GetInt("is_cm_mode_nomal", 0) == 0) this.is_cm_mode_nomal = true; else this.is_cm_mode_nomal = false;
    }

    public void show_add_command_with_pater(string s_chat, string pater)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.add_command;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        data_new_chat["key"] = "";
        data_new_chat["pater"] = pater;
        data_new_chat["pater_msg"] = s_chat;
        this.show_edit_by_data(data_new_chat);
    }

    public void show_add_command_with_pater_and_keyword(string s_key, string s_chat, string pater)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.add_command;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        data_new_chat["key"] = s_key;
        data_new_chat["pater"] = pater;
        data_new_chat["pater_msg"] = s_chat;
        this.show_edit_by_data(data_new_chat);
    }

    public void show_add_command_new()
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.add_command;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        data_new_chat["key"] = "";
        this.show_edit_by_data(data_new_chat);
    }

    public void show_new_command(string s_new_keyword)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.add_command;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        data_new_chat["key"] = s_new_keyword;
        this.show_edit_by_data(data_new_chat);
    }

    public void show_edit_command(int index, Carrot_Box_Item item_edit)
    {
        this.reset_all_s_data();
        this.item_command_edit_temp = item_edit;
        this.type_act = Command_Type_Act.edit_command;
        this.show_edit_by_index(index);
    }

    public void show_edit_command_from_log(IDictionary data, Item_command_chat item_edit_cm_log)
    {
        this.reset_all_s_data();
        this.item_command_log_temp = item_edit_cm_log;
        this.type_act = Command_Type_Act.edit_command_from_log;
        this.show_edit_by_data(data);
    }

    private void show_edit_by_index(int index)
    {
        this.reset_all_s_data();
        this.index_cm_update = index;
        string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + index);
        IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
        this.show_edit_by_data(data_chat);
    }

    public void show_edit_dev(IDictionary data_chat, Carrot_Box_Item item_edit)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.add_command;
        this.item_command_edit_temp = item_edit;
        this.show_edit_by_data(data_chat);
    }

    public void show_edit_pass(IDictionary data_chat, Carrot_Box_Item item_edit)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.edit_pass;
        this.item_command_edit_temp = item_edit;
        this.show_edit_by_data(data_chat);
    }

    public void show_edit_pending_to_pass(int index_pending, Carrot_Box_Item item_edit)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.edit_pending_to_pass;
        this.index_cm_update = index_pending;
        this.item_command_edit_temp = item_edit;
        string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + index_pending);
        IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
        this.show_edit_by_data(data_chat);
    }

    public void show_edit_live(IDictionary data_chat)
    {
        this.reset_all_s_data();
        this.type_act = Command_Type_Act.edit_live;
        this.show_edit_by_data(data_chat);
    }

    private void show_edit_by_data(IDictionary data_chat)
    {
        if (data_chat["id"] != null) this.s_id = data_chat["id"].ToString();
        if (data_chat["status"] != null) this.s_status = data_chat["status"].ToString();

        if (data_chat["sex_character"] != null)
            this.s_sex_character = data_chat["sex_character"].ToString();
        else
            this.s_sex_character = this.app.setting.get_character_sex();

        if (data_chat["sex_user"] != null)
            this.s_sex_user = data_chat["sex_user"].ToString();
        else
            this.s_sex_user = this.app.setting.get_user_sex();

        this.get_list_key_block();
        if (this.box_add_chat != null) this.box_add_chat.close();

        this.box_add_chat = this.GetComponent<App>().carrot.Create_Box("box_command_editor");

        if (this.type_act == Command_Type_Act.add_command) box_add_chat.set_title(PlayerPrefs.GetString("brain_add", "Add the command"));
        if (this.type_act == Command_Type_Act.edit_command) box_add_chat.set_title(PlayerPrefs.GetString("brain_update", "Update command"));
        if (this.type_act == Command_Type_Act.edit_live) box_add_chat.set_title(PlayerPrefs.GetString("brain_update", "Update command"));
        if (this.type_act == Command_Type_Act.edit_pass) box_add_chat.set_title("Update Pass Command (Dev)");
        if (this.type_act == Command_Type_Act.edit_pending_to_pass) box_add_chat.set_title("Update Pending To Pass Command (Dev)");

        box_add_chat.set_icon(this.sp_icon_add_chat);

        this.btn_model_nomal = this.box_add_chat.create_btn_menu_header(this.app.carrot.icon_carrot_nomal);
        btn_model_nomal.set_act(() => this.act_box_add_model_nomal());
        if (this.is_cm_mode_nomal == true) btn_model_nomal.set_icon_color(this.GetComponent<App>().carrot.color_highlight);
        this.btn_model_advanced = this.box_add_chat.create_btn_menu_header(this.app.carrot.icon_carrot_advanced);
        btn_model_advanced.set_act(() => this.act_box_add_model_advanced());
        if (this.is_cm_mode_nomal == false) btn_model_advanced.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        if (this.type_act != Command_Type_Act.edit_live)
        {
            Carrot.Carrot_Box_Btn_Item btn_list_key_block = this.box_add_chat.create_btn_menu_header(this.app.carrot.icon_carrot_bug);
            btn_list_key_block.set_act(() => this.show_list_block_key_chat());
        }

        if (data_chat["pater"] != null)
        {
            if (data_chat["pater"].ToString() != "")
            {
                this.s_pater_id = data_chat["pater"].ToString();
                if (data_chat["pater_msg"] != null) this.s_pater_msg = data_chat["pater_msg"].ToString();
                else this.s_pater_msg = this.s_pater_id;
            }
        }

        if (this.s_pater_id != "")
        {
            this.item_patert = this.box_add_chat.create_item("item_patert");
            item_patert.set_type(Carrot.Box_Item_Type.box_value_txt);
            item_patert.set_icon(this.sp_icon_patert);
            item_patert.set_title("Add a reply to this chat");
            item_patert.set_tip("Add an answer to this conversation");
            item_patert.set_lang_data("brain_add", "cm_pater");
            item_patert.set_val(this.s_pater_msg);
            item_patert.load_lang_data();

            Carrot.Carrot_Box_Btn_Item btn_info_patert = item_patert.create_item();
            btn_info_patert.set_icon(this.app.command.icon_info_chat);
            btn_info_patert.set_color(this.app.carrot.color_highlight);
            btn_info_patert.set_act(() => this.app.command.show_info_chat_by_id(this.s_pater_id));

            Carrot.Carrot_Box_Btn_Item btn_del_patert = item_patert.create_item();
            btn_del_patert.set_icon(this.app.carrot.sp_icon_del_data);
            btn_del_patert.set_color(this.app.carrot.color_highlight);
            btn_del_patert.set_act(() => act_del_patert_chat());
        }

        if (data_chat["key"] != null)
        {
            this.item_keyword = box_add_chat.create_item("item_keyword");
            item_keyword.set_type(Carrot.Box_Item_Type.box_value_input);
            item_keyword.check_type();
            item_keyword.set_icon(this.sp_icon_key);
            item_keyword.set_title("key word");
            item_keyword.set_tip("keywords to execute the command");
            item_keyword.set_lang_data("cm_keyword", "cm_keyword_tip");
            item_keyword.load_lang_data();
            this.item_keyword.set_val(data_chat["key"].ToString());

            Carrot_Box_Btn_Item btn_key_mic = this.item_keyword.create_item();
            btn_key_mic.set_icon(this.app.command_voice.icon_mic_chat);
            btn_key_mic.set_color(this.app.carrot.color_highlight);
            btn_key_mic.set_act(() => this.app.command_voice.start_inp_mic(this.item_keyword.inp_val));

            if (this.app.carrot.model_app == ModelApp.Develope)
            {
                Carrot.Carrot_Box_Btn_Item btn_key_same = this.item_keyword.create_item();
                btn_key_same.set_color(this.app.carrot.color_highlight);
                btn_key_same.set_icon(this.app.command_dev.sp_icon_key_same);
                btn_key_same.set_act(() => this.app.command_dev.show_chat_key_same(this.item_keyword.get_val()));

                Carrot.Carrot_Box_Btn_Item btn_translate = this.item_keyword.create_item();
                btn_translate.set_color(this.app.carrot.color_highlight);
                btn_translate.set_icon(this.app.command_dev.sp_icon_translate);
                btn_translate.set_act(() => this.act_translate(this.item_keyword.get_val()));
            }
        }

        this.item_msg = box_add_chat.create_item("item_msg");
        item_msg.set_type(Carrot.Box_Item_Type.box_value_input);
        item_msg.check_type();
        item_msg.set_icon(this.sp_icon_msg);
        item_msg.set_title("Responsive text content");
        item_msg.set_tip("Character response text when replying to the keyword");
        item_msg.set_lang_data("cm_msg", "cm_msg_tip");
        item_msg.load_lang_data();
        if (data_chat["msg"] != null) this.item_msg.set_val(data_chat["msg"].ToString());

        Carrot_Box_Btn_Item btn_msg_mic = this.item_msg.create_item();
        btn_msg_mic.set_icon(this.app.command_voice.icon_mic_chat);
        btn_msg_mic.set_color(this.app.carrot.color_highlight);
        btn_msg_mic.set_act(() => this.app.command_voice.start_inp_mic(this.item_msg.inp_val));

        if (this.app.carrot.model_app == ModelApp.Develope)
        {
            Carrot.Carrot_Box_Btn_Item btn_translate = this.item_msg.create_item();
            btn_translate.set_color(this.app.carrot.color_highlight);
            btn_translate.set_icon(this.app.command_dev.sp_icon_translate);
            btn_translate.set_act(() => this.act_translate(this.item_msg.get_val()));
        }

        Carrot.Carrot_Box_Btn_Item btn_parameter_tag = this.item_msg.create_item();
        btn_parameter_tag.set_icon(this.sp_icon_parameter_tag);
        btn_parameter_tag.set_color(this.GetComponent<App>().carrot.color_highlight);
        btn_parameter_tag.set_act(() => this.show_list_parameter_tag());

        this.item_action = box_add_chat.create_item("item_action");
        item_action.set_icon(this.sp_icon_action);
        item_action.set_title("Act");
        item_action.set_tip("The character's actions follow the response text");
        item_action.set_lang_data("act", "act_tip");
        item_action.load_lang_data();

        if (data_chat["act"] != null)
        {
            string s_act = data_chat["act"].ToString();
            if (s_act != "")
            {
                this.item_action.set_val(s_act);
                item_action.set_type(Carrot.Box_Item_Type.box_value_txt);
                item_action.check_type();
            }
        }
        item_action.set_act(() => this.app.action.btn_show_category(item_action));

        Carrot.Carrot_Box_Btn_Item btn_act_list = this.item_action.create_item();
        btn_act_list.set_icon(this.app.carrot.icon_carrot_all_category);
        btn_act_list.set_color(this.app.carrot.color_highlight);
        btn_act_list.set_act(() => this.app.action.btn_show_category(item_action));

        Carrot.Carrot_Box_Btn_Item btn_act_random = this.item_action.create_item();
        btn_act_random.set_icon(this.sp_icon_random);
        btn_act_random.set_color(this.app.carrot.color_highlight);
        btn_act_random.set_act(() => this.change_act_random());

        this.item_face = box_add_chat.create_item("item_face");
        item_face.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        item_face.check_type();
        item_face.set_icon(this.sp_icon_face);
        item_face.set_title("Emotional face");
        item_face.set_tip("Show the character's facial expression when saying this command");
        item_face.set_lang_data("face", "face_tip");
        item_face.load_lang_data();
        item_face.dropdown_val.ClearOptions();
        for (int i = 0; i <= this.c_length_face; i++)
        {
            string s_name_face = PlayerPrefs.GetString("face", "Face") + " " + (i + 1);
            item_face.dropdown_val.options.Add(new Dropdown.OptionData(s_name_face));
        }
        if (data_chat["face"] != null) this.item_face.set_val(data_chat["face"].ToString());

        Carrot.Carrot_Box_Btn_Item btn_face_random = this.item_face.create_item();
        btn_face_random.set_icon(this.sp_icon_random);
        btn_face_random.set_color(this.app.carrot.color_highlight);
        btn_face_random.set_act(() => this.change_face_random());

        this.item_run_cmd = box_add_chat.create_item("item_run_cmd");
        item_run_cmd.set_type(Carrot.Box_Item_Type.box_value_input);
        item_run_cmd.check_type();
        item_run_cmd.set_icon(this.sp_icon_run);
        item_run_cmd.set_title("Open websites and apps");
        item_run_cmd.set_tip("Please enter the web address or the URL Schema Name of the app, to open a website or app.");
        item_run_cmd.set_lang_data("cm_func_web", "cm_func_web_tip");
        item_run_cmd.load_lang_data();
        if (data_chat["link"] != null) this.item_run_cmd.set_val(data_chat["link"].ToString());
        if (this.is_cm_mode_nomal)
            this.item_run_cmd.gameObject.SetActive(false);
        else
            this.item_run_cmd.gameObject.SetActive(true);

        this.item_run_control = box_add_chat.create_item("item_run_control");
        item_run_control.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        item_run_control.check_type();
        item_run_control.set_icon(this.sp_icon_run_control);
        item_run_control.set_title("Program control");
        item_run_control.set_tip("Select a function you want to control");
        item_run_control.set_lang_data("cm_func_app", "cm_func_app_tip");
        item_run_control.load_lang_data();
        if (this.is_cm_mode_nomal)
            this.item_run_control.gameObject.SetActive(false);
        else
            this.item_run_control.gameObject.SetActive(true);
        this.item_run_control.dropdown_val.ClearOptions();
        item_run_control.dropdown_val.ClearOptions();
        foreach (string option in this.func_app_name) item_run_control.dropdown_val.options.Add(new Dropdown.OptionData(option));
        if (data_chat["func"] != null)
        {
            if (data_chat["func"].ToString() != "")
            {
                int index_control_app = int.Parse(data_chat["func"].ToString());
                this.item_run_control.set_val(index_control_app.ToString());
            }
        }
        item_run_control.dropdown_val.onValueChanged.AddListener(this.act_check_run_control);

        this.item_icon = box_add_chat.create_item("item_icon");
        item_icon.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_icon.check_type();
        item_icon.set_icon(this.sp_icon_icons);
        item_icon.set_title("Icons and colors");
        item_icon.set_tip("Choose icons and colors in the system's icon store to increase the liveliness of the dialogue");
        item_icon.set_lang_data("setting_bubble_icon", "setting_bubble_icon_tip");
        item_icon.load_lang_data();
        item_icon.set_act(() => this.app.icon.btn_show_list_emoji_and_color(item_icon));
        item_icon.set_val("#" + this.s_color);
        if (this.is_cm_mode_nomal)
            this.item_icon.gameObject.SetActive(false);
        else
            this.item_icon.gameObject.SetActive(true);

        Carrot.Carrot_Box_Btn_Item btn_color = this.item_icon.create_item();
        btn_color.set_color(this.GetComponent<App>().carrot.color_highlight);
        btn_color.set_icon(this.sp_icon_colo_sel);
        Destroy(btn_color.GetComponent<UnityEngine.UI.Button>());
        if (data_chat["color"] != null)
        {
            this.s_color = data_chat["color"].ToString();
            Color color_item;
            ColorUtility.TryParseHtmlString(this.s_color, out color_item);
            this.item_icon.txt_val.color = color_item;
            this.item_icon.set_val(this.s_color);
        }

        if (data_chat["icon"] != null)
        {
            this.s_id_icon = data_chat["icon"].ToString();
            this.item_icon.set_val(this.s_id_icon);

            Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs(this.s_id_icon);
            if (sp_icon != null) this.item_icon.set_icon_white(sp_icon);
        }

        if (this.app.icon.count_icon_name() > 0)
        {
            Carrot_Box_Btn_Item btn_icon_random = this.item_icon.create_item();
            btn_icon_random.set_icon(this.sp_icon_random);
            btn_icon_random.set_color(this.app.carrot.color_highlight);
            btn_icon_random.set_act(() => this.change_icon_random());
        }

        this.item_user_sex = box_add_chat.create_item("item_user_sex");
        item_user_sex.set_icon(this.app.setting.sp_icon_sex_user);
        item_user_sex.set_title(PlayerPrefs.GetString("setting_your_sex", "Your gender"));
        item_user_sex.set_act(() => this.show_select_sex_chat(this.item_user_sex));
        if (this.is_cm_mode_nomal)
            this.item_user_sex.gameObject.SetActive(false);
        else
            this.item_user_sex.gameObject.SetActive(true);
        if (this.s_sex_user == "0")
            item_user_sex.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
        else
            item_user_sex.set_tip(PlayerPrefs.GetString("user_sex_girl", "Girl"));

        Carrot_Box_Btn_Item btn_user_sex_edit = item_user_sex.create_item();
        btn_user_sex_edit.set_icon(this.app.carrot.user.icon_user_edit);
        btn_user_sex_edit.set_color(this.app.carrot.color_highlight);
        Destroy(btn_user_sex_edit.GetComponent<Button>());

        this.item_character_sex = box_add_chat.create_item("item_character_sex");
        item_character_sex.set_icon(this.app.setting.sp_icon_sex_character);
        item_character_sex.set_title(PlayerPrefs.GetString("setting_char_sex", "Character gender"));
        item_character_sex.set_act(() => this.show_select_sex_chat(this.item_character_sex));
        if (this.is_cm_mode_nomal)
            this.item_character_sex.gameObject.SetActive(false);
        else
            this.item_character_sex.gameObject.SetActive(true);
        if (this.s_sex_character == "0")
            item_character_sex.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
        else
            item_character_sex.set_tip(PlayerPrefs.GetString("user_sex_girl", "Girl"));

        Carrot_Box_Btn_Item btn_user_character_edit = item_character_sex.create_item();
        btn_user_character_edit.set_icon(this.app.carrot.user.icon_user_edit);
        btn_user_character_edit.set_color(this.app.carrot.color_highlight);
        Destroy(btn_user_character_edit.GetComponent<Button>());

        if (data_chat["user"] != null)
        {
            if (data_chat["user"].ToString() != "")
            {
                IDictionary data_user_create = (IDictionary)data_chat["user"];
                if (data_user_create["id"] != null)
                {
                    string user_id = data_user_create["id"].ToString();
                    string user_lang = data_user_create["lang"].ToString();
                    this.item_user_create = this.box_add_chat.create_item("item_user");
                    this.item_user_create.set_title(PlayerPrefs.GetString("chat_creator", "Creator"));
                    this.item_user_create.set_icon(this.app.carrot.user.icon_user_info);
                    this.item_user_create.set_tip(data_user_create["name"].ToString());

                    if (data_user_create["avatar"] != null)
                    {
                        Sprite sp_avatar = this.app.carrot.get_tool().get_sprite_to_playerPrefs("avatar_user_" + data_user_create["id"]);
                        if (sp_avatar != null) this.item_user_create.set_icon_white(sp_avatar);
                        else this.app.carrot.get_img_and_save_playerPrefs(data_user_create["avatar"].ToString(), this.item_user_create.img_icon, "avatar_user_" + data_user_create["id"]);
                    }

                    this.item_user_create.set_act(() => this.app.carrot.user.show_user_by_id(user_id, user_lang));
                    if (this.is_cm_mode_nomal)
                        this.item_user_create.gameObject.SetActive(false);
                    else
                        this.item_user_create.gameObject.SetActive(true);

                    this.s_user_id = data_user_create["id"].ToString();
                    this.s_user_name = data_user_create["name"].ToString();
                    this.s_user_lang = data_user_create["lang"].ToString();
                    if (data_user_create["avatar"] != null) this.s_user_avatar = data_user_create["avatar"].ToString();

                    Carrot_Box_Btn_Item btn_del_user = this.item_user_create.create_item();
                    btn_del_user.set_icon(this.app.carrot.sp_icon_del_data);
                    btn_del_user.set_color(Color.red);
                    btn_del_user.set_icon_color(Color.white);
                    btn_del_user.set_act(() => this.btn_del_user_create());
                }
            }
        }

        Carrot.Carrot_Box_Btn_Panel obj_panel_btn = box_add_chat.create_panel_btn();

        Carrot.Carrot_Button_Item obj_btn_done = obj_panel_btn.create_btn("btn_done");
        obj_btn_done.set_act_click(act_done_submit_command);
        obj_btn_done.set_bk_color(this.app.carrot.color_highlight);
        obj_btn_done.set_label_color(Color.white);
        obj_btn_done.set_label(PlayerPrefs.GetString("done", "Done"));
        obj_btn_done.set_icon(this.sp_icon_add_chat);

        obj_btn_test = obj_panel_btn.create_btn("btn_test");
        obj_btn_test.set_act_click(btn_test_command);
        obj_btn_test.set_bk_color(this.app.carrot.color_highlight);

        obj_btn_test.set_label_color(Color.white);
        obj_btn_test.set_label(PlayerPrefs.GetString("cm_test", "Test"));
        obj_btn_test.set_icon(this.app.carrot.game.icon_play_music_game);
    }

    private void change_act_random()
    {
        this.app.carrot.play_sound_click();
        IList list_all_name_animations = this.app.action.get_list_all_name_animations();
        if (list_all_name_animations == null)
        {
            int index_act = UnityEngine.Random.Range(0, this.app.action.list_anim_act_defalt.Length);
            this.item_action.set_type(Box_Item_Type.box_value_txt);
            this.item_action.check_type();
            this.item_action.set_val(this.app.action.list_anim_act_defalt[index_act]);
        }
        else
        {
            int index_act = UnityEngine.Random.Range(0, list_all_name_animations.Count);
            this.item_action.set_type(Box_Item_Type.box_value_txt);
            this.item_action.check_type();
            this.item_action.set_val(list_all_name_animations[index_act].ToString());
        }
    }

    private void change_face_random()
    {
        this.app.carrot.play_sound_click();
        int index_act = UnityEngine.Random.Range(0, this.c_length_face);
        this.item_face.set_val(index_act.ToString());
    }

    private void change_icon_random()
    {
        this.app.carrot.play_sound_click();
        IList icons = this.app.icon.get_list_icon_name();
        if (icons != null)
        {
            Color color_icon = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            int index_icons = UnityEngine.Random.Range(0, icons.Count);
            this.s_id_icon = icons[index_icons].ToString();
            this.item_icon.set_val(this.s_id_icon);
            Sprite sp_icon_chat = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_icon);
            if (sp_icon_chat != null)
            {
                this.item_icon.set_icon_white(sp_icon_chat);
            }
            else
            {
                this.item_icon.set_icon(this.app.setting.sp_icon_chat_bubble);
                this.item_icon.img_icon.color = Color.black;
            }
            this.item_icon.txt_val.color = color_icon;
            this.s_color = "#" + ColorUtility.ToHtmlStringRGBA(color_icon);
        }
    }

    private void show_select_sex_chat(Carrot_Box_Item item_sex_sel)
    {
        string s_val_sex = "";

        if (item_sex_sel.name == "item_character_sex") s_val_sex = this.s_sex_character;
        if (item_sex_sel.name == "item_user_sex") s_val_sex = this.s_sex_user;

        this.box_sex_sel_chat = this.app.carrot.Create_Box("sel_sex");
        this.box_sex_sel_chat.set_title(item_sex_sel.txt_name.text);
        this.box_sex_sel_chat.set_icon(item_sex_sel.img_icon.sprite);

        Carrot.Carrot_Box_Item item_sex_boy = this.box_sex_sel_chat.create_item("item_sex_boy");
        item_sex_boy.set_icon(this.app.setting.sp_icon_sex_boy);
        item_sex_boy.set_title(PlayerPrefs.GetString("user_sex_boy", "Boy"));
        item_sex_boy.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
        item_sex_boy.set_act(() => this.act_sel_sex_chat(item_sex_sel, "0"));

        if (s_val_sex == "0")
        {
            Carrot_Box_Btn_Item btn_sel = item_sex_boy.create_item();
            btn_sel.set_icon(this.app.carrot.icon_carrot_done);
            btn_sel.set_icon_color(this.app.carrot.color_highlight);
            btn_sel.set_color(Color.white);
        }

        Carrot.Carrot_Box_Item item_sex_girl = this.box_sex_sel_chat.create_item("item_sex_girl");
        item_sex_girl.set_icon(this.app.setting.sp_icon_sex_girl);
        item_sex_girl.set_title(PlayerPrefs.GetString("user_sex_girl", "Girl"));
        item_sex_girl.set_tip(PlayerPrefs.GetString("user_sex_girl", "Female"));
        item_sex_girl.set_act(() => this.act_sel_sex_chat(item_sex_sel, "1"));

        if (s_val_sex == "1")
        {
            Carrot_Box_Btn_Item btn_sel = item_sex_girl.create_item();
            btn_sel.set_icon(this.app.carrot.icon_carrot_done);
            btn_sel.set_icon_color(this.app.carrot.color_highlight);
            btn_sel.set_color(Color.white);
        }
    }

    private void act_sel_sex_chat(Carrot_Box_Item item_sex_sel, string s_val_sex)
    {
        this.app.carrot.play_sound_click();
        if (item_sex_sel.name == "item_character_sex") this.s_sex_character = s_val_sex;
        if (item_sex_sel.name == "item_user_sex") this.s_sex_user = s_val_sex;

        if (s_val_sex == "0")
            item_sex_sel.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
        else
            item_sex_sel.set_tip(PlayerPrefs.GetString("user_sex_girl", "Girl"));

        if (this.box_sex_sel_chat != null) this.box_sex_sel_chat.close();
    }

    private void btn_del_user_create()
    {
        this.app.carrot.play_sound_click();
        this.s_user_avatar = "";
        this.s_user_id = "";
        this.s_user_name = "";
        this.s_user_lang = "";
        if (this.item_user_create != null) this.item_user_create.gameObject.SetActive(false);
    }

    public void add_command_offline(IDictionary data_chat)
    {
        if (this.get_cm_by_id(data_chat["id"].ToString()) == null)
        {
            string s = Carrot.Json.Serialize(data_chat);
            this.add_command_offline(s);
        }
    }

    public void add_command_offline(string s_data)
    {
        PlayerPrefs.SetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.length, s_data);
        this.length++;
        PlayerPrefs.SetInt("cm_length", this.length);
    }

    public IDictionary act_call_cm_offline(string cm_cmd, string id_pather = "")
    {
        cm_cmd = cm_cmd.ToLower();
        
        List<IDictionary> list_chat = new List<IDictionary>();
        for (int i = 0; i < this.length; i++)
        {
            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if (s_data != "")
            {
                IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
                data_chat["index_cm"] = i;
                if (id_pather == "")
                {
                    if (cm_cmd.Equals(data_chat["key"].ToString().ToLower())) list_chat.Add(data_chat);
                }
                else
                {
                    if (cm_cmd.Equals(data_chat["key"].ToString().ToLower()) && data_chat["pater"].ToString() == id_pather) list_chat.Add(data_chat);
                }
            }
        }

        Debug.Log("act_call_cm_offline found " + list_chat.Count+" pater:"+id_pather+" length:"+this.length+" lang:"+this.app.carrot.lang.get_key_lang()+" user:"+this.app.setting.get_user_sex()+" char:"+this.app.setting.get_character_sex());

        if (list_chat.Count > 0)
        {
            if (list_chat.Count == 1)
            {
                return list_chat[0];
            }
            else
            {
                int rand_index = UnityEngine.Random.Range(0, list_chat.Count);
                return list_chat[rand_index];
            }
        }
        return null;
    }

    public IDictionary get_cm_by_id(string id_chat)
    {
        for (int i = 0; i < this.length; i++)
        {
            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if (s_data != "")
            {
                IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
                if (data_chat["id"].ToString() == id_chat) return data_chat;
            }
        }
        return null;
    }

    [ContextMenu("Delete all command")]
    public void delete_all_cm()
    {
        for (int i = 0; i < this.length; i++) this.act_delete_cm(i);
        this.length = 0;
        PlayerPrefs.SetInt("cm_length", 0);
        string s_title = PlayerPrefs.GetString("brain_list", "List command");
        this.app.carrot.show_msg(s_title, "Delete all command success!!!");
        this.check_load_command_storage();
        this.app.command_dev.close_all_box();
        this.app.command.clear_log_chat();
        this.app.command.obj_btn_clear_all_log.SetActive(false);
    }

    public void show_list_cm(string s_type)
    {
        this.app.command_dev.set_type(Command_Dev_Type.storage);
        Carrot_Box box;
        if (s_type == "0")
        {
            box = this.app.command_dev.box_list(this.get_list_all_cm());
            if (box == null) return;
            box.set_icon(this.icon_list_command);
            box.set_title(PlayerPrefs.GetString("brain_list", "List command"));
        }
        else
        {
            box = this.app.command_dev.box_list(this.get_list_buy_cm());
            if (box == null) return;
            box.set_icon(this.sp_icon_command_purchased);
            box.set_title(PlayerPrefs.GetString("brain_list_buy", "Purchase command"));
        }

        Carrot.Carrot_Box_Btn_Item btn_command_teach = box.create_btn_menu_header(this.sp_icon_command_teach);
        btn_command_teach.set_act(() => this.show_list_cm("0"));
        if (s_type != "0") btn_command_teach.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        Carrot.Carrot_Box_Btn_Item btn_command_purchased = box.create_btn_menu_header(this.sp_icon_command_purchased);
        btn_command_purchased.set_act(() => this.show_list_cm("1"));
        if (s_type != "1") btn_command_purchased.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        Carrot.Carrot_Box_Btn_Item btn_del_all = box.create_btn_menu_header(this.GetComponent<App>().carrot.sp_icon_del_data);
        btn_del_all.set_act(() => this.delete_all_cm());
        btn_del_all.set_icon_color(Color.red);
    }

    private List<IDictionary> get_list_all_cm()
    {
        List<IDictionary> list_cm = new List<IDictionary>();
        for (int i = this.length; i >= 0; i--)
        {
            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if (s_data == "") continue;

            IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
            data_chat["index_cm"] = i;
            list_cm.Add(data_chat);
        }
        return list_cm;
    }

    private List<IDictionary> get_list_buy_cm()
    {
        List<IDictionary> list_cm = new List<IDictionary>();
        for (int i = this.length; i >= 0; i--)
        {
            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if (s_data == "") continue;

            IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
            if (data_chat["status"] != null)
            {
                if (data_chat["status"].ToString() == "buy")
                {
                    data_chat["index_cm"] = i;
                    list_cm.Add(data_chat);
                }
            }
        }
        return list_cm;
    }

    public void delete_cm(int index, GameObject obj_item)
    {
        this.act_delete_cm(index);
        if (obj_item != null)
        {
            Destroy(obj_item.gameObject);
            this.app.command_dev.close_box_last();
        }
    }

    private void act_delete_cm(int index)
    {
        PlayerPrefs.DeleteKey("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + index);
    }

    public void download_command_offline()
    {
        if (this.app.setting.check_buy_product(4))
            download_command_shop();
        else
            this.app.buy_product(4);
    }

    public void download_command_shop()
    {
        Query DownloadChatQuery = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).WhereEqualTo("sex_user", this.app.setting.get_user_sex()).WhereEqualTo("sex_character", this.app.setting.get_character_sex());
        DownloadChatQuery.Limit(50).GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot capitalQuerySnapshot = task.Result;
            if (task.IsCompleted)
            {
                if (capitalQuerySnapshot.Count > 0)
                {
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary c = documentSnapshot.ToDictionary();
                        c["id"] = documentSnapshot.Id;
                        c["status"] = "buy";
                        this.app.command_storage.add_command_offline(c);
                    };
                    this.app.carrot.show_msg(PlayerPrefs.GetString("brain_download", "Download commands"), PlayerPrefs.GetString("shop_buy_success", "Purchase successful! the function you purchased has been activated. Please restart the application to use it"), Carrot.Msg_Icon.Success);
                }
            }
        });
    }

    public void hide_box_add()
    {
        if (this.box_add_chat != null) this.box_add_chat.gameObject.SetActive(false);
    }

    #region Mode Test Command
    public void btn_test_command()
    {
        if (this.item_msg.get_val() == "")
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("error_null_key_and_msg", "Your keywords and content can't be blank!"));
            return;
        }

        IDictionary data_test = this.get_data_chat_editor();
        data_test["status"] = "test";
        this.app.command_dev.set_all_box_active(false);
        this.box_add_chat.gameObject.SetActive(false);
        this.act_test_command(data_test);
    }

    public void play_one_test_command(IDictionary data_chat)
    {
        data_chat["status"] = "test";
        this.app.command_dev.set_all_box_active(false);
        this.act_test_command(data_chat);
    }

    public void btn_close_test_command()
    {
        if (this.box_add_chat != null) this.box_add_chat.gameObject.SetActive(true);
        this.app.action.show_box();
        this.app.command_dev.set_all_box_active(true);
        this.app.panel_inp_command_test.SetActive(false);
        this.app.panel_inp_func.SetActive(false);
        this.app.panel_inp_msg.SetActive(true);
        this.index_command_test_play = -1;
        this.app.show_character_on_test(this.app.setting.get_character_sex());
    }

    public void btn_replay_test_command()
    {
        this.act_test_command(this.data_chat_test);
    }

    public void act_test_command(IDictionary data_chat)
    {
        this.data_chat_test = data_chat;
        this.obj_button_prev_command_replay.SetActive(true);
        if (data_chat["status"].ToString() == "test_list")
        {
            if (data_chat["index_list"] != null) this.index_command_test_play = int.Parse(data_chat["index_list"].ToString());
            this.obj_button_next_command_test.SetActive(true);
            this.obj_button_prev_command_test.SetActive(true);
        }
        else
        {
            this.obj_button_next_command_test.SetActive(false);
            this.obj_button_prev_command_test.SetActive(false);
        }

        this.app.panel_main.SetActive(true);
        this.app.panel_inp_command_test.SetActive(true);
        this.app.panel_inp_func.SetActive(false);
        this.app.panel_inp_msg.SetActive(false);
        this.app.panel_chat_func.SetActive(false);
        this.app.panel_chat_msg.SetActive(true);
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait()) this.app.panel_menu_right.SetActive(false);
        this.app.command.act_chat(data_chat, false);
    }

    public void btn_play_next_command_test()
    {
        this.index_command_test_play++;
        if (this.index_command_test_play >= this.app.command_dev.length_list_test()) this.index_command_test_play = 0;
        this.app.command_dev.act_play_test_list(this.index_command_test_play);
    }

    public void btn_play_prev_command_test()
    {
        this.index_command_test_play--;
        if (this.index_command_test_play < 0) this.index_command_test_play = (this.app.command_dev.length_list_test() - 1);
        this.app.command_dev.act_play_test_list(this.index_command_test_play);
    }
    #endregion

    private void act_box_add_model_nomal()
    {
        this.is_cm_mode_nomal = true;
        this.btn_model_advanced.set_icon_color(Color.black);
        this.btn_model_nomal.set_icon_color(this.app.carrot.color_highlight);
        PlayerPrefs.SetInt("is_cm_mode_nomal", 0);
        this.item_icon.gameObject.SetActive(false);
        this.item_run_cmd.gameObject.SetActive(false);
        this.item_run_control.gameObject.SetActive(false);
        this.item_character_sex.gameObject.SetActive(false);
        this.item_user_sex.gameObject.SetActive(false);
        if (this.item_user_create != null) this.item_user_create.gameObject.SetActive(false);
    }

    private void act_box_add_model_advanced()
    {
        this.is_cm_mode_nomal = false;
        this.btn_model_advanced.set_icon_color(this.app.carrot.color_highlight);
        this.btn_model_nomal.set_icon_color(Color.black);
        PlayerPrefs.SetInt("is_cm_mode_nomal", 1);
        this.item_icon.gameObject.SetActive(true);
        this.item_run_cmd.gameObject.SetActive(true);
        this.item_run_control.gameObject.SetActive(true);
        this.item_character_sex.gameObject.SetActive(true);
        this.item_user_sex.gameObject.SetActive(true);
        if (this.item_user_create != null) this.item_user_create.gameObject.SetActive(true);
    }

    private void get_list_key_block()
    {
        if (this.list_key_block == null)
        {
            DocumentReference keyblockRef = this.app.carrot.db.Collection("block").Document(this.app.carrot.lang.get_key_lang());
            keyblockRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot collectionSnapshot = task.Result;
                    IDictionary key_block = collectionSnapshot.ToDictionary();
                    this.list_key_block = (IList)key_block["chat"];
                }
            });
        }
    }

    private string check_keyblock(string s_key_check)
    {
        foreach (string s_key in this.list_key_block)
        {
            if (s_key_check.ToLower().Equals(s_key.ToLower()) || s_key_check.ToLower().Contains(s_key.ToLower()))
            {
                return s_key;
            }
        }
        return "";
    }

    private chat get_data_chat()
    {
        chat c = new chat
        {
            msg = this.item_msg.get_val().Trim(),
            act = this.item_action.get_val(),
            face = this.item_face.get_val(),
            limit = "1",
            sex_character = this.s_sex_character,
            sex_user = this.s_sex_user,
            pater = this.s_pater_id,
            link = this.item_run_cmd.get_val(),
            color = this.s_color,
            icon = this.s_id_icon,
            status = this.s_status,
            func = this.item_run_control.get_val(),
            date_create = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        if (this.s_id.Trim() == "") c.id = "chat" + this.app.carrot.generateID();
        else c.id = this.s_id;

        if (this.item_keyword != null) c.key = this.item_keyword.get_val().Trim().ToLower();
        c.user = this.get_user_data_editor(this.s_user_id);
        return c;
    }

    private Carrot_Rate_user_data get_user_data_editor(string s_user_id)
    {
        Carrot_Rate_user_data user_login = new Carrot_Rate_user_data();
        if (s_user_id != "")
        {
            user_login.name = this.s_user_name;
            user_login.id = s_user_id;
            user_login.lang = this.s_user_lang;
            user_login.avatar = this.s_user_avatar;
        }
        else
        {
            if (this.app.carrot.user.get_id_user_login() != "")
            {
                user_login.name = this.app.carrot.user.get_data_user_login("name");
                user_login.id = this.app.carrot.user.get_id_user_login();
                user_login.lang = this.app.carrot.user.get_lang_user_login();
                user_login.avatar = this.app.carrot.user.get_data_user_login("avatar");
            }
        }
        return user_login;
    }

    private IDictionary get_data_chat_editor()
    {
        IDictionary data_chat = (IDictionary)Json.Deserialize("{}");
        if (this.s_id != "")
            data_chat["id"] = this.s_id;
        else
            data_chat["id"] = "chat" + this.app.carrot.generateID();

        if (this.item_keyword != null) data_chat["key"] = this.item_keyword.get_val();
        data_chat["msg"] = this.item_msg.get_val();
        data_chat["act"] = this.item_action.get_val();
        data_chat["face"] = this.item_face.get_val();
        data_chat["func"] = this.item_run_control.get_val();
        data_chat["link"] = this.item_run_cmd.get_val();
        data_chat["icon"] = this.s_id_icon;
        data_chat["color"] = this.s_color;
        data_chat["pater"] = this.s_pater_id;
        data_chat["pater_msg"] = this.s_pater_msg;
        data_chat["sex_character"] = this.s_sex_character;
        data_chat["sex_user"] = this.s_sex_user;

        IDictionary data_user = (IDictionary)Json.Deserialize(JsonConvert.SerializeObject(this.get_user_data_editor(this.s_user_id)));
        data_chat["user"] = data_user;
        if (this.s_status != "")
            data_chat["status"] = this.s_status;
        else
            data_chat["status"] = "pending";
        return data_chat;
    }

    private void act_done_submit_command()
    {
        this.app.carrot.show_loading();

        if (this.type_act == Command_Type_Act.add_command)
        {
            chat c = this.get_data_chat();
            string s_error_key_block = this.check_keyblock(this.item_keyword.get_val());

            if (this.item_keyword.get_val().Trim().Length == 0 || this.item_msg.get_val().Trim().Length == 0)
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("error_null_key_and_msg", "Your keywords and content can't be blank!"));
                return;
            }

            if (s_error_key_block != "")
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("keyblock_keyword_error", "Added keyword is forbidden, please add more content (" + s_error_key_block + ")"));
                return;
            }

            s_error_key_block = this.check_keyblock(this.item_msg.get_val());

            if (s_error_key_block != "")
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("keyblock_msg_error", "Keyword add in response forbidden content, please add more content (" + s_error_key_block + ")"));
                return;
            }

            this.app.carrot.hide_loading();
            if (this.app.carrot.is_online())
            {
                string s_id_chat_new = "chat" + this.app.carrot.generateID();
                CollectionReference chatDbRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang());
                if (this.app.carrot.model_app == ModelApp.Publish)
                {
                    DocumentReference chatRef = chatDbRef.Document(s_id_chat_new);
                    c.id = s_id_chat_new;
                    IDictionary chat_data = (IDictionary)Carrot.Json.Deserialize(JsonConvert.SerializeObject(c));
                    chatRef.SetAsync(c).ContinueWithOnMainThread(task => {
                        if (task.IsFaulted) this.add_command_offline(chat_data);
                        if (task.IsCompleted) this.add_command_offline(chat_data);
                    });
                }

                if (this.app.carrot.model_app == ModelApp.Develope)
                {
                    DocumentReference chatRef = chatDbRef.Document(c.id);
                    c.status = "passed";
                    chatRef.SetAsync(c);
                }
            }
            else
            {
                string s_data_chat_new = JsonConvert.SerializeObject(c);
                this.add_command_offline(s_data_chat_new);
            }

            if (this.app.carrot.model_app == ModelApp.Publish)
            {
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("brain_add_success", "Your chat has been published successfully!"));
            }
            else
            {
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), "The chat has been published successfully! (Dev)");
                if (this.item_command_edit_temp != null) Destroy(this.item_command_edit_temp.gameObject);
            }
        }

        if (this.type_act == Command_Type_Act.edit_pass)
        {
            this.app.carrot.hide_loading();
            CollectionReference chatDbRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang());
            chat c = this.get_data_chat();
            DocumentReference chatRef = chatDbRef.Document(c.id);
            c.status = "passed";
            chatRef.SetAsync(c);
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), "Chat update published successfully! (Dev)");
            if (this.item_command_edit_temp != null) Destroy(this.item_command_edit_temp.gameObject);
        }

        if (this.type_act == Command_Type_Act.edit_pending_to_pass)
        {
            this.app.carrot.hide_loading();
            CollectionReference chatDbRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang());
            chat c = this.get_data_chat();
            DocumentReference chatRef = chatDbRef.Document(c.id);
            c.status = "passed";
            chatRef.SetAsync(c);
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), "Convert draft dialogue into successfully published conversation! (Dev)");
            if (this.item_command_edit_temp != null) Destroy(this.item_command_edit_temp.gameObject);
            this.act_delete_cm(this.index_cm_update);
        }

        if (this.type_act == Command_Type_Act.edit_command)
        {
            chat c = this.get_data_chat();
            string s_chat_data = JsonConvert.SerializeObject(c);
            PlayerPrefs.SetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.index_cm_update, s_chat_data);
            if (this.item_command_edit_temp != null)
            {
                this.item_command_edit_temp.set_title(c.key);
                this.item_command_edit_temp.set_tip(c.msg);
            }
            this.app.carrot.hide_loading();
        }

        if (this.type_act == Command_Type_Act.edit_live)
        {
            this.app.carrot.hide_loading();
            IDictionary data_live = this.get_data_chat_editor();
            this.app.live.update_data_item_chat_live(data_live);
        }

        if (this.box_add_chat != null) this.box_add_chat.close();
        if (this.app.carrot.model_app == ModelApp.Publish) this.app.carrot.ads.show_ads_Interstitial();
    }

    private void act_del_patert_chat()
    {
        this.s_pater_id = "";
        this.s_pater_msg = "";
        Destroy(this.item_patert.gameObject);
    }

    public void btn_add_key_tag(string s_tag)
    {
        string s_val = this.item_msg.get_val() + " " + s_tag + " ";
        this.item_msg.set_val(s_val);
        if (this.box_parameter_tag != null) this.box_parameter_tag.close();
    }

    private void show_list_parameter_tag()
    {
        this.box_parameter_tag = this.app.carrot.Create_Box("list_tag");
        this.box_parameter_tag.set_title("Parameter Tag");
        this.box_parameter_tag.set_icon(this.sp_icon_parameter_tag);

        for (int i = 0; i < this.list_parameter_tag_name.Length; i++)
        {
            var s_tag = this.list_parameter_tag_name[i];
            Carrot.Carrot_Box_Item item_tag = this.box_parameter_tag.create_item("item_tag_" + i);
            item_tag.set_icon(this.sp_icon_parameter_tag);
            item_tag.set_title(this.list_parameter_tag_name[i]);
            item_tag.set_tip(this.list_parameter_tag_val[i]);
            item_tag.set_act(() => btn_add_key_tag(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_add_tag = item_tag.create_item();
            btn_add_tag.set_icon(this.sp_icon_add_chat);
            btn_add_tag.set_color(this.GetComponent<App>().carrot.color_highlight);
            Destroy(btn_add_tag.GetComponent<UnityEngine.UI.Button>());
        }
    }

    public void show_list_block_key_chat()
    {
        if (this.list_key_block == null)
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("list_none", "List is empty, no items found!")); return;
        }

        Carrot.Carrot_Box box_list_key = this.app.carrot.Create_Box("key_block");
        box_list_key.set_title(PlayerPrefs.GetString("list_banned_keywords", "List of prohibited keywords"));
        box_list_key.set_icon(this.app.carrot.icon_carrot_bug);

        for (int i = 0; i < this.list_key_block.Count; i++)
        {
            Carrot.Carrot_Box_Item item_key = box_list_key.create_item("key_block_" + i);
            item_key.set_icon(this.sp_icon_key);
            item_key.set_title(this.list_key_block[i].ToString());
            item_key.set_tip(this.list_key_block[i].ToString());
        }
    }

    private void act_translate(string s_txt)
    {
        s_txt = UnityWebRequest.EscapeURL(s_txt);
        string s_tr = "https://translate.google.com/?sl=" + this.app.carrot.lang.get_key_lang() + "&tl=vi&text=" + s_txt + "&op=translate";
        Application.OpenURL(s_tr);
    }

    public void set_s_color(string s_color)
    {
        this.s_color = s_color;
    }

    public void set_s_id_icon(string s_id_icon)
    {
        this.s_id_icon = s_id_icon;
    }

    private void act_check_run_control(int index_sel)
    {
        if (this.btn_run_control_tags != null) Destroy(this.btn_run_control_tags.gameObject);

        if (index_sel == 16)
        {
            this.btn_run_control_tags = this.item_run_cmd.create_item();
            btn_run_control_tags.set_icon(this.sp_icon_parameter_tag);
            btn_run_control_tags.set_color(this.app.carrot.color_highlight);
            btn_run_control_tags.set_act(() => this.show_list_sys_act_tag());
        }

        if (index_sel == 19)
        {
            this.btn_run_control_tags = this.item_run_cmd.create_item();
            btn_run_control_tags.set_icon(this.sp_icon_parameter_tag);
            btn_run_control_tags.set_color(this.app.carrot.color_highlight);
            btn_run_control_tags.set_act(() => this.show_list_package_tag());
        }
    }

    private void show_list_sys_act_tag()
    {
        this.box_parameter_tag = this.app.carrot.Create_Box("list_sys_act_tag");
        this.box_parameter_tag.set_title("sys_action_by_link");
        this.box_parameter_tag.set_icon(this.sp_icon_parameter_tag);

        for (int i = 0; i < this.app.tool.list_name_action.Length; i++)
        {
            var s_tag = this.app.tool.list_name_action[i];
            Carrot.Carrot_Box_Item item_tag = this.box_parameter_tag.create_item("item_tag_" + i);
            item_tag.set_icon(this.sp_icon_parameter_tag);
            item_tag.set_title(this.app.tool.list_name_action[i]);
            item_tag.set_tip(this.app.tool.list_name_action[i]);
            item_tag.set_act(() => btn_add_sys_act_tag(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_test_tag = item_tag.create_item();
            btn_test_tag.set_icon(this.sp_icon_run_control);
            btn_test_tag.set_color(this.app.carrot.color_highlight);
            btn_test_tag.set_act(() => this.app.tool.open_content_Intent(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_add_tag = item_tag.create_item();
            btn_add_tag.set_icon(this.sp_icon_add_chat);
            btn_add_tag.set_color(this.app.carrot.color_highlight);
            Destroy(btn_add_tag.GetComponent<UnityEngine.UI.Button>());
        }
    }

    private void show_list_package_tag()
    {
        this.box_parameter_tag = this.app.carrot.Create_Box("list_sys_act_tag");
        this.box_parameter_tag.set_title("sys_action_by_link");
        this.box_parameter_tag.set_icon(this.sp_icon_parameter_tag);

        for (int i = 0; i < this.app.tool.list_package_action.Length; i++)
        {
            var s_tag = this.app.tool.list_package_action[i];
            Carrot.Carrot_Box_Item item_tag = this.box_parameter_tag.create_item("item_package_" + i);
            item_tag.set_icon(this.sp_icon_parameter_tag);
            item_tag.set_title(this.app.tool.list_package_action[i]);
            item_tag.set_tip(this.app.tool.list_package_action[i]);
            item_tag.set_act(() => btn_add_sys_act_tag(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_test_tag = item_tag.create_item();
            btn_test_tag.set_icon(this.sp_icon_run_control);
            btn_test_tag.set_color(this.app.carrot.color_highlight);
            btn_test_tag.set_act(() => this.app.tool.OpenApp_by_bundleId(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_add_tag = item_tag.create_item();
            btn_add_tag.set_icon(this.sp_icon_add_chat);
            btn_add_tag.set_color(this.app.carrot.color_highlight);
            Destroy(btn_add_tag.GetComponent<UnityEngine.UI.Button>());
        }
    }

    public void btn_add_sys_act_tag(string s_tag)
    {
        this.item_run_cmd.set_val(s_tag);
        if (this.box_parameter_tag != null) this.box_parameter_tag.close();
    }

    public void add_log(string s_key)
    {
        Chat_Log log = new Chat_Log
        {
            key = s_key,
            pater = this.app.command.get_id_chat_cur(),
            date_create = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            lang = this.app.carrot.lang.get_key_lang()
        };
        log.key = s_key;

        if (this.app.carrot.user.get_id_user_login() != "")
        {
            Carrot_Rate_user_data user_login = new Carrot_Rate_user_data();
            user_login.name = this.app.carrot.user.get_data_user_login("name");
            user_login.id = this.app.carrot.user.get_id_user_login();
            user_login.lang = this.app.carrot.user.get_lang_user_login();
            user_login.avatar = this.app.carrot.user.get_data_user_login("avatar");
            log.user = user_login;
        }

        string s_id_log = "log" + this.app.carrot.generateID();
        CollectionReference chatDbRef = this.app.carrot.db.Collection("chat-log");
        DocumentReference chatRef = chatDbRef.Document(s_id_log);
        log.id = s_id_log;
        chatRef.SetAsync(log);
    }

    #region Query Key Setting
    public void show_setting_query_key()
    {
        this.box_add_chat = this.app.carrot.Create_Box("query_key_setting");
        this.box_add_chat.set_title(PlayerPrefs.GetString("cm_query_setting", "Customize command query keywords"));
        this.box_add_chat.set_icon(this.app.carrot.sp_icon_dev);
        this.list_item_key_query = new List<Carrot_Box_Item>();
        string s_data_querys_song = this.app.player_music.playlist.get_data_key_query_music();
        if (s_data_querys_song == "")
        {
            Carrot_Box_Item item_key_query = this.box_add_chat.create_item("item_key_0");
            item_key_query.set_title(PlayerPrefs.GetString("music", "Music"));
            item_key_query.set_icon(this.app.command_storage.sp_icon_key);
            item_key_query.set_tip("Keyword used to open the song by name");
            item_key_query.set_type(Box_Item_Type.box_value_input);
            item_key_query.check_type();
            this.list_item_key_query.Add(item_key_query);
        }
        else
        {
            string[] list_key = s_data_querys_song.Split(";");
            for (int i = 0; i < list_key.Length; i++)
            {
                var index_item = i;
                string s_name_query = list_key[i].ToString();
                Carrot_Box_Item item_key_query = this.box_add_chat.create_item("item_key_" + i);
                item_key_query.set_icon(this.app.command_storage.sp_icon_key);
                item_key_query.set_title(PlayerPrefs.GetString("music", "Music") + " (" + (i + 1) + ")");
                item_key_query.set_tip(s_name_query + " {song_name} -> Keyword used to open the song by name");
                item_key_query.set_type(Box_Item_Type.box_value_input);
                item_key_query.check_type();
                item_key_query.set_val(s_name_query);
                this.list_item_key_query.Add(item_key_query);

                Carrot_Box_Btn_Item btn_del = item_key_query.create_item();
                btn_del.set_icon(this.app.carrot.sp_icon_del_data);
                btn_del.set_color(this.app.carrot.color_highlight);
                btn_del.set_act(() => this.delete_query(index_item));
            }
        }

        Carrot_Box_Btn_Item btn_add_key = this.box_add_chat.create_btn_menu_header(this.sp_icon_add_chat);
        btn_add_key.set_act(() => this.add_key_query());

        Carrot_Box_Btn_Panel panel_btn = this.box_add_chat.create_panel_btn();
        Carrot_Button_Item btn_done = panel_btn.create_btn("btn_done");
        btn_done.set_icon_white(this.app.carrot.icon_carrot_done);
        btn_done.set_bk_color(this.app.carrot.color_highlight);
        btn_done.set_label(PlayerPrefs.GetString("done", "Done"));
        btn_done.set_label_color(Color.white);
        btn_done.set_act_click(() => this.done_box_setting_query());

        Carrot_Button_Item btn_close = panel_btn.create_btn("btn_close");
        btn_close.set_icon_white(this.app.carrot.icon_carrot_done);
        btn_close.set_bk_color(this.app.carrot.color_highlight);
        btn_close.set_label(PlayerPrefs.GetString("cancel", "Cancel"));
        btn_close.set_label_color(Color.white);
        btn_close.set_act_click(() => this.close_box_setting_query());
    }

    private void delete_query(int index)
    {
        Destroy(this.list_item_key_query[index]);
        Destroy(this.list_item_key_query[index].gameObject);
        this.list_item_key_query[index] = null;
    }

    private void add_key_query()
    {
        this.app.carrot.play_sound_click();
        Carrot_Box_Item item_key_query = this.box_add_chat.create_item("item_key_" + this.list_item_key_query.Count);
        item_key_query.set_icon(this.app.command_storage.sp_icon_key);
        item_key_query.set_title(PlayerPrefs.GetString("music", "Music") + " (" + (this.list_item_key_query.Count + 1) + ")");
        item_key_query.set_tip("Keyword used to open the song by name");
        item_key_query.set_type(Box_Item_Type.box_value_input);
        item_key_query.check_type();
        item_key_query.set_val("");
        item_key_query.transform.SetSiblingIndex(0);
        this.list_item_key_query.Add(item_key_query);
    }

    private void done_box_setting_query()
    {
        string s_query_data = "";
        for (int i = 0; i < this.list_item_key_query.Count; i++)
        {
            if (this.list_item_key_query[i] != null)
            {
                s_query_data = s_query_data + this.list_item_key_query[i].get_val() + ";";
                Destroy(this.list_item_key_query[i]);
            }
        }
        s_query_data = s_query_data + "end";
        s_query_data = s_query_data.Replace(";end", "");
        if (s_query_data == "end") s_query_data = "";
        this.app.player_music.playlist.set_data_key_query_music(s_query_data);
        this.app.carrot.play_sound_click();
        if (this.box_add_chat != null) this.box_add_chat.close();
    }

    private void close_box_setting_query()
    {
        this.app.carrot.play_sound_click();
        if (this.box_add_chat != null) this.box_add_chat.close();
    }
    #endregion
}