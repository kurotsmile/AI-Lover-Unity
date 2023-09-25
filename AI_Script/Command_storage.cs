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

public enum Command_Type_Act {
    add_command,
    edit_command,
    edit_pass,
    list_command,
    list_pass,
    list_pedding
}

[FirestoreData]
public struct chat
{
    public string id { get; set; }
    [FirestoreProperty]
    public string key {get; set;}
    [FirestoreProperty]
    public string msg {get; set;}
    [FirestoreProperty]
    public string action {get; set;}
    [FirestoreProperty]
    public string face {get; set;}
    [FirestoreProperty]
    public string limit { get; set; }
    [FirestoreProperty]
    public string mp3 {get; set;}
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

public class Command_storage : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    [Header("Panel Add Command")]
    public Sprite icon_list_command;
    public string[] list_parameter_tag_name;
    public string[] list_parameter_tag_val;

    private int length;

    private int index_cm_update = -1;
    private bool is_cm_mode_nomal = true;

    private string s_id = "";
    private string s_pater_id = "";
    private string s_pater_msg = "";

    private string s_color = "FFFFFF";
    private string s_id_icon = "";
    private string s_type_view_list = "0";
    [Header("Asset Func App")]
    public string[] func_app_name;

    [Header("Mode Test Command")]
    public GameObject obj_button_next_command_test;
    public GameObject obj_button_prev_command_test;
    private int index_command_test_play = 0;
    private bool is_list_command_test_play = false;

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
    public Sprite icon_command_pass;
    public Sprite sp_icon_translation;

    private Carrot.Carrot_Box box_list;
    private Carrot.Carrot_Box box_list_icon;
    private Carrot.Carrot_Box box_list_icon_category;
    private Carrot.Carrot_Box box_add_chat;
    private Carrot.Carrot_Box_Item item_patert;
    private Carrot.Carrot_Box_Item item_msg;
    private Carrot.Carrot_Box_Item item_run_control;
    private Carrot.Carrot_Box_Item item_run_cmd;
    private Carrot.Carrot_Box_Item item_icon;
    private Carrot.Carrot_Box_Item item_keyword;
    private Carrot.Carrot_Box_Item item_action;
    private Carrot.Carrot_Box_Item item_face;
    private Carrot.Carrot_Box_Item item_limit;

    private Carrot.Carrot_Box_Btn_Item btn_model_nomal;
    private Carrot.Carrot_Box_Btn_Item btn_model_advanced;
    private Carrot.Carrot_Button_Item obj_btn_test;

    private Command_Type_Act type_act = Command_Type_Act.list_command;

    private IList list_key_block;
    private IDictionary data_chat_test;
    private QuerySnapshot IconQuerySnapshot;
    private QuerySnapshot IconCategoryQuerySnapshot=null;

    public void check_load_command_storage()
    {
        this.length = PlayerPrefs.GetInt("cm_length", 0);
        if (PlayerPrefs.GetInt("is_cm_mode_nomal", 0) == 0) this.is_cm_mode_nomal = true; else this.is_cm_mode_nomal = false;
    }

    public void show_add_command_with_pater(string s_chat, string pater)
    {
        this.type_act = Command_Type_Act.add_command;
        this.s_pater_msg = s_chat;
        this.s_pater_id = pater;
        this.index_cm_update = -1;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        this.show_edit_by_data(data_new_chat);
    }

    public void show_add_command_new()
    {
        this.type_act = Command_Type_Act.add_command;
        this.s_pater_msg = "";
        this.s_pater_id = "";
        this.index_cm_update = -1;
        IDictionary data_new_chat = (IDictionary) Carrot.Json.Deserialize("{}");
        this.show_edit_by_data(data_new_chat);
    }

    public void show_new_command(string s_new_keyword)
    {
        this.type_act = Command_Type_Act.add_command;
        IDictionary data_new_chat = (IDictionary)Carrot.Json.Deserialize("{}");
        this.show_edit_by_data(data_new_chat);
        this.item_keyword.set_val(s_new_keyword);
    }

    public void show_edit_command(int index)
    {
        this.type_act = Command_Type_Act.edit_command;
        this.show_edit(index);
    }
     
    public void show_edit(int index)
    {
        this.index_cm_update = index;
        string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + index);
        IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
        this.show_edit_by_data(data_chat);
    }

    public void show_edit_dev(IDictionary data_chat)
    {
        this.type_act= Command_Type_Act.add_command;
        this.show_edit_by_data(data_chat);
    }

    private void show_edit_by_data(IDictionary data_chat)
    {
        this.s_id_icon = "";
        this.s_color = "#FFFFFF";

        if (data_chat["id"] != null) this.s_id = data_chat["id"].ToString();

        this.get_list_key_block();
        if (this.box_add_chat != null) this.box_add_chat.close();

        this.box_add_chat = this.GetComponent<App>().carrot.Create_Box("command_add");

        if (this.type_act == Command_Type_Act.add_command) box_add_chat.set_title(PlayerPrefs.GetString("brain_add", "Add the command"));
        if (this.type_act == Command_Type_Act.edit_command) box_add_chat.set_title(PlayerPrefs.GetString("brain_update", "Update command"));
        if (this.type_act == Command_Type_Act.edit_pass) box_add_chat.set_title("Update Pass Command");

        box_add_chat.set_icon(this.sp_icon_add_chat);

        this.btn_model_nomal = this.box_add_chat.create_btn_menu_header(this.app.carrot.icon_carrot_nomal);
        btn_model_nomal.set_act(() => this.act_box_add_model_nomal());
        if (this.is_cm_mode_nomal == true) btn_model_nomal.set_icon_color(this.GetComponent<App>().carrot.color_highlight);
        this.btn_model_advanced = this.box_add_chat.create_btn_menu_header(this.app.carrot.icon_carrot_advanced);
        btn_model_advanced.set_act(() => this.act_box_add_model_advanced());
        if (this.is_cm_mode_nomal == false) btn_model_advanced.set_icon_color(this.GetComponent<App>().carrot.color_highlight);
        Carrot.Carrot_Box_Btn_Item btn_list_key_block = this.box_add_chat.create_btn_menu_header(this.app.setting.sp_icon_chat_limit);
        btn_list_key_block.set_act(() => this.show_list_block_key_chat());

        this.item_patert = box_add_chat.create_item("item_patert");
        item_patert.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_patert.set_icon(this.sp_icon_patert);
        item_patert.set_title("Add a reply to this chat");
        item_patert.set_tip("Add an answer to this conversation");
        item_patert.set_lang_data("brain_add", "cm_pater");
        item_patert.set_val(this.s_pater_msg);
        item_patert.load_lang_data();

        Carrot.Carrot_Box_Btn_Item btn_del_patert = item_patert.create_item();
        btn_del_patert.set_icon(this.GetComponent<App>().carrot.sp_icon_del_data);
        btn_del_patert.set_color(this.GetComponent<App>().carrot.color_highlight);
        btn_del_patert.set_act(() => act_del_patert_chat());
        if (this.s_pater_id == "") this.item_patert.gameObject.SetActive(false);

        this.item_keyword = box_add_chat.create_item("item_keyword");
        item_keyword.set_type(Carrot.Box_Item_Type.box_value_input);
        item_keyword.check_type();
        item_keyword.set_icon(this.sp_icon_key);
        item_keyword.set_title("key word");
        item_keyword.set_tip("keywords to execute the command");
        item_keyword.set_lang_data("cm_keyword", "cm_keyword_tip");
        item_keyword.load_lang_data();

        if (this.app.carrot.model_app == ModelApp.Develope)
        {
            Carrot.Carrot_Box_Btn_Item btn_translate = this.item_keyword.create_item();
            btn_translate.set_color(this.app.carrot.color_highlight);
            btn_translate.set_icon(this.app.command_dev.sp_icon_translate);
            btn_translate.set_act(() =>this.act_translate(this.item_keyword.get_val()));
        }

        this.item_msg = box_add_chat.create_item("item_msg");
        item_msg.set_type(Carrot.Box_Item_Type.box_value_input);
        item_msg.check_type();
        item_msg.set_icon(this.sp_icon_msg);
        item_msg.set_title("Responsive text content");
        item_msg.set_tip("Character response text when replying to the keyword");
        item_msg.set_lang_data("cm_msg", "cm_msg_tip");
        item_msg.load_lang_data();

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
        item_action.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        item_action.check_type();
        item_action.set_icon(this.sp_icon_action);
        item_action.set_title("Act");
        item_action.set_tip("The character's actions follow the response text");
        item_action.set_lang_data("act", "act_tip");
        item_action.load_lang_data();
        item_action.dropdown_val.ClearOptions();
        for (int i = 0; i <= 41; i++)
        {
            string s_name_action = PlayerPrefs.GetString("act", "Action") + " " + (i + 1);
            item_action.dropdown_val.options.Add(new Dropdown.OptionData(s_name_action));
        }

        this.item_face = box_add_chat.create_item("item_face");
        item_face.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        item_face.check_type();
        item_face.set_icon(this.sp_icon_face);
        item_face.set_title("Emotional face");
        item_face.set_tip("Show the character's facial expression when saying this command");
        item_face.set_lang_data("face", "face_tip");
        item_face.load_lang_data();
        item_face.dropdown_val.ClearOptions();
        for (int i = 0; i <= 18; i++)
        {
            string s_name_face = PlayerPrefs.GetString("face", "Face") + " " + (i + 1);
            item_face.dropdown_val.options.Add(new Dropdown.OptionData(s_name_face));
        }

        this.item_run_cmd = box_add_chat.create_item("item_run_cmd");
        item_run_cmd.set_type(Carrot.Box_Item_Type.box_value_input);
        item_run_cmd.check_type();
        item_run_cmd.set_icon(this.sp_icon_run);
        item_run_cmd.set_title("Open websites and apps");
        item_run_cmd.set_tip("Please enter the web address or the URL Schema Name of the app, to open a website or app.");
        item_run_cmd.set_lang_data("cm_func_web", "cm_func_web_tip");
        item_run_cmd.load_lang_data();
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

        this.item_limit = box_add_chat.create_item("item_limit");
        item_limit.set_type(Carrot.Box_Item_Type.box_value_slider);
        item_limit.set_icon(this.sp_icon_limit);
        item_limit.set_title("Limit vulgarity and sex");
        item_limit.set_tip("Set limits for children");
        item_limit.set_lang_data("report_limit_chat", "chat_limit_tip");
        item_limit.load_lang_data();
        item_limit.set_fill_color(this.GetComponent<App>().carrot.color_highlight);
        item_limit.check_type();
        this.item_limit.slider_val.wholeNumbers = true;
        this.item_limit.slider_val.minValue = 1;
        this.item_limit.slider_val.maxValue = 4;
        item_limit.set_val(this.GetComponent<App>().setting.get_limit_chat().ToString());
        if (this.is_cm_mode_nomal)
            this.item_limit.gameObject.SetActive(false);
        else
            this.item_limit.gameObject.SetActive(true);

        this.item_icon = box_add_chat.create_item("item_icon");
        item_icon.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_icon.check_type();
        item_icon.set_icon(this.sp_icon_icons);
        item_icon.set_title("Icons and colors");
        item_icon.set_tip("Choose icons and colors in the system's icon store to increase the liveliness of the dialogue");
        item_icon.set_lang_data("setting_bubble_icon", "setting_bubble_icon_tip");
        item_icon.load_lang_data();
        item_icon.set_act(() => this.btn_show_list_emoji_and_color());
        item_icon.set_val("#" + this.s_color);
        if (this.is_cm_mode_nomal)
            this.item_icon.gameObject.SetActive(false);
        else
            this.item_icon.gameObject.SetActive(true);

        Carrot.Carrot_Box_Btn_Item btn_color = this.item_icon.create_item();
        btn_color.set_color(this.GetComponent<App>().carrot.color_highlight);
        btn_color.set_icon(this.sp_icon_colo_sel);
        Destroy(btn_color.GetComponent<UnityEngine.UI.Button>());

        if (this.app.carrot.model_app == ModelApp.Develope)
        {
            if (data_chat["sex_user"] != null)
            {
                Carrot_Box_Item item_user_sex = box_add_chat.create_item("item_user_sex");
                item_user_sex.set_icon(this.app.setting.sp_icon_sex_user);
                item_user_sex.set_title(PlayerPrefs.GetString("setting_your_sex", "Your gender"));

                if (data_chat["sex_user"].ToString() == "0")
                    item_user_sex.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
                else
                    item_user_sex.set_tip(PlayerPrefs.GetString("user_sex_girl", "Girl"));
            }

            if (data_chat["sex_character"] != null)
            {
                Carrot_Box_Item item_npc_sex = box_add_chat.create_item("item_npc_sex");
                item_npc_sex.set_icon(this.app.setting.sp_icon_sex_character);
                item_npc_sex.set_title(PlayerPrefs.GetString("setting_char_sex", "Character gender"));

                if (data_chat["sex_character"].ToString() == "0")
                    item_npc_sex.set_tip(PlayerPrefs.GetString("user_sex_boy", "Boy"));
                else
                    item_npc_sex.set_tip(PlayerPrefs.GetString("user_sex_girl", "Girl"));
            }
        }

        Carrot.Carrot_Box_Btn_Panel obj_panel_btn = box_add_chat.create_panel_btn();

        Carrot.Carrot_Button_Item obj_btn_done = obj_panel_btn.create_btn("btn_done");
        obj_btn_done.set_act_click(act_done_submit_command);
        obj_btn_done.set_bk_color(this.GetComponent<App>().carrot.color_highlight);
        obj_btn_done.set_label_color(Color.white);
        obj_btn_done.set_label(PlayerPrefs.GetString("done", "Done"));
        obj_btn_done.set_icon(this.sp_icon_add_chat);

        obj_btn_test = obj_panel_btn.create_btn("btn_test");
        obj_btn_test.set_act_click(btn_test_command);
        obj_btn_test.set_bk_color(this.GetComponent<App>().carrot.color_highlight);

        obj_btn_test.set_label_color(Color.white);
        obj_btn_test.set_label(PlayerPrefs.GetString("cm_test", "Test"));
        obj_btn_test.set_icon(this.GetComponent<App>().carrot.game.icon_play_music_game);

        box_add_chat.set_act_before_closing(act_close_box);

        if (data_chat["id"] != null) this.s_id = data_chat["id"].ToString();
        if (data_chat["link"] != null) this.item_run_cmd.set_val(data_chat["link"].ToString());

        if (data_chat["func"] != null)
        {
            if (data_chat["func"].ToString() != "")
            {
                int index_control_app = int.Parse(data_chat["func"].ToString());
                this.item_run_control.set_val(index_control_app.ToString());
            }
        }

        if (data_chat["key"] != null) this.item_keyword.set_val(data_chat["key"].ToString());
        if (data_chat["msg"] != null) this.item_msg.set_val(data_chat["msg"].ToString());
        if (data_chat["limit"] != null) this.item_limit.set_val(data_chat["limit"].ToString());
        if (data_chat["face"] != null) this.item_face.set_val(data_chat["face"].ToString());
        if (data_chat["action"] != null) this.item_action.set_val(data_chat["action"].ToString());

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
    }

    public void add_command_offline(IDictionary data_chat)
    {
        if (!this.check_existence_cm_offline(data_chat["id"].ToString()))
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
            if (s_data!= "")
            {
                IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
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

        if (list_chat.Count > 0)
        {
            if (list_chat.Count==1)
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

    public bool check_existence_cm_offline(string id_chat)
    {
        for (int i = 0; i < this.length; i++)
        {
            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if(s_data!="")
            {
                IDictionary data_chat = (IDictionary)Carrot.Json.Deserialize(s_data);
                if (data_chat["id"].ToString() == id_chat) return true;
            }
        }
        return false;
    }

    [ContextMenu("Delete all command")]
    public void delete_all_cm()
    {
        for(int i = 0; i < this.length; i++)  this.act_delete_cm(i);
        this.length = 0;
        PlayerPrefs.SetInt("cm_length",0);
        string s_title=PlayerPrefs.GetString("brain_list", "List command");
        this.app.carrot.show_msg(s_title, "Delete all command success!!!");
        this.check_load_command_storage();
        if (this.box_list != null) this.box_list.close();
    }

    public void show_list_cm(string s_type)
    {
        this.s_type_view_list = s_type;

        int count_command = 0;

        if (this.box_list != null) this.box_list.close();
        if (s_type == "0")
            this.box_list = this.GetComponent<App>().carrot.Create_Box(PlayerPrefs.GetString("brain_list", "List command"), this.icon_list_command);
        else
            this.box_list = this.GetComponent<App>().carrot.Create_Box(PlayerPrefs.GetString("brain_list_buy", "Purchase command"), this.icon_list_command);

        Carrot.Carrot_Box_Btn_Item btn_command_teach = this.box_list.create_btn_menu_header(this.sp_icon_command_teach);
        btn_command_teach.set_act(() => this.show_list_cm("0"));
        btn_command_teach.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        Carrot.Carrot_Box_Btn_Item btn_command_purchased = this.box_list.create_btn_menu_header(this.sp_icon_command_purchased);
        btn_command_purchased.set_act(() => this.show_list_cm("1"));
        btn_command_purchased.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        Carrot.Carrot_Box_Btn_Item btn_del_all = this.box_list.create_btn_menu_header(this.GetComponent<App>().carrot.sp_icon_del_data);
        btn_del_all.set_act(() => this.delete_all_cm());
        btn_del_all.set_icon_color(this.GetComponent<App>().carrot.color_highlight);

        for (int i = this.length; i >= 0; i--)
        {
            var index_cm = i;

            string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + i);
            if (s_data == "") continue;

            IDictionary data_chat =(IDictionary) Carrot.Json.Deserialize(s_data);
            Carrot.Carrot_Box_Item item_cm = this.box_list.create_item("cm_item_" + i);
            item_cm.set_title(data_chat["key"].ToString());

            item_cm.set_tip(data_chat["msg"].ToString());
            item_cm.set_icon(this.sp_icon_msg);
            item_cm.set_title(data_chat["key"].ToString());
            item_cm.set_act(() => this.show_edit_command(index_cm));

            if (data_chat["status"] != null)
            {
                string s_status = data_chat["status"].ToString();
                if (s_status == "passed") item_cm.img_icon.color = this.app.carrot.color_highlight;
                else item_cm.img_icon.color = Color.black;
            }

            Carrot.Carrot_Box_Btn_Item btn_add = item_cm.create_item();
            btn_add.set_color(this.GetComponent<App>().carrot.color_highlight);
            btn_add.set_icon(this.sp_icon_patert);
            btn_add.set_act(() => this.show_add_command_with_pater(data_chat["msg"].ToString(), data_chat["id"].ToString()));

            Carrot.Carrot_Box_Btn_Item btn_test = item_cm.create_item();
            btn_test.set_color(this.GetComponent<App>().carrot.color_highlight);
            btn_test.set_icon(this.GetComponent<App>().carrot.game.icon_play_music_game);
            btn_test.set_act(() => this.play_test_command(index_cm));

            if (data_chat["pater"] != null)
            {
                if (data_chat["pater"].ToString() != "")
                {
                    string s_id_chat_father = data_chat["pater"].ToString();
                    Carrot_Box_Btn_Item btn_father = item_cm.create_item();
                    btn_father.set_color(this.app.carrot.color_highlight);
                    btn_father.set_icon(this.sp_icon_father);
                    btn_father.set_act(() => this.app.command.show_info_chat_by_id(s_id_chat_father));
                }
            }

            Carrot.Carrot_Box_Btn_Item btn_del = item_cm.create_item();
            btn_del.set_color(this.GetComponent<App>().carrot.color_highlight);
            btn_del.set_icon(this.sp_icon_delete);
            btn_del.set_act(() => this.delete_cm(index_cm));

            count_command++;
        }

        if (count_command==0)
        {
            if (this.box_list != null) this.box_list.close();
            this.GetComponent<App>().carrot.show_msg(PlayerPrefs.GetString("brain_list", "List command"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
        }
    }

    public void delete_cm(int index)
    {
        this.act_delete_cm(index);
        this.check_load_command_storage();
        this.show_list_cm(this.s_type_view_list);
    }

    private void act_delete_cm(int index)
    {
        PlayerPrefs.DeleteKey("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + index);
    }

    public void download_command_offline()
    {
        if (this.GetComponent<App>().setting.check_buy_product(4))
            download_command_shop();
        else
            this.GetComponent<App>().buy_product(4);
    }

    public void download_command_shop()
    {
        Query DownloadChatQuery = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).WhereEqualTo("sex_user", this.app.setting.get_user_sex()).WhereEqualTo("sex_character", this.app.setting.get_character_sex());
        DownloadChatQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
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
                        this.app.command_storage.add_command_offline(c);
                    };
                    this.app.carrot.show_msg(PlayerPrefs.GetString("brain_download", "Download commands"), PlayerPrefs.GetString("shop_buy_success", "Purchase successful! the function you purchased has been activated. Please restart the application to use it"), Carrot.Msg_Icon.Success);
                }
            }
        });
    }

    public void btn_show_list_emoji_and_color()
    {
        this.app.carrot.show_loading();
        if (IconQuerySnapshot == null)
        {
            Query IconQuery = this.app.carrot.db.Collection("icon");
            IconQuery = IconQuery.Limit(50);
            IconQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                this.IconQuerySnapshot = task.Result; 
                this.act_load_icon_and_emoji(this.IconQuerySnapshot);
            });
        }
        else
        {
            this.act_load_icon_and_emoji(this.IconQuerySnapshot);
        }
    }

    private void act_load_icon_and_emoji(QuerySnapshot query_icon)
    {
        this.app.carrot.hide_loading();
        if (this.box_list_icon != null) this.box_list_icon.close();
        this.box_list_icon = this.app.carrot.Create_Box();
        this.box_list_icon.set_icon(this.sp_icon_icons);
        this.box_list_icon.set_title(PlayerPrefs.GetString("setting_bubble_icon", "Icons and colors"));
        Carrot_Box_Btn_Item btn_icon_category=this.box_list_icon.create_btn_menu_header(this.app.carrot.icon_carrot_all_category);
        btn_icon_category.set_act(()=> list_category_icon());

        foreach (DocumentSnapshot document in query_icon)
        {
            string id_icon = document.Id;
            string s_color = "#FFFFFF";
            IDictionary icon_data = document.ToDictionary();
            Carrot_Box_Item item_icon = this.box_list_icon.create_item();
            item_icon.set_title(document.Id);
            item_icon.set_tip(icon_data["icon"].ToString());

            if (icon_data["color"] != null)
            {
                s_color = icon_data["color"].ToString();
                Color color_item;
                ColorUtility.TryParseHtmlString(icon_data["color"].ToString(), out color_item);
                item_icon.set_tip(s_color);
                item_icon.txt_tip.color = color_item;
            }

            Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_icon);
            if (sp_icon != null)
                item_icon.set_icon_white(sp_icon);
            else
                if (icon_data["icon"] != null) this.app.carrot.get_img_and_save_playerPrefs(icon_data["icon"].ToString(), item_icon.img_icon, id_icon);

            item_icon.set_act(() => this.set_icon_and_emoji(Color.red, item_icon.img_icon.sprite, s_color, id_icon));
        }
    } 

    private void list_category_icon()
    {
        this.app.carrot.show_loading();
        if (this.IconCategoryQuerySnapshot == null)
        {
            Query iconQuery = this.app.carrot.db.Collection("icon_category");
            iconQuery.Limit(20);
            iconQuery.GetSnapshotAsync().ContinueWithOnMainThread(Task => {
                if (Task.IsCompleted)
                {
                    this.IconCategoryQuerySnapshot = Task.Result;
                    this.load_data_category_icon_by_query(IconCategoryQuerySnapshot);
                }
            });
        }
        else
        {
            this.load_data_category_icon_by_query(this.IconCategoryQuerySnapshot);
        }
    }

    private void load_data_category_icon_by_query(QuerySnapshot IconCategoryQuerySnapshot)
    {
        this.app.carrot.hide_loading();
        if (this.box_list_icon_category != null) this.box_list_icon_category.close();
        this.box_list_icon_category = this.app.carrot.Create_Box();
        this.box_list_icon_category.set_icon(this.app.carrot.icon_carrot_all_category);
        this.box_list_icon_category.set_title("Bundle of object styles");

        foreach (DocumentSnapshot documentSnapshot in IconCategoryQuerySnapshot.Documents)
        {
            IDictionary icon_data = documentSnapshot.ToDictionary();
            Carrot_Box_Item item_cat = this.box_list_icon_category.create_item("item_icon");
            item_cat.set_icon(this.sp_icon_icons);
            if (icon_data["key"] != null)
            {
                string s_key_cat = icon_data["key"].ToString();
                item_cat.set_title(s_key_cat);
                item_cat.set_tip(s_key_cat);
                item_cat.set_act(() => this.view_list_icon_by_category_key(s_key_cat));
            }
        };

        this.box_list_icon_category.update_color_table_row();
    }

    private void view_list_icon_by_category_key(string s_key)
    {
        Query IconRef = this.app.carrot.db.Collection("icon");
        IconRef=IconRef.WhereEqualTo("category", s_key);
        IconRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                this.IconQuerySnapshot = task.Result;
                this.act_load_icon_and_emoji(this.IconQuerySnapshot);
            }
        });
    }

    public void set_icon_and_emoji(Color32 color_cm,Sprite sp_cm,string s_color,string s_id)
    {
        this.s_color = s_color;
        this.s_id_icon = s_id;
        this.item_icon.set_val(s_id);
        this.item_icon.set_icon_white(sp_cm);
        this.item_icon.txt_val.color = color_cm;
        if (this.box_list_icon != null) this.box_list_icon.close();
        if (this.box_list_icon_category != null) this.box_list_icon_category.close();
    }


    #region Mode Test Command
    public void btn_test_command()
    {
        chat chat_test = this.get_data_chat();

        if (chat_test.msg.ToString().Trim()=="")
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("error_null_key_and_msg", "Your keywords and content can't be blank!"));
            return;
        }

        if (this.box_list != null) this.box_list.close();
        if (this.app.command_dev.get_box_list() != null) this.app.command_dev.get_box_list().close();
        if (this.app.command_dev.get_box_list_same() != null) this.app.command_dev.get_box_list_same().close();

        this.box_add_chat.gameObject.SetActive(false);
        this.is_list_command_test_play = false;
        this.obj_button_next_command_test.SetActive(false);
        this.obj_button_prev_command_test.SetActive(false);

        this.data_chat_test = (IDictionary) Carrot.Json.Deserialize(JsonConvert.SerializeObject(chat_test));
        this.act_test_command(this.data_chat_test);
    }

    public void play_one_test_command(IDictionary data_chat)
    {
        this.data_chat_test = data_chat;
        if (this.box_list != null) this.box_list.close();
        if (this.app.command_dev.get_box_list() != null) this.app.command_dev.get_box_list().close();
        if (this.app.command_dev.get_box_list_same() != null) this.app.command_dev.get_box_list_same().close();

        this.is_list_command_test_play = false;
        this.obj_button_next_command_test.SetActive(false);
        this.obj_button_prev_command_test.SetActive(false);
        this.act_test_command(data_chat);
    }

    public void btn_close_test_command()
    {
        if (this.box_add_chat != null) this.box_add_chat.gameObject.SetActive(true);
        if (this.box_list != null) this.box_list.gameObject.SetActive(true);

        this.GetComponent<App>().panel_inp_command_test.SetActive(false);
        this.GetComponent<App>().panel_inp_func.SetActive(false);
        this.GetComponent<App>().panel_inp_msg.SetActive(true);
    }

    public void btn_back_edit_command()
    {
        if (this.is_list_command_test_play)
            this.show_edit_command(this.index_command_test_play);
        else
            this.app.panel_main.SetActive(false);
    }

    public void btn_replay_test_command()
    {
        if (this.is_list_command_test_play)
        {
            this.act_test_command(this.data_chat_test);
        }
        else
        {
            this.GetComponent<Command>().act_chat(data_chat_test, true);
            this.app.panel_inp_func.SetActive(false);
        }
    }

    public void play_test_command(int index_comand)
    {
        if (this.box_list != null) this.box_list.close();
        this.obj_button_next_command_test.SetActive(true);
        this.obj_button_prev_command_test.SetActive(true);
        this.is_list_command_test_play = true;
        this.index_command_test_play = index_comand;
        this.data_chat_test = (IDictionary)Carrot.Json.Deserialize(PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.index_command_test_play));
        this.act_test_command(this.data_chat_test);
    }

    private void act_test_command(IDictionary data_chat)
    {
        this.GetComponent<App>().panel_main.SetActive(true);
        this.GetComponent<App>().panel_inp_command_test.SetActive(true);
        this.GetComponent<App>().panel_inp_func.SetActive(false);
        this.GetComponent<App>().panel_inp_msg.SetActive(false);
        this.GetComponent<App>().panel_chat_func.SetActive(false);
        this.GetComponent<App>().panel_chat_msg.SetActive(true);
        if(this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait()) this.GetComponent<App>().panel_menu_right.SetActive(false);
        this.GetComponent<Command>().act_chat(data_chat, true);
    }

    public void btn_play_next_command_test()
    {
        this.index_command_test_play++;
        if (this.index_command_test_play >=this.length) this.index_command_test_play=0;
        string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.index_command_test_play);
        if (s_data != "") {
            this.data_chat_test = (IDictionary)Carrot.Json.Deserialize(s_data);
            this.act_test_command(this.data_chat_test);
        }
        else
        {
            this.btn_play_next_command_test();
        }
    }

    public void btn_play_prev_command_test()
    {
        this.index_command_test_play--;
        if (this.index_command_test_play < 0) this.index_command_test_play=(this.length-1);
        string s_data = PlayerPrefs.GetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.index_command_test_play);
        if (s_data != "")
        {
            this.data_chat_test = (IDictionary)Carrot.Json.Deserialize(s_data);
            this.act_test_command(this.data_chat_test);
        }
        else
        {
            this.btn_play_prev_command_test();
        }
    }
    #endregion

    private void act_box_add_model_nomal()
    {
        this.is_cm_mode_nomal = true;
        this.btn_model_advanced.set_icon_color(Color.black);
        this.btn_model_nomal.set_icon_color(this.GetComponent<App>().carrot.color_highlight);
        PlayerPrefs.SetInt("is_cm_mode_nomal", 0);
        this.item_icon.gameObject.SetActive(false);
        this.item_run_cmd.gameObject.SetActive(false);
        this.item_run_control.gameObject.SetActive(false);
        this.item_limit.gameObject.SetActive(false);
    }

    private void act_box_add_model_advanced()
    {
        this.is_cm_mode_nomal = false;
        this.btn_model_advanced.set_icon_color(this.GetComponent<App>().carrot.color_highlight);
        this.btn_model_nomal.set_icon_color(Color.black);
        PlayerPrefs.SetInt("is_cm_mode_nomal",1);
        this.item_icon.gameObject.SetActive(true);
        this.item_run_cmd.gameObject.SetActive(true);
        this.item_run_control.gameObject.SetActive(true);
        this.item_limit.gameObject.SetActive(true);
    }

    private void act_close_box()
    {
        this.s_id_icon = "";
        this.s_color = "";
        this.s_pater_id = "";
        this.s_pater_msg = "";
        this.GetComponent<App>().show_func_function();
        if (this.box_list != null)
        {
            if (this.box_list.gameObject.activeInHierarchy) return;
        }
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
                    Debug.Log("Key block:" + this.list_key_block.Count);
                }
            });
        }
    }

    private string check_keyblock(string s_key_check)
    {
        foreach(string s_key in this.list_key_block)
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
            key = this.item_keyword.get_val(),
            msg = this.item_msg.get_val(),
            action = this.item_action.get_val(),
            face = this.item_face.get_val(),
            limit = this.item_limit.get_val(),
            sex_character = this.app.setting.get_character_sex(),
            sex_user = this.app.setting.get_user_sex(),
            pater = this.s_pater_id,
            link = this.item_run_cmd.get_val(),
            color = this.s_color,
            icon = this.s_id_icon,
            status = "pending",
            func = this.item_run_control.get_val(),
            date_create = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        if (this.app.carrot.user.get_id_user_login() != "")
        {
            Carrot_Rate_user_data user_login = new Carrot_Rate_user_data();
            user_login.name = this.app.carrot.user.get_data_user_login("name");
            user_login.id = this.app.carrot.user.get_id_user_login();
            user_login.lang = this.app.carrot.user.get_lang_user_login();
            user_login.avatar = this.app.carrot.user.get_data_user_login("avatar");
            c.user = user_login;
        }
        return c;
    }

    private void act_done_submit_command()
    {
        this.app.carrot.show_loading();
        chat c = this.get_data_chat();
        if (this.type_act == Command_Type_Act.add_command)
        {
            string s_error_key_block = this.check_keyblock(this.item_keyword.get_val());

            if(this.item_keyword.get_val().Trim().Length==0|| this.item_msg.get_val().Trim().Length == 0)
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("error_null_key_and_msg", "Your keywords and content can't be blank!"));
                return;
            }

            if (s_error_key_block!="")
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"),PlayerPrefs.GetString("keyblock_keyword_error", "Added keyword is forbidden, please add more content ("+ s_error_key_block+")"));
                return;
            }

            s_error_key_block = this.check_keyblock(this.item_msg.get_val());

            if (s_error_key_block!="")
            {
                this.app.carrot.hide_loading();
                this.app.carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("keyblock_msg_error", "Keyword add in response forbidden content, please add more content (" +s_error_key_block+")"));
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
                    chatRef.SetAsync(c);
                    c.id = s_id_chat_new;
                    IDictionary chat_data = (IDictionary)Carrot.Json.Deserialize(JsonConvert.SerializeObject(c));
                    this.add_command_offline(chat_data);
                }

                if (this.app.carrot.model_app == ModelApp.Develope)
                {
                    if (this.s_id.Trim() == "")
                    {
                        string s_new_id= "chat" + this.app.carrot.generateID();
                        DocumentReference chatRef = chatDbRef.Document(s_new_id);
                        c.id = s_new_id;
                        c.status = "passed";
                        chatRef.SetAsync(c);
                    }
                    else
                    {
                        DocumentReference chatRef = chatDbRef.Document(this.s_id);
                        c.id = this.s_id;
                        c.status = "passed";
                        chatRef.SetAsync(c);
                    }

                    if (this.app.command_dev.get_box_list() != null) this.app.command_dev.get_box_list().close();
                }
            }
            else
            {
                c.id = "chat" + this.app.carrot.generateID();
                IDictionary chat_data= (IDictionary)Carrot.Json.Deserialize(JsonConvert.SerializeObject(c));
                this.add_command_offline(chat_data);
            }


            if (this.box_list != null) this.box_list.close();
            if (this.box_add_chat != null) this.box_add_chat.close();

            if(this.app.carrot.model_app==ModelApp.Publish)
                this.GetComponent<App>().carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), PlayerPrefs.GetString("brain_add_success", "Your chat has been published successfully!"));
            else
                this.GetComponent<App>().carrot.show_msg(PlayerPrefs.GetString("brain_add", "Create a new command"), "The chat has been published successfully! (Dev)");
        }

        if (this.type_act == Command_Type_Act.edit_command)
        {
            c.id = this.s_id;
            string s_chat_data = JsonConvert.SerializeObject(c);
            PlayerPrefs.SetString("command_offline_" + this.app.carrot.lang.get_key_lang() + "_" + this.app.setting.get_user_sex() + "_" + this.app.setting.get_character_sex() + "_" + this.index_cm_update, s_chat_data);
            this.app.carrot.hide_loading();
            if (this.box_list != null) this.box_list.close();
            if (this.box_add_chat != null) this.box_add_chat.close();
            this.show_list_cm(this.s_type_view_list);
        }

        this.app.carrot.ads.show_ads_Interstitial();
    }

    private void act_del_patert_chat()
    {
        this.s_pater_id = "";
        this.s_pater_msg = "";
        Destroy(this.item_patert.gameObject);
    }

    public void btn_add_key_tag(string s_tag)
    {
        string s_val= this.item_msg.get_val() + " " + s_tag + " ";
        this.item_msg.set_val(s_val);
        if (this.box_list != null) this.box_list.close();
    }

    public void show_list_parameter_tag()
    {
        this.box_list=this.GetComponent<App>().carrot.Create_Box("list_tag");
        this.box_list.set_title("Parameter Tag");
        this.box_list.set_icon(this.sp_icon_parameter_tag);

        for(int i = 0; i < this.list_parameter_tag_name.Length; i++)
        {
            var s_tag = this.list_parameter_tag_name[i];
            Carrot.Carrot_Box_Item item_tag=this.box_list.create_item("item_tag_" + i);
            item_tag.set_icon(this.sp_icon_parameter_tag);
            item_tag.set_title(this.list_parameter_tag_name[i]);
            item_tag.set_tip(this.list_parameter_tag_val[i]);
            item_tag.set_act(() => btn_add_key_tag(s_tag));

            Carrot.Carrot_Box_Btn_Item btn_add_tag =item_tag.create_item();
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
        box_list_key.set_title(PlayerPrefs.GetString("list_banned_keywords","List of prohibited keywords"));
        box_list_key.set_icon(this.app.setting.sp_icon_chat_limit);

        for(int i = 0; i < this.list_key_block.Count; i++)
        {
            Carrot.Carrot_Box_Item item_key=box_list_key.create_item("key_block_" + i);
            item_key.set_icon(this.sp_icon_key);
            item_key.set_title(this.list_key_block[i].ToString());
            item_key.set_tip(this.list_key_block[i].ToString());
        } 
    }

    private void act_translate(string s_txt)
    {
        s_txt=UnityWebRequest.EscapeURL(s_txt);
        string s_tr = "https://translate.google.com/?sl="+this.app.carrot.lang.get_key_lang()+"&tl=vi&text="+s_txt+"&op=translate";
        Application.OpenURL(s_tr);
    }
}
