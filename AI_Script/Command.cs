﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Carrot;
using Crosstales;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Command : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    [Header("Obj chat")]
    public InputField inp_command;
    public GameObject prefab_item_command_chat;
    public GameObject prefab_item_command_pc;
    public GameObject prefab_item_command_loading;
    public GameObject prefab_effect_icon_chat;

    public GameObject obj_btn_clear_all_log;
    public Transform area_body_log_command;
    public ScrollRect ScrollRect_log_command;
    public AudioSource sound_command;
    public string chat_pater = "";
    private bool is_test_command = false;

    public Sprite icon_pc_music;
    public Sprite icon_info_chat;
    public Sprite sp_icon_info_add_chat;
    public Sprite sp_icon_info_share_chat;
    public Sprite sp_icon_info_report_chat;

    [Header("Effect show text chat")]
    public Text txt_show_chat;
    public Image img_border_color_chat;
    public GameObject panel_show_msg_chat;
    public GameObject panel_show_log_chat;
    public GameObject obj_btn_info_chat;
    public GameObject obj_btn_report_chat;
    public GameObject obj_btn_new_chat;
    public GameObject obj_btn_add_chat_whith_father;
    public GameObject obj_btn_translate;
    public GameObject obj_btn_more;
    public GameObject obj_btn_log;

    [Header("Ui")]
    public Text txt_cmd_pending_title;
    public Text txt_cmd_sys_title;

    private float count_timer_show_text = 0;
    private float count_timer_hide_text = 0;
    private int index_text = 0;
    private string s_chat_temp;
    private bool is_hide_text = false;
    private bool is_show_text = false;

    private Carrot.Carrot_Box box_list;
    private string s_command_chat_last;
    private string id_cur_chat = "";
    private IDictionary data_chat_cur;
    private GameObject item_command_loading;
    private float timer_msg_tip = 0;
    private bool is_tts = false;
    private bool is_ready_msg_tip = false;
    IList all_item = null;
    private string file_name_data_chat = "";
    public void load()
    {
        this.obj_btn_clear_all_log.SetActive(false);
        this.app.textToSpeech.onStartCallBack = this.act_start_speech;
        this.app.textToSpeech.onDoneCallback = this.act_done_speech;
    }

    public void load_all_data_chat(UnityAction act_done)
    {
        this.file_name_data_chat = "chat-" + PlayerPrefs.GetString("lang", "en") + ".json";
        if (this.app.carrot.get_tool().check_file_exist(this.file_name_data_chat))
        {
            string s_data = this.app.carrot.get_tool().get_file_path(this.file_name_data_chat);
            string s_text = FileHelper.ReadAllText(s_data);
            load_all_item_chat(s_text);
            act_done.Invoke();
        }
        else
        {
            this.app.carrot.Get_Data("https://raw.githubusercontent.com/kurotsmile/Database-Store-Json/refs/heads/main/" + this.file_name_data_chat, (s_data) =>
            {
                this.load_all_item_chat(s_data);
                this.app.carrot.get_tool().save_file(this.file_name_data_chat, s_data);
                act_done.Invoke();
            });
        }
    }

    private void load_all_item_chat(string s_data)
    {
        IDictionary data = (IDictionary)Json.Deserialize(s_data);
        this.all_item = data["all_item"] as IList;
        Debug.Log("Load " + all_item.Count + " Chat cmd");
        for (int i = 0; i < all_item.Count; i++)
        {
            IDictionary item = all_item[i] as IDictionary;
            if (item != null)
            {
                item["index_item"] = i;
                List<object> keysToRemove = new List<object>();
                foreach (DictionaryEntry entry in item)
                {
                    if (entry.Value == null || (entry.Value is string str && string.IsNullOrEmpty(str))) keysToRemove.Add(entry.Key);
                }

                foreach (object key in keysToRemove)
                {
                    item.Remove(key);
                }
            }
        }
        this.Update_UI_info();
    }

    public void Update_UI_info()
    {
        this.txt_cmd_pending_title.text = this.app.carrot.L("brain_list", "Command list") + " (" + this.GetPendingCmd().Count + ")";
        this.txt_cmd_sys_title.text = this.app.carrot.L("cmd_sys_list", "System chat list") + " (" + this.all_item.Count + ")";
    }

    public void send_command()
    {
        if (this.inp_command.text.Trim() != "")
        {
            this.send_chat(this.inp_command.text, true);
            this.inp_command.text = "";
        }
    }

    public void send_chat_no_father(string s_key, bool is_log_show = false)
    {
        this.id_cur_chat = "";
        this.send_chat(s_key, is_log_show);
    }

    public void send_chat(string s_key, bool is_log_show = false)
    {
        s_key = s_key.Trim().ToLower();

        this.s_command_chat_last = s_key;
        this.On_reset_timer_msg_tip();

        if (is_log_show)
        {
            this.Add_item_log_chat(s_key);
            this.Add_item_log_loading();
            if (this.app.player_music.playlist.Check_query_key(s_key)) return;
        }

        IDictionary db_chat = this.Check_cmd_data(s_key);
        if (db_chat != null)
        {
            this.act_chat(db_chat);
            Debug.Log("Co -------> child");
            return;
        }
        else
        {
            this.id_cur_chat = "";
            IDictionary db_chat_again = this.Check_cmd_data(s_key);
            if (db_chat_again != null)
            {
                this.act_chat(db_chat_again);
                Debug.Log("Co ------> No Child");
                return;
            }
        }

        Debug.Log("chat online (Id father:" + this.id_cur_chat + ")");
        if (this.app.carrot.is_online())
        {
            if (this.app.setting.get_index_prioritize() == 0)
            {
                this.play_chat(s_key);
            }
            else if (this.app.setting.get_index_prioritize() == 1)
            {
                this.play_chat(s_key);
            }
            else if (this.app.setting.get_index_prioritize() == 2)
            {
                if (this.app.gemini_AI.is_active == false && this.app.open_AI.is_active == false)
                {
                    this.show_msg_no_chat();
                }
                else
                {
                    if (this.app.open_AI.is_active)
                        this.app.open_AI.send_chat(s_key);
                    else
                        this.app.gemini_AI.send_chat(s_key);
                }
            }
            else if (this.app.setting.get_index_prioritize() == 3)
            {
                if (this.app.gemini_AI.is_active == false && this.app.open_AI.is_active == false)
                {
                    this.show_msg_no_chat();
                }
                else
                {
                    if (this.app.gemini_AI.is_active)
                        this.app.gemini_AI.send_chat(s_key);
                    else
                        this.app.open_AI.send_chat(s_key);
                }
            }
        }
        else
        {
            this.show_msg_no_chat();
        }
    }
    private IDictionary Check_cmd_data(string s_key)
    {
        Debug.Log("Check_cmd_data");
        if (all_item != null)
        {
            IList list_cmd = Json.Deserialize("[]") as IList;
            for (int i = 0; i < this.all_item.Count; i++)
            {
                IDictionary item_c = this.all_item[i] as IDictionary;
                if (item_c["pater"] == null) item_c["pater"] = "";
                if (item_c["key"] == null) item_c["key"] = "";
                if (item_c["key"].ToString().Trim().ToLower() == s_key.Trim() &&
                    item_c["sex_user"].ToString().ToLower() == this.app.setting.get_user_sex() &&
                    item_c["sex_character"].ToString().ToLower() == this.app.setting.get_character_sex() &&
                    item_c["pater"].ToString() == this.id_cur_chat)
                    list_cmd.Add(item_c);
            }

            if (list_cmd.Count == 0)
            {
                for (int i = 0; i < this.all_item.Count; i++)
                {
                    IDictionary item_c = this.all_item[i] as IDictionary;

                    if (item_c["pater"] == null) item_c["pater"] = "";
                    if (item_c["key"] == null) item_c["key"] = "";
                    if (item_c["key"].ToString().Trim().ToLower().Contains(s_key.Trim()) &&
                        item_c["sex_user"].ToString().ToLower() == this.app.setting.get_user_sex() &&
                        item_c["sex_character"].ToString().ToLower() == this.app.setting.get_character_sex() &&
                        item_c["pater"].ToString() == this.id_cur_chat)
                        list_cmd.Add(item_c);
                }
            }

            if (list_cmd.Count != 0)
            {
                IDictionary data_c;
                if (list_cmd.Count == 0)
                {
                    data_c = list_cmd[0] as IDictionary;
                }
                else
                {
                    int r_index = Random.Range(0, list_cmd.Count);
                    data_c = list_cmd[r_index] as IDictionary;
                }
                if (data_c["id_import"] != null) data_c["id"] = data_c["id_import"];
                return data_c;
            }
        }
        return null;
    }

    private void play_chat(string s_key)
    {
        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Add_where("key", Query_OP.EQUAL, s_key);
        q.Add_where("sex_user", Query_OP.EQUAL, this.app.setting.get_user_sex());
        q.Add_where("sex_character", Query_OP.EQUAL, this.app.setting.get_character_sex());
        q.Add_where("pater", Query_OP.EQUAL, this.id_cur_chat);
        q.Set_limit(10);
        this.app.carrot.server.Get_doc(q.ToJson(), Act_doc_done, Act_doc_fail);
    }

    private void Act_doc_done(string s_data)
    {
        Fire_Collection fc = new(s_data);
        if (this.id_cur_chat != "")
        {
            if (fc.is_null)
            {
                this.id_cur_chat = "";
                this.play_chat(this.s_command_chat_last);
            }
            else
            {
                this.act_chat(fc.Get_doc_random().Get_IDictionary());
            }
        }
        else
        {
            if (fc.is_null)
                this.Send_to_all_ai(this.s_command_chat_last);
            else
                this.act_chat(fc.Get_doc_random().Get_IDictionary());
        }
    }

    private void Act_doc_fail(string s_data)
    {
        this.Send_to_all_ai(this.s_command_chat_last);
    }

    private void Send_to_all_ai(string s_key)
    {
        if (this.app.gemini_AI.is_active == false && this.app.open_AI.is_active == false)
        {
            this.show_msg_no_chat();
        }
        else
        {
            if (this.app.setting.get_index_prioritize() == 0)
            {
                if (this.app.open_AI.is_active)
                    this.app.open_AI.send_chat(s_key);
                else
                    this.app.gemini_AI.send_chat(s_key);
            }
            else
            {
                if (this.app.gemini_AI.is_active)
                    this.app.gemini_AI.send_chat(s_key);
                else
                    this.app.open_AI.send_chat(s_key);
            }
        }
    }

    private void Hide_all_obj_msg()
    {
        this.id_cur_chat = "";
        this.obj_btn_info_chat.SetActive(false);
        this.obj_btn_report_chat.SetActive(false);
        this.obj_btn_add_chat_whith_father.SetActive(false);
        this.obj_btn_new_chat.SetActive(true);
        this.obj_btn_translate.SetActive(false);
        this.obj_btn_more.SetActive(false);
        this.set_color(Color.red);
    }

    public void show_msg_no_chat()
    {
        if (this.item_command_loading != null) Destroy(this.item_command_loading);
        this.data_chat_cur = null;
        this.Hide_all_obj_msg();
        this.show_effect_txt_msg(app.carrot.L("no_chat", "No related answers yet, please teach me!"));
    }

    public void send_command_by_text(string s_text)
    {
        this.inp_command.text = s_text;
        this.send_command();
    }

    private void Add_item_log_chat(string s_inp_command)
    {
        GameObject item_command_chat = Instantiate(this.prefab_item_command_chat);
        item_command_chat.transform.SetParent(this.area_body_log_command);
        item_command_chat.transform.localScale = new Vector3(1f, 1f, 1f);
        item_command_chat.transform.localPosition = new Vector3(item_command_chat.transform.localPosition.x, item_command_chat.transform.localPosition.y, 0f);
        item_command_chat.transform.localRotation = Quaternion.Euler(Vector3.zero);
        item_command_chat.name = "user_chat";

        Item_command_chat c_chat = item_command_chat.GetComponent<Item_command_chat>();
        c_chat.txt_chat.text = s_inp_command;
        c_chat.txt_chat.color = this.app.carrot.color_highlight;
        c_chat.icon.sprite = this.app.get_icon_sex_user();
        c_chat.btn_add_app.GetComponent<Image>().color = this.app.carrot.color_highlight;

        IDictionary Idata = (IDictionary)Json.Deserialize("{}");
        Idata["id"] = "chat" + this.app.carrot.generateID();
        Idata["key"] = s_inp_command;
        Idata["msg"] = s_inp_command;
        Idata["status"] = "input";

        c_chat.idata_chat = Idata;
        c_chat.set_act_click(() => this.app.command_storage.show_new_command(s_inp_command));
        c_chat.set_act_add(() => this.app.command_storage.show_new_command(s_inp_command));

        if (data_chat_cur != null)
        {
            Debug.Log(Json.Serialize(data_chat_cur));
            c_chat.btn_extension.GetComponent<Button>().onClick.RemoveAllListeners();
            string s_id = data_chat_cur["id"].ToString();
            string s_msg = data_chat_cur["msg"].ToString();
            c_chat.btn_extension.GetComponent<Button>().onClick.AddListener(() => this.app.command_storage.show_add_command_with_pater_and_keyword(s_inp_command, s_msg, s_id));
        }
        else
        {
            c_chat.btn_extension.SetActive(false);
        }
    }

    private void Add_item_log_loading()
    {
        if (this.item_command_loading != null)
        {
            Destroy(this.item_command_loading);
            this.app.open_AI.stop_All_Action();
            this.app.gemini_AI.stop_All_Action();
        }
        this.item_command_loading = Instantiate(this.prefab_item_command_loading);
        this.item_command_loading.transform.SetParent(this.area_body_log_command);
        this.item_command_loading.transform.localScale = new Vector3(1f, 1f, 1f);
        this.item_command_loading.transform.localPosition = new Vector3(item_command_loading.transform.localPosition.x, item_command_loading.transform.localPosition.y, 0f);
        this.item_command_loading.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this.item_command_loading.name = "loading_chat";
        this.ScrollRect_log_command.verticalNormalizedPosition = -1f;
    }

    public Item_command_chat add_item_pc_chat(string s_txt_show, Sprite icon, IDictionary i_data = null)
    {
        if (this.item_command_loading != null) Destroy(this.item_command_loading);

        GameObject item_command_chat = Instantiate(this.prefab_item_command_pc);
        item_command_chat.transform.SetParent(this.area_body_log_command);
        item_command_chat.transform.localPosition = new Vector3(item_command_chat.transform.localPosition.x, item_command_chat.transform.localPosition.y, 0f);
        item_command_chat.transform.localScale = new Vector3(1f, 1f, 1f);
        item_command_chat.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Item_command_chat comand_chat = item_command_chat.GetComponent<Item_command_chat>();
        comand_chat.txt_chat.text = s_txt_show;
        comand_chat.icon.sprite = icon;
        comand_chat.btn_add_app.GetComponent<Image>().color = this.app.carrot.color_highlight;

        if (i_data != null)
        {
            i_data["type_command"] = "log";
            comand_chat.idata_chat = i_data;
            if (i_data["status"] != null)
            {
                string s_status = i_data["status"].ToString();
                if (s_status == "music")
                {
                    item_command_chat.gameObject.name = "music_item";
                    comand_chat.btn_add_app.SetActive(false);
                    comand_chat.set_act_click(() => this.app.player_music.act_play_data(i_data));
                }
                else
                {
                    comand_chat.set_act_add(() => this.app.command_storage.show_add_command_with_pater(i_data["msg"].ToString(), i_data["id"].ToString()));
                    comand_chat.set_act_click(() => this.act_chat(i_data, false));
                }
            }
        }
        else
        {
            comand_chat.btn_add_app.SetActive(false);
        }

        this.app.set_item_cur_log_chat(comand_chat);
        this.ScrollRect_log_command.verticalNormalizedPosition = -1f;
        Canvas.ForceUpdateCanvases();
        return comand_chat;
    }

    public void add_item_log_music(string s_name, IDictionary id_data)
    {
        id_data["status"] = "music";
        this.add_item_pc_chat(s_name, this.icon_pc_music, id_data);
    }

    public void act_chat(IDictionary data_chat, bool is_add_log = true)
    {
        this.On_reset_timer_msg_tip();
        this.obj_btn_log.SetActive(true);
        this.obj_btn_info_chat.SetActive(true);
        this.obj_btn_translate.SetActive(true);
        this.obj_btn_new_chat.SetActive(false);
        this.obj_btn_report_chat.SetActive(false);
        this.obj_btn_add_chat_whith_father.SetActive(false);
        this.obj_btn_clear_all_log.SetActive(false);
        this.obj_btn_more.SetActive(true);

        this.data_chat_cur = data_chat;
        this.app.panel_main.SetActive(true);
        this.app.textToSpeech.StopSpeak();
        this.app.get_character().get_npc().transform.eulerAngles = Vector3.zero;
        this.app.get_character().get_npc().transform.localPosition = Vector3.zero;
        if (data_chat["id"] != null)
        {
            this.id_cur_chat = data_chat["id"].ToString();
        }

        if (data_chat["status"] != null)
        {
            string s_status = data_chat["status"].ToString();
            if (s_status == "passed") this.obj_btn_report_chat.SetActive(true);
            if (s_status == "passed" || s_status == "pending" || s_status == "buy") this.obj_btn_add_chat_whith_father.SetActive(true);
            if (s_status == "test" || s_status == "test_list")
            {
                this.is_test_command = true;
                this.obj_btn_log.SetActive(false);
                this.obj_btn_more.SetActive(false);

                if (data_chat["sex_character"] != null)
                {
                    string sex_character = data_chat["sex_character"].ToString();
                    this.app.show_character_on_test(sex_character);
                }
            }
        }

        string s_msg_chat = "";
        if (data_chat["msg"] != null)
        {
            s_msg_chat = data_chat["msg"].ToString();
            s_msg_chat = this.app.s_rep_key(s_msg_chat);
            this.show_effect_txt_msg(s_msg_chat);
            if (this.app.setting.get_status_sound_voice()) this.play_text_audio(s_msg_chat);
        }

        if (data_chat["action"] != null) this.app.action.play_act_anim_by_index_default(int.Parse(data_chat["action"].ToString()));
        if (data_chat["act"] != null) this.app.action.play_act_anim(data_chat["act"].ToString());
        if (data_chat["face"] != null) this.act_cm_face(data_chat["face"].ToString());
        if (data_chat["color"] != null) this.set_color_by_string(data_chat["color"].ToString());

        if (data_chat["ai"] != null)
        {
            this.id_cur_chat = "";
        }

        if (this.app.setting.get_status_sound_voice())
        {
            if (this.app.carrot.is_online())
            {
                if (data_chat["mp3"] != null)
                {
                    if (data_chat["mp3"].ToString() != "") StartCoroutine(get_audio_chat_form_url(data_chat["mp3"].ToString()));
                }
            }
        }

        if (data_chat["link"] != null) if (data_chat["link"].ToString().Trim() != "") Application.OpenURL(data_chat["link"].ToString());
        if (data_chat["func"] != null)
        {
            string index_func = data_chat["func"].ToString();
            if (index_func == "1") this.app.sel_menu_with_sound(2);
            if (index_func == "2") this.app.sel_menu_with_sound(0);
            if (index_func == "3") this.app.sel_menu_with_sound(3);
            if (index_func == "4") this.app.command_storage.show_add_command_new();
            if (index_func == "5") this.app.player_music.play_new_song();
            if (index_func == "6") this.app.player_music.btn_stop();
            if (index_func == "7") this.app.player_music.playlist.show_playlist();
            if (index_func == "8") this.app.player_music.playlist.show_list_music_online();
            if (index_func == "9") this.app.player_music.playlist.show_list_radio();
            if (index_func == "10") this.app.carrot.show_rate();
            if (index_func == "11") this.app.carrot.show_share();
            if (index_func == "12") this.app.carrot.delay_function(3.6f, this.app.exit_app);
            if (index_func == "13") this.app.carrot.delay_function(2f, this.app.view.show_list_background_image);
            if (index_func == "14") this.app.player_music.btn_next();
            if (index_func == "15") this.app.player_music.btn_pause();
            if (index_func == "16") this.app.carrot.delay_function(1.6f, () => this.app.tool.open_content_Intent(data_chat["link"].ToString().Trim()));
            if (index_func == "17") this.app.tool.on_Flashlight();
            if (index_func == "18") this.app.tool.off_Flashlight();
            if (index_func == "19") this.app.carrot.delay_function(1.6f, () => this.app.tool.OpenApp_by_bundleId(data_chat["link"].ToString().Trim()));
            if (index_func == "20") this.app.carrot.delay_function(1.6f, () => this.app.setting.show_shop());
        }
        if (data_chat["icon"] != null)
        {
            if (data_chat["icon"].ToString() != "")
            {
                string s_id_icon = data_chat["icon"].ToString();
                if (s_id_icon != "undefined")
                {
                    Sprite sp_icon_chat = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_icon);
                    if (sp_icon_chat != null)
                    {
                        this.act_play_effect_icon_chat(sp_icon_chat.texture);
                    }
                    else
                    {
                        if (s_id_icon != "") this.get_effect_icon_chat(s_id_icon);
                    }
                }
            }
        }

        if (is_add_log)
        {
            this.add_item_pc_chat(s_msg_chat, this.app.get_character().icon_sex, data_chat);
        }

        this.app.ads.show_ads_Interstitial();
    }

    public void act_cm_face(string s_face)
    {
        if (s_face != "") this.play_face(int.Parse(s_face));
    }

    public void play_face(int index)
    {
        this.app.get_character().play_ani_face(index);
    }

    private void Action_waitting()
    {
        this.app.get_character().play_ani_waitting();
    }

    void Update()
    {
        if (this.is_show_text)
        {
            this.count_timer_show_text = this.count_timer_show_text + Time.deltaTime;
            if (this.count_timer_show_text >= 0.02f)
            {
                this.index_text++;
                this.count_timer_show_text = 0f;
                this.txt_show_chat.text = this.s_chat_temp.Substring(0, this.index_text);
                if (this.index_text >= this.s_chat_temp.Length)
                {
                    this.is_show_text = false;
                    this.is_hide_text = true;
                    this.On_reset_timer_msg_tip();
                }
            }
        }

        if (this.is_hide_text)
        {
            this.count_timer_hide_text = this.count_timer_hide_text + Time.deltaTime;
            if (this.count_timer_hide_text > 5f)
            {
                this.count_timer_hide_text = 0;
                this.is_hide_text = false;
                this.panel_show_msg_chat.SetActive(false);
                this.panel_show_log_chat.SetActive(true);
                this.On_reset_timer_msg_tip();
                if (this.area_body_log_command.childCount > 0) this.obj_btn_clear_all_log.SetActive(true);
                if (this.app.player_music.sound_music.isPlaying == false) this.Action_waitting();
            }
        }


        this.timer_msg_tip += Time.deltaTime;
        if (this.timer_msg_tip > 30f)
        {
            this.timer_msg_tip = 0;
            if (this.app.player_music.sound_music.isPlaying == false && this.app.panel_chat_msg.activeInHierarchy == true && this.is_show_text == false && this.is_tts == false && this.is_ready_msg_tip == true)
            {
                this.send_chat_no_father("tip");
            }
        }
    }

    public void On_reset_timer_msg_tip()
    {
        this.timer_msg_tip = 0f;
    }

    public void show_effect_txt_msg(string s_txt)
    {
        if (s_txt.Trim() != "")
        {
            this.txt_show_chat.text = "";
            this.s_chat_temp = s_txt;
            this.index_text = 0;
            this.count_timer_show_text = 0;
            this.count_timer_hide_text = 0;
            this.is_hide_text = false;
            this.is_show_text = true;
            this.panel_show_msg_chat.SetActive(true);
            this.panel_show_log_chat.SetActive(false);
            if (this.is_test_command == false) this.app.show_chat_function();
        }
    }

    public void play_text_audio(string s_chat)
    {
        if (this.app.setting.get_tts_type() == "0")
        {
            if (this.app.carrot.is_online())
            {
                StartCoroutine(get_audio_chat_form_txt(s_chat, this.app.carrot.lang.Get_key_lang()));
            }
            else
            {
                this.app.textToSpeech.StartSpeak(s_chat);
            }
        }
        else if (this.app.setting.get_tts_type() == "2")
        {
            this.app.textToSpeech.StartSpeak(s_chat);
        }
        else
        {
            StartCoroutine(get_audio_chat_form_txt(s_chat, this.app.carrot.lang.Get_key_lang()));
        }
    }

    public IEnumerator get_audio_chat_form_txt(string txt_chat, string s_lang)
    {
        string s_url_audio = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=" + txt_chat.Length + "&client=tw-ob&q=" + txt_chat + "&tl=" + s_lang;
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            this.app.textToSpeech.StartSpeak(txt_chat);
        }
        else
        {
            this.play_sound_chat(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    public IEnumerator get_audio_chat_form_url(string s_url_audio)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            this.play_sound_chat(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    public string get_id_chat_cur()
    {
        return this.id_cur_chat;
    }

    private void play_sound_chat(AudioClip audio_clip)
    {
        this.sound_command.clip = audio_clip;
        this.sound_command.pitch = this.app.setting.get_voice_speed();
        this.sound_command.Play();
    }

    public void get_msg_hit()
    {
        this.send_chat("hit", true);
    }

    private void get_effect_icon_chat(string s_id_icon)
    {
        this.app.carrot.server.Get_doc_by_path("icon", s_id_icon, act_get_effect_icon_chat_done);
    }

    private void act_get_effect_icon_chat_done(string s_data)
    {
        Fire_Document fd = new(s_data);
        IDictionary icon_data = fd.Get_IDictionary();
        this.app.carrot.get_img_and_save_playerPrefs(icon_data["icon"].ToString(), null, fd.Get_id(), act_play_effect_icon_chat);
    }

    private void act_play_effect_icon_chat(Texture2D tex_icon)
    {
        GameObject effect_icon = Instantiate(this.prefab_effect_icon_chat);
        effect_icon.transform.SetParent(this.app.view.mouseOrbit_Improved.target.transform);
        effect_icon.transform.localPosition = Vector3.zero;
        effect_icon.transform.localScale = new Vector3(1f, 1f, 1f);

        ParticleSystem sys_effec = effect_icon.GetComponent<ParticleSystem>();
        ParticleSystemRenderer renderer_effec = sys_effec.GetComponent<ParticleSystemRenderer>();
        renderer_effec.material.mainTexture = tex_icon;
        Destroy(effect_icon, 3f);
    }

    public void clear_log_chat()
    {
        this.app.carrot.clear_contain(this.area_body_log_command);
    }

    public void show_box_link_share_chat(string s_link_share)
    {
        this.app.carrot.show_share(s_link_share, app.carrot.L("share_chat_tip", "You can share and show this dialogue to others if they also have the app installed"));
    }

    public string get_s_command_chat_last()
    {
        return this.s_command_chat_last;
    }

    public void set_color_by_string(string s_color)
    {
        if (!s_color.Contains("#")) s_color = "#" + s_color;
        Color color_item;
        ColorUtility.TryParseHtmlString(s_color, out color_item);
        this.set_color(color_item);
    }

    public void set_color(Color32 color_set)
    {
        this.img_border_color_chat.color = color_set;
    }

    public void show_info_chat()
    {
        if (this.data_chat_cur == null) return;
        this.box_info_chat(this.data_chat_cur);
    }

    public void Show_info_chat_by_index(int index)
    {
        this.box_info_chat(this.all_item[index] as IDictionary);
    }

    public void Show_info_chat_by_id(string s_id_chat)
    {
        IDictionary data_chat = this.GetCmdById(s_id_chat);
        if (s_id_chat == null || s_id_chat.Trim() == "")
        {
            this.app.carrot.Show_msg("Chat Info", app.carrot.L("no_chat", "No related answers yet, please teach me!"), Carrot.Msg_Icon.Alert);
            return;
        }
        this.box_info_chat(data_chat);
    }

    private void Act_show_info_chat_by_id_done(string s_data)
    {
        Fire_Document doc = new(s_data);
        IDictionary data_info = doc.Get_IDictionary();
        if (data_info != null)
            this.box_info_chat(data_info);
        else
            this.app.carrot.Show_msg("Chat Info", app.carrot.L("no_chat", "No related answers yet, please teach me!"), Carrot.Msg_Icon.Alert);
    }

    private void Act_show_info_chat_by_id_fail(string s_error)
    {
        this.app.carrot.Show_msg("Chat Info", "The data retrieval process encountered a problem!", Carrot.Msg_Icon.Error);
    }

    public void box_info_chat(IDictionary data_chat)
    {
        if (data_chat == null)
        {
            this.app.carrot.Show_msg(app.carrot.L("command_pass", "Published chat"), "There is no data for this chat!");
            return;
        }

        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.Create_Box("info_chat");
        this.box_list.set_title(app.carrot.L("command_pass", "Published chat"));
        this.box_list.set_icon(this.icon_info_chat);

        foreach (var key in data_chat.Keys)
        {
            if (data_chat[key] != null)
            {
                if (key.ToString() == "user")
                {
                    if (data_chat["user"] != null)
                    {
                        if (data_chat["user"].ToString().Trim() != "")
                        {
                            IDictionary data_user = (IDictionary)data_chat[key];
                            if (data_user["id"] != null)
                            {
                                string user_id = data_user["id"].ToString();
                                string user_lang = data_user["lang"].ToString();
                                Carrot.Carrot_Box_Item item_user = this.box_list.create_item("item_" + key.ToString());
                                item_user.set_title(app.carrot.L("chat_creator", "Creator"));
                                item_user.set_icon(this.icon_info_chat);
                                item_user.set_tip(data_user["name"].ToString());

                                if (data_user["avatar"] != null)
                                {
                                    Sprite sp_avatar = this.app.carrot.get_tool().get_sprite_to_playerPrefs("avatar_user_" + data_user["id"]);
                                    if (sp_avatar != null) item_user.set_icon_white(sp_avatar);
                                    else this.app.carrot.get_img_and_save_playerPrefs(data_user["avatar"].ToString(), item_user.img_icon, "avatar_user_" + data_user["id"]);
                                }

                                item_user.set_act(() => this.app.carrot.user.show_user_by_id(user_id, user_lang));
                            }
                        }
                    }
                }
                else
                {
                    string s_data_val = data_chat[key].ToString();
                    string s_key = key.ToString();
                    if (s_data_val != "")
                    {
                        string s_field_title = "";
                        string s_field_val = "";

                        Carrot.Carrot_Box_Item item_field = this.box_list.create_item("item_" + key.ToString());

                        if (s_key == "sex_user")
                        {
                            s_field_title = app.carrot.L("user_sex", "User Sex");
                            if (s_data_val == "0") s_field_val = app.carrot.L("user_sex_boy", "Male");
                            else s_field_val = app.carrot.L("user_sex_girl", "Female");
                            item_field.set_icon(this.app.setting.sp_icon_sex_user);
                        }
                        else if (s_key == "sex_character")
                        {
                            s_field_title = app.carrot.L("setting_char_sex", "Character Sex");
                            if (s_data_val == "0") s_field_val = app.carrot.L("user_sex_boy", "Male");
                            else s_field_val = app.carrot.L("user_sex_girl", "Female");
                            item_field.set_icon(this.app.setting.sp_icon_sex_character);
                        }
                        else if (s_key == "face")
                        {
                            s_field_title = app.carrot.L("face", "Face");
                            s_field_val = app.carrot.L("face", "Face") + " " + s_data_val;
                            item_field.set_icon(this.app.command_storage.sp_icon_face);
                        }
                        else if (s_key == "action")
                        {
                            s_field_title = app.carrot.L("act", "Action");
                            s_field_val = app.carrot.L("act", "Action") + " " + s_data_val;
                            item_field.set_icon(this.app.command_storage.sp_icon_action);
                        }
                        else if (s_key == "msg")
                        {
                            s_field_title = app.carrot.L("cm_msg", "Feedback message");
                            s_field_val = s_data_val;
                            item_field.set_icon(this.app.command_storage.sp_icon_msg);

                            Carrot_Box_Btn_Item btn_copy_msg = item_field.create_item();
                            btn_copy_msg.set_icon(this.app.carrot.icon_carrot_write);
                            btn_copy_msg.set_color(this.app.carrot.color_highlight);
                            btn_copy_msg.set_act(() => this.act_open_copy_msg(s_data_val));
                        }
                        else if (s_key == "key")
                        {
                            s_field_title = app.carrot.L("cm_keyword", "Keywords");
                            s_field_val = s_data_val;
                            item_field.set_icon(this.app.command_storage.sp_icon_key);
                        }
                        else if (s_key == "color")
                        {
                            s_field_title = app.carrot.L("bk_color", "Color");
                            s_field_val = s_data_val;
                            item_field.set_icon(this.app.setting.sp_icon_select_color);
                            if (s_field_val.ToLower() != "#ffffff")
                            {
                                Color newCol;
                                if (ColorUtility.TryParseHtmlString(s_data_val, out newCol)) item_field.img_icon.color = newCol;
                            }
                        }
                        else if (s_key == "icon")
                        {
                            s_field_title = app.carrot.L("setting_bubble_icon", "Icons");
                            s_field_val = s_data_val;
                            item_field.set_icon(this.app.setting.sp_icon_select_color);
                            if (s_data_val != "")
                            {
                                Sprite sp_icon_chat = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_data_val);
                                if (sp_icon_chat != null) item_field.set_icon_white(sp_icon_chat);
                            }
                        }
                        else if (s_key == "func")
                        {
                            s_field_title = app.carrot.L("cm_func_app", "Program control");
                            item_field.set_icon(this.app.command_storage.sp_icon_func);
                            try
                            {
                                int index_name_func = int.Parse(s_data_val);
                                s_field_val = this.app.command_storage.func_app_name[index_name_func];
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log(e.Message);
                                s_field_val = "None";
                            }
                        }
                        else if (s_key == "reports")
                        {
                            IList list_report = (IList)data_chat["reports"];
                            s_field_title = app.carrot.L("report_title", "Report");
                            s_field_val = list_report.Count + " Report";
                            item_field.set_icon(this.sp_icon_info_report_chat);
                            item_field.set_act(() => this.app.report.show_list_report(list_report));

                            Carrot_Box_Btn_Item btn_list_report = item_field.create_item();
                            btn_list_report.set_icon(this.app.carrot.icon_carrot_bug);
                            btn_list_report.set_color(this.app.carrot.color_highlight);
                            Destroy(btn_list_report.GetComponent<Button>());
                        }
                        else
                        {
                            s_field_title = s_key;
                            s_field_val = s_data_val;
                            item_field.set_icon(this.icon_info_chat);
                        }

                        item_field.set_title(s_field_title);
                        item_field.set_tip(s_field_val);
                    }
                }
            }
        }

        if (data_chat["status"] != null)
        {
            string s_status = data_chat["status"].ToString();
            if (s_status == "passed")
            {
                string s_id_chat = "";
                Carrot_Box_Btn_Panel panel_btn = this.box_list.create_panel_btn();
                if (data_chat["id"] != null)
                {
                    s_id_chat = data_chat["id"].ToString();
                    string s_link_share = this.app.carrot.mainhost + "/?p=chat&id=" + s_id_chat + "&lang_chat=" + this.app.carrot.lang.Get_key_lang();
                    Carrot_Button_Item btn_share = panel_btn.create_btn("item_share");
                    btn_share.set_bk_color(this.app.carrot.color_highlight);
                    btn_share.set_icon_white(this.app.carrot.sp_icon_share);
                    btn_share.set_label_color(Color.white);
                    btn_share.set_label(app.carrot.L("share", "Share"));
                    btn_share.set_act_click(() => this.share_chat(s_link_share));
                }
            }
        }
    }

    private void act_open_copy_msg(string s_msg)
    {
        this.app.carrot.Show_input("Copy", "You can copy the content here", s_msg);
    }

    public void btn_new_chat_with_fater()
    {
        this.app.command_storage.show_add_command_with_pater(this.txt_show_chat.text, this.id_cur_chat);
    }

    public void btn_new_chat()
    {
        this.app.command_storage.show_new_command(this.s_command_chat_last);
    }

    private void share_chat(string s_link)
    {
        this.app.carrot.show_share(s_link, "Share this conversation with your friends!");
    }

    public void btn_translate()
    {
        string s_txt = this.data_chat_cur["msg"].ToString();
        s_txt = UnityWebRequest.EscapeURL(s_txt);
        string s_tr = "https://translate.google.com/?sl=" + this.app.carrot.lang.Get_key_lang() + "&tl=en&text=" + s_txt + "&op=translate";
        Application.OpenURL(s_tr);
    }


    public void btn_clear_all_chat_log()
    {
        this.app.carrot.play_sound_click();
        this.clear_log_chat();
        this.obj_btn_clear_all_log.SetActive(false);
    }

    public void show_chat_log()
    {
        this.app.carrot.play_sound_click();
        this.app.command.panel_show_msg_chat.SetActive(false);
        this.app.command.panel_show_log_chat.SetActive(true);
        this.obj_btn_clear_all_log.SetActive(true);
    }

    public void btn_show_sub_menu()
    {
        this.app.command_dev.sub_menu(this.data_chat_cur);
    }

    public void act_start_speech()
    {
        this.is_tts = true;
    }

    public void act_done_speech()
    {
        this.is_tts = false;
    }

    public void Play_chat_by_ID(string s_id, string s_lang)
    {
        this.app.carrot.server.Get_doc_by_path("chat-" + s_lang, s_id, Act_play_chat_by_ID_done, Act_play_chat_by_ID_fail);
    }

    private void Act_play_chat_by_ID_done(string s_data)
    {
        Debug.Log("Play_chat_by_ID:" + s_data);
        Fire_Document fd = new(s_data);
        this.act_chat(fd.Get_IDictionary());
    }

    private void Act_play_chat_by_ID_fail(string s_error)
    {
        this.app.carrot.Show_msg("Chat Info", app.carrot.L("no_chat", "No related answers yet, please teach me!"), Carrot.Msg_Icon.Alert);
    }

    public void Set_ready_msg_tip()
    {
        this.is_ready_msg_tip = true;
    }

    public void Btn_export_data()
    {
        this.app.carrot.play_sound_click();
        this.app.file.Set_filter(Carrot_File_Data.JsonData);
        this.app.file.Save_file(paths =>
        {
            string s_path = paths[0];

            foreach (var obj in this.all_item)
            {
                if (obj is IDictionary item && item.Contains("index_item"))
                {
                    item.Remove("index_item");
                }
            }
            IDictionary data_export = Json.Deserialize("{}") as IDictionary;
            data_export["all_item"] = this.all_item;
            //data_export["cmd_offline"] = this.app.command_storage.get_cm_offline();
            FileHelper.WriteAllText(s_path, Json.Serialize(data_export));
            this.app.carrot.Show_msg("Export Success\n at:" + s_path);
        });
    }

    public void Btn_import_data()
    {
        this.app.carrot.play_sound_click();
        this.app.file.Set_filter(Carrot_File_Data.JsonData);
        this.app.file.Open_file(paths =>
        {
            string s_path = paths[0];
            string s_data = FileHelper.ReadAllText(s_path);
            load_all_item_chat(s_data);
            this.Update_data_file();
            this.app.carrot.Show_msg("Import Success!");
        });
    }

    public void Btn_show_list_command()
    {
        this.app.command_storage.show_list_cm("3");
    }

    public List<IDictionary> Get_list_random_cmd()
    {

        return GetRandomUniqueItems<IDictionary>(all_item, 20);
    }

    public List<IDictionary> Get_list_cmd_by_key(string s_key)
    {

        return GetFilteredKeyItems<IDictionary>(all_item, 20, s_key);
    }

    private List<T> GetRandomUniqueItems<T>(IList list, int count)
    {
        return list.Cast<T>().OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
    }

    public IDictionary Get_data_item_cmd_index(int index)
    {
        return this.all_item[index] as IDictionary;
    }

    private List<T> GetFilteredKeyItems<T>(IList list, int count, string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return new List<T>();

        string lowerKeyword = keyword.ToLower();

        var filteredList = list.Cast<IDictionary>()
            .Where(item =>
                item.Contains("key") &&
                item["key"] != null &&
                item["key"].ToString().ToLower().Contains(lowerKeyword)
            )
            .Cast<T>()
            .Take(count)
            .ToList();
        return filteredList;
    }

    public List<IDictionary> GetPendingCmd()
    {
        var filteredList = this.all_item.Cast<IDictionary>()
            .Where(item =>
                item.Contains("status") &&
                item["status"].ToString() == "pending"
            )
            .Cast<IDictionary>()
            .ToList();
        return filteredList;
    }

    public IDictionary GetCmdById(string id)
    {
        var item = this.all_item.Cast<IDictionary>()
            .FirstOrDefault(cmd =>
                cmd.Contains("id") &&
                cmd["id"] != null &&
                cmd["id"].ToString() == id
            );

        return item;
    }

    public void DeletePendingCmd()
    {
        var itemsToRemove = this.all_item.Cast<IDictionary>()
            .Where(item =>
                item.Contains("status") &&
                item["status"] != null &&
                item["status"].ToString() == "pending"
            ).ToList();

        foreach (var item in itemsToRemove)
        {
            this.all_item.Remove(item);
        }
        this.Update_data_file();
    }

    public void Set_data_item_cmd_index(int index, IDictionary data)
    {
        if (index < 0 || index >= this.all_item.Count) return;
        this.all_item[index] = data;
    }

    public void Delete_cmd_by_index(int index)
    {
        if (index < 0 || index >= this.all_item.Count) return;
        this.all_item.RemoveAt(index);
        Debug.Log("Delete command at index: " + index);
        this.Update_data_file();
    }

    private void Update_data_file()
    {
        string name_file_chat = "chat-" + this.app.carrot.lang.Get_key_lang() + ".json";
        IDictionary s_data = Json.Deserialize("{}") as IDictionary;
        s_data["all_item"] = this.all_item;
        this.app.carrot.get_tool().save_file(name_file_chat, Json.Serialize(s_data));
        this.Update_UI_info();
    }

    public void Add_cmd(IDictionary data_cmd)
    {
        this.all_item.Add(data_cmd);
        this.Update_data_file();
        this.Update_UI_info();
    }

    public void Btn_Update_cmd()
    {
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        this.app.carrot.Get_Data("https://raw.githubusercontent.com/kurotsmile/Database-Store-Json/refs/heads/main/" + this.file_name_data_chat, (s_data) =>
        {
            this.app.carrot.hide_loading();
            this.load_all_item_chat(s_data);
            this.app.carrot.get_tool().save_file(this.file_name_data_chat, s_data);
            this.app.carrot.Show_msg("Update Success", "The chat data has been successfully updated!");
        });
    }
}