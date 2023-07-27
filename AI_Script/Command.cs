using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject prefab_effect_icon_chat;

    public Transform area_body_log_command;
    public ScrollRect ScrollRect_log_command;
    public AudioSource sound_command;
    public string chat_pater="";
    private string s_hint= "Hint";
    private bool is_test_command = false;

    public Sprite icon_pc_music;
    public Sprite icon_info_chat;
    public Sprite sp_icon_info_add_chat;
    public Sprite sp_icon_info_share_chat;
    public Sprite sp_icon_info_report_chat;

    [Header("Tip command")]
    private List<string> list_tip;
    private float time_next_tip = 2f;
    private float count_timer_tip = 0;
    private bool is_act_tip = false;
    private int index_tip = 0;
    public Text txt_tip_chat;

    [Header("Effect show text chat")]
    public Text txt_show_chat;
    public Image img_border_color_chat;
    public GameObject panel_show_msg_chat;
    public GameObject panel_show_log_chat;
    public GameObject obj_btn_info_chat;
    private float count_timer_show_text = 0;
    private float count_timer_hide_text = 0;
    private bool is_show_text = false;
    private int index_text = 0;
    private string s_chat_temp;
    private bool is_hide_text = false;

    private Carrot.Carrot_Box box_list;
    private string s_command_chat_last;
    private string id_cur_chat;
    private IDictionary data_chat_cur;

    public void send_command()
    {
        if (this.inp_command.text.Trim() != "")
        {
            this.send_chat(this.inp_command.text,true);
            this.inp_command.text = "";
        }
    }

    public void send_chat(string s_key,bool is_log_show=false)
    {
        this.s_command_chat_last = s_key;
        if(is_log_show) add_item_log_chat(s_key);
        IDictionary chat_offline = this.app.command_storage.act_call_cm_offline(s_key);
        if (chat_offline != null)
        {
            this.id_cur_chat = chat_offline["id"].ToString();
            this.act_chat(chat_offline);
            Debug.Log("Chat offline");
        }
        else
        {
            this.play_chat(s_key);
        }
    }

    private void play_chat(string s_key)
    {
        Debug.Log("play_chat:" + s_key);
        Query ChatQuery = this.app.carrot.db.Collection("chat-" + this.app.setting.get_lang());
        ChatQuery=ChatQuery.WhereEqualTo("key", s_key);
        ChatQuery=ChatQuery.WhereEqualTo("sex_user", this.app.setting.get_user_sex());
        ChatQuery=ChatQuery.WhereEqualTo("sex_character", this.app.setting.get_character_sex());

        if (this.id_cur_chat != "") ChatQuery=ChatQuery.WhereEqualTo("pater", this.id_cur_chat);

        if (Random.Range(0, 2) == 0)
            ChatQuery.OrderBy("msg");
        else
            ChatQuery.OrderByDescending("msg");
        ChatQuery.Limit(1);
        ChatQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot capitalQuerySnapshot = task.Result;
            if (task.IsFaulted)
            {
                this.id_cur_chat = "";
                this.show_effect_txt_msg(task.Exception.Message);
                this.set_color(Color.red);
            }

            if (task.IsCompleted)
            {
                if (capitalQuerySnapshot.Count > 0)
                {
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        this.id_cur_chat = documentSnapshot.Id;
                        IDictionary c = documentSnapshot.ToDictionary();
                        c["id"] = this.id_cur_chat;
                        list_chat.Add(c);
                        this.app.command_storage.add_command_offline(c);
                    };

                    if (list_chat.Count > 1)
                    {
                        int index_random = Random.Range(0, list_chat.Count);
                        this.act_chat(list_chat[index_random], false);
                    }
                    else
                    {
                        this.act_chat(list_chat[0], false);
                    }
                }
                else
                {
                    this.id_cur_chat = "";
                    this.show_effect_txt_msg(PlayerPrefs.GetString("no_chat", "No related answers yet, please teach me!"));
                    this.set_color(Color.red);
                }
            }
        });
    }

    public void send_command_by_text(string s_text)
    {
        this.inp_command.text = s_text;
        this.send_command();
    }

    private void add_item_log_chat(string s_inp_command)
    {
        GameObject item_command_chat = Instantiate(this.prefab_item_command_chat);
        item_command_chat.transform.SetParent(this.area_body_log_command);
        item_command_chat.transform.localScale = new Vector3(1f, 1f, 1f);
        item_command_chat.transform.localPosition = new Vector3(item_command_chat.transform.localPosition.x, item_command_chat.transform.localPosition.y, 0f);
        item_command_chat.transform.localRotation = Quaternion.Euler(Vector3.zero);
        item_command_chat.GetComponent<Item_command_chat>().txt_chat.text = s_inp_command;
        item_command_chat.GetComponent<Item_command_chat>().txt_chat.color = this.GetComponent<App>().carrot.color_highlight;
        item_command_chat.GetComponent<Item_command_chat>().icon.sprite = this.GetComponent<App>().get_icon_sex_user();
        item_command_chat.GetComponent<Item_command_chat>().btn_add_app.GetComponent<Image>().color=this.GetComponent<App>().carrot.color_highlight;
        if(this.GetComponent<App>().carrot.model_app==Carrot.ModelApp.Develope) item_command_chat.GetComponent<Item_command_chat>().btn_add_web.SetActive(true);
        else item_command_chat.GetComponent<Item_command_chat>().btn_add_web.SetActive(false);
    }

    public void add_item_pc_chat(string s_txt_show,Sprite icon,bool is_music=false, IDictionary i_data_chat=null)
    {
        GameObject item_command_chat = Instantiate(this.prefab_item_command_pc);
        item_command_chat.transform.SetParent(this.area_body_log_command);
        item_command_chat.transform.localPosition = new Vector3(item_command_chat.transform.localPosition.x, item_command_chat.transform.localPosition.y, 0f);
        item_command_chat.transform.localScale = new Vector3(1f, 1f, 1f);
        item_command_chat.transform.localRotation = Quaternion.Euler(Vector3.zero);
        item_command_chat.GetComponent<Item_command_chat>().txt_chat.text = s_txt_show;
        item_command_chat.GetComponent<Item_command_chat>().icon.sprite = icon;
        item_command_chat.GetComponent<Item_command_chat>().is_music = is_music;
        item_command_chat.GetComponent<Item_command_chat>().btn_add_app.GetComponent<Image>().color=this.GetComponent<App>().carrot.color_highlight;
        if(i_data_chat!=null) item_command_chat.GetComponent<Item_command_chat>().idata_chat = i_data_chat;
        if (this.is_test_command) item_command_chat.GetComponent<Item_command_chat>().btn_add_app.SetActive(false);
        this.GetComponent<App>().set_item_cur_log_chat(item_command_chat.GetComponent<Item_command_chat>());
        this.ScrollRect_log_command.horizontalNormalizedPosition = 1f;
        Canvas.ForceUpdateCanvases();
    }

    public void add_item_log_music(string s_name, IDictionary id_data)
    {
        this.add_item_pc_chat(s_name, this.icon_pc_music, true, id_data);
    }

    public void act_chat(IDictionary data_chat,bool is_test=false)
    {
        this.data_chat_cur = data_chat;
        this.app.panel_main.SetActive(true);
        this.is_test_command = is_test;
        if (is_test) this.obj_btn_info_chat.SetActive(false); else this.obj_btn_info_chat.SetActive(true);

        string s_msg_chat = "";
        if (data_chat["msg"] != null)
        {
            s_msg_chat = data_chat["msg"].ToString();
            s_msg_chat = this.app.s_rep_key(s_msg_chat);
            this.show_effect_txt_msg(s_msg_chat);
            this.play_text_audio(s_msg_chat);
        }

        if (data_chat["action"]!= null) this.play_act(data_chat["action"].ToString());
        if (data_chat["face"]!= null) this.act_cm_face(data_chat["face"].ToString());
        if (data_chat["color"]!= null) this.set_color_by_string(data_chat["color"].ToString());
        if (data_chat["mp3"] != null)if (this.app.setting.get_status_sound_voice()) StartCoroutine(get_audio_chat_form_url(data_chat["mp3"].ToString()));
        if (data_chat["link"]!= null) if(data_chat["link"].ToString().Trim()!="") Application.OpenURL(data_chat["link"].ToString());
        if (data_chat["func"] != null)
        {
            string index_func = data_chat["func"].ToString();
            if (index_func == "1") this.app.sel_menu_with_sound(2);
        }
        if (data_chat["icon"]!= null)
        {
            if (data_chat["icon"].ToString() != "")
            {
                string s_id_icon = data_chat["icon"].ToString();
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

        this.add_item_pc_chat(s_msg_chat, this.app.get_character().icon_sex, false, data_chat);
        this.app.carrot.ads.show_ads_Interstitial();
    }

    public void act_cm_face(string s_face)
    {
        if(s_face!="") this.play_face(int.Parse(s_face));
    }

    public void play_act(string number_animation)
    {
        this.app.get_character().play_ani(int.Parse(number_animation));
    }

    public void play_face(int index)
    {
        this.GetComponent<App>().get_character().play_ani_face(index);
    }

    private void action_waitting()
    {
        this.GetComponent<App>().get_character().play_ani_waitting();
    }

    public void load_chat_tip()
    {
        this.s_hint = PlayerPrefs.GetString("hint", "Hint");
        this.list_tip = this.GetComponent<Command_storage>().get_list_key_tip();
        if(this.list_tip.Count>0) this.is_act_tip = true;
    }

    void Update()
    {
        if (this.is_act_tip)
        {
            this.count_timer_tip = this.count_timer_tip + Time.deltaTime;
            if (this.count_timer_tip > this.time_next_tip)
            {
                this.count_timer_tip = 0;
                this.index_tip++;
                if (this.index_tip >= this.list_tip.Count) this.index_tip = 0;
                this.txt_tip_chat.text = this.s_hint+" : "+this.list_tip[this.index_tip].ToString();
            }
        }

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
                this.action_waitting();
            }
        }
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
            if(this.is_test_command==false) this.GetComponent<App>().show_chat_function();
        }
    }

    public void play_text_audio(string s_chat)
    {
        StartCoroutine(get_audio_chat_form_txt(s_chat, this.app.setting.get_lang()));
    }

    public IEnumerator get_audio_chat_form_txt(string txt_chat, string s_lang)
    {
        string s_url_audio = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=" + txt_chat.Length + "&client=tw-ob&q=" + txt_chat + "&tl=" + s_lang;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log(www.error);
            else
                this.play_sound_chat(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    public IEnumerator get_audio_chat_form_url(string s_url_audio)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG))
        {
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
    }

    private void play_sound_chat(AudioClip audio_clip) {
        this.sound_command.clip = audio_clip;
        this.sound_command.pitch = this.GetComponent<App>().setting.get_voice_speed();
        this.sound_command.Play();
        this.GetComponent<App>().waitting_command();
    }

    public void get_msg_hit()
    {
        this.send_chat("hit",true);
    }

    private void get_effect_icon_chat(string s_id_icon)
    {
        DocumentReference iconRef=this.app.carrot.db.Collection("icon").Document(s_id_icon);
        iconRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot collectionIcon = task.Result;
                IDictionary icon_data = collectionIcon.ToDictionary();
                this.app.carrot.get_img_and_save_playerPrefs(icon_data["icon"].ToString(), null, s_id_icon, act_play_effect_icon_chat);
            }
        });
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

    public void clear_log_chat(){
        this.GetComponent<App>().carrot.clear_contain(this.area_body_log_command);
    }

    public void show_box_link_share_chat(string s_link_share)
    {
        this.app.carrot.show_share(s_link_share, PlayerPrefs.GetString("share_chat_tip", "You can share and show this dialogue to others if they also have the app installed"));
    }

    private void show_box_learn_chat(string s_data)
    {
        IDictionary data_learn = (IDictionary)Carrot.Json.Deserialize(s_data);
        this.app.command_storage.show_add_command_with_pater(data_learn["msg"].ToString(), data_learn["id"].ToString());
        if (this.box_list != null) this.box_list.close();
    }

    private void show_box_report_chat()
    {
        this.app.show_report();
        if (this.box_list != null) this.box_list.close();
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

        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.Create_Box("info_chat");
        this.box_list.set_title(PlayerPrefs.GetString("command_pass", "Published chat"));
        this.box_list.set_icon(this.icon_info_chat);

        foreach(var key in this.data_chat_cur.Keys){
            if (this.data_chat_cur[key] != null)
            {
                if (key.ToString()== "user")
                {
                    IDictionary data_user = (IDictionary) this.data_chat_cur[key];
                    string user_id = data_user["id"].ToString();
                    string user_lang = data_user["lang"].ToString();
                    Carrot.Carrot_Box_Item item_user = this.box_list.create_item("item_" + key.ToString());
                    item_user.set_title(PlayerPrefs.GetString("chat_creator","Creator"));
                    item_user.set_icon(this.icon_info_chat);
                    item_user.set_tip(data_user["name"].ToString());

                    if (data_user["avatar"] != null)
                    {
                        Sprite sp_avatar = this.app.carrot.get_tool().get_sprite_to_playerPrefs("avatar_user_" + data_user["id"]);
                        if (sp_avatar != null) item_user.set_icon_white(sp_avatar);
                        else this.app.carrot.get_img_and_save_playerPrefs(data_user["avatar"].ToString(),item_user.img_icon, "avatar_user_" + data_user["id"]);
                    }

                    item_user.set_act(()=>this.app.carrot.user.show_user_by_id(user_id,user_lang));
                }
                else
                {
                    string s_data_val = this.data_chat_cur[key].ToString();
                    string s_key = key.ToString();
                    if (s_data_val != "")
                    {
                        string s_field_title="";
                        string s_field_val="";

                        if (s_key == "sex_user")
                        {
                            s_field_title=PlayerPrefs.GetString("user_sex", "User Sex");
                            if (s_data_val == "0") s_field_val = PlayerPrefs.GetString("user_sex_boy", "Male");
                            else s_field_val=PlayerPrefs.GetString("user_sex_girl", "Female");
                        }
                        else if(s_key== "sex_character")
                        {
                            s_field_title=PlayerPrefs.GetString("setting_char_sex", "Character Sex");
                            if (s_data_val=="0") s_field_val = PlayerPrefs.GetString("user_sex_boy", "Male");
                            else s_field_val=PlayerPrefs.GetString("user_sex_girl", "Female");
                        }
                        else if(s_key== "face")
                        {
                            s_field_title = PlayerPrefs.GetString("face", "Face");
                            s_field_val = PlayerPrefs.GetString("face", "Face") + " " + s_data_val;
                        }
                        else if(s_key == "action")
                        {
                            s_field_title = PlayerPrefs.GetString("act", "Action");
                            s_field_val = PlayerPrefs.GetString("act", "Action") + " " + s_data_val;
                        }
                        else
                        {
                            s_field_title=s_key;
                            s_field_val=s_data_val;
                        }

                        Carrot.Carrot_Box_Item item_field = this.box_list.create_item("item_" + key.ToString());
                        item_field.set_icon(this.icon_info_chat);
                        item_field.set_title(s_field_title);
                        item_field.set_tip(s_field_val);
                    }
                }

            }
        }
    }
}
