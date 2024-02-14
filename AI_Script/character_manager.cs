using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class character_manager : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    [Header("Obj Other")]
    public string name_sex;
    public Sprite icon_sex;
    public GameObject[] obj_character_phone;

    public string character_name;
    private int sel_character;
    public string[] arr_name_ani_waitting;
    public string[] arr_name_ani_stop_dance;
    public string[] arr_name_ani_hit;
    public string[] arr_name_ani_speak;
    public string[] arr_name_ani_face;
    private GameObject obj_npc;

    public GameObject prefab_character_item;
    public Transform body_all_item_fashion_style;
    public Transform body_all_item_costumes_style;
    public Transform body_all_item_head_style;
    public GameObject panel_costumes;
    public GameObject panel_head;
    private string sel_costumes_style;
    private string sel_head_style;

    private List<Item_character> list_character = null;
    private List<Item_character> list_costumes = null;
    private List<Item_character> list_head = null;

    private string s_data_json_head_offline;
    private string s_data_json_costumes_offline;

    public void load_character()
    {
        this.s_data_json_costumes_offline = PlayerPrefs.GetString("s_data_json_costumes_offline");
        this.s_data_json_head_offline = PlayerPrefs.GetString("s_data_json_head_offline");

        if (this.app.carrot.is_offline())
        {
            this.app.s_data_json_costumes_temp = this.s_data_json_costumes_offline;
            this.app.s_data_json_head_temp = this.s_data_json_head_offline;
        }

        string s_name_lang = PlayerPrefs.GetString("name_char_" + this.name_sex);
        string s_name_customer = PlayerPrefs.GetString("character_name_" + this.name_sex, "");
        if (s_name_customer != "")
            this.character_name = PlayerPrefs.GetString("character_name_" + this.name_sex);
        else
            this.character_name = s_name_lang;
        this.sel_character = PlayerPrefs.GetInt("sel_character_" + this.name_sex, 0);
        this.sel_costumes_style = PlayerPrefs.GetString("sel_costumes_style" + this.name_sex);
        this.sel_head_style = PlayerPrefs.GetString("sel_head_style" + this.name_sex);

        this.app.carrot.clear_contain(this.body_all_item_fashion_style);
        this.app.carrot.clear_contain(this.body_all_item_costumes_style);
        this.app.carrot.clear_contain(this.body_all_item_head_style);

        this.list_character = new List<Item_character>();
        for (int i = 0; i < this.obj_character_phone.Length; i++)
        {
            var index_character = i;
            Item_character item_c = create_item(this.body_all_item_fashion_style);
            item_c.s_id = i.ToString();
            item_c.is_free = this.obj_character_phone[i].GetComponent<character_obj>().is_free;

            item_c.set_icon(this.obj_character_phone[i].GetComponent<character_obj>().icon);

            if (item_c.is_free) item_c.btn_buy.SetActive(false);
            if (this.sel_character == i) item_c.GetComponent<Image>().color = this.app.carrot.color_highlight;

            if (!item_c.is_free)
            {
                if (this.app.setting.check_buy_product(6)) item_c.is_free = true;
                if (this.app.setting.check_buy_product(0)) item_c.is_free = true;
            }

            if (item_c.is_free)
            {
                item_c.btn_buy.SetActive(false);
                item_c.set_act(() => this.choise_character(index_character));
            }
            else
            {
                item_c.btn_buy.SetActive(true);
                item_c.set_act(() => this.app.carrot.buy_product(6));
            }
            this.list_character.Add(item_c);

        }

        this.select_character(this.sel_character);

        if (this.sel_costumes_style != "")
        {
            Texture2D texCostumes = this.app.carrot.get_tool().get_texture2D_to_playerPrefs(this.get_npc().s_type_costumes + "_" + this.sel_costumes_style);
            if (texCostumes != null) this.get_npc().set_skinned_costumes(texCostumes);
        }

        if (this.sel_head_style != "")
        {
            Texture2D tex2d_head = this.app.carrot.get_tool().get_texture2D_to_playerPrefs(this.get_npc().s_type_head + "_" + this.sel_head_style);
            if (tex2d_head != null) this.get_npc().set_skinned_head(tex2d_head);
        }
    }

    public string get_name_character() {
        String s_name_lang = PlayerPrefs.GetString("name_char_" + this.name_sex);
        string s_name_customer = PlayerPrefs.GetString("character_name_" + this.name_sex, "");
        if (s_name_customer != "")
            this.character_name = PlayerPrefs.GetString("character_name_" + this.name_sex);
        else
            this.character_name = s_name_lang;
        return this.character_name;
    }

    public void set_character_name(string s_new_name)
    {
        this.character_name = s_new_name;
        PlayerPrefs.SetString("character_name_" + this.name_sex, s_new_name);
    }

    private void select_character(int sel_index)
    {
        this.app.carrot.clear_contain(this.transform);
        this.obj_npc = Instantiate(this.obj_character_phone[sel_index]);
        this.obj_npc.transform.SetParent(this.transform);
        this.obj_npc.transform.localPosition = Vector3.zero;
        this.obj_npc.gameObject.SetActive(true);
    }

    public void choise_character(int sel_index)
    {
        this.reset_item_ui_list(this.list_character);
        this.list_character[sel_index].set_color_bk(this.app.carrot.color_highlight);
        this.sel_character = sel_index;
        this.select_character(this.sel_character);
        PlayerPrefs.SetInt("sel_character_" + this.name_sex, this.sel_character);
        if (this.app.s_data_json_costumes_temp != "")
            this.load_costumes_by_style_character_query();
        else
            this.load_costumes_by_style_character_s_data(this.sel_costumes_style);

    }

    public void choise_costumes(string s_id_costumes)
    {
        Texture2D tex2d_costumes = this.app.carrot.get_tool().get_texture2D_to_playerPrefs(this.get_npc().s_type_costumes + "_" + s_id_costumes);
        if (tex2d_costumes != null)
        {
            this.sel_costumes_style = s_id_costumes;
            PlayerPrefs.SetString("sel_costumes_style" + this.name_sex, this.sel_costumes_style);
            this.get_npc().set_skinned_costumes(tex2d_costumes);
        }
    }

    public void choise_head(string s_id_head)
    {
        Texture2D tex2d_head = this.app.carrot.get_tool().get_texture2D_to_playerPrefs(this.get_npc().s_type_head + "_" + this.sel_head_style);
        if (tex2d_head != null)
        {
            this.sel_head_style = s_id_head;
            PlayerPrefs.SetString("sel_head_style" + this.name_sex, this.sel_head_style);
            this.get_npc().set_skinned_head(tex2d_head);
        }
    }

    public Animator get_anim_character()
    {
        return this.obj_npc.GetComponent<Animator>();
    }

    public void play_ani_hit(int index_hit)
    {
        this.get_anim_character().Play(this.arr_name_ani_hit[index_hit], 0);
    }

    public int get_length_ani_hit()
    {
        return this.arr_name_ani_hit.Length;
    }

    public character_obj get_npc()
    {
        return this.obj_npc.GetComponent<character_obj>();
    }

    public void next_character()
    {
        this.sel_character++;
        if (this.sel_character >= this.obj_character_phone.Length) this.sel_character = 0;
        this.select_character(this.sel_character);
        PlayerPrefs.SetInt("sel_character_" + this.name_sex, this.sel_character);
    }

    public void prev_character()
    {
        this.sel_character--;
        if (this.sel_character <= -1) this.sel_character = this.obj_character_phone.Length - 1;
        this.select_character(this.sel_character);
        PlayerPrefs.SetInt("sel_character_" + this.name_sex, this.sel_character);
    }


    public void play_speak()
    {
        int index_ani_speak = UnityEngine.Random.Range(0, this.arr_name_ani_speak.Length);
        this.get_anim_character().Play(this.arr_name_ani_speak[index_ani_speak], 1);
    }

    public void play_ani(string s_name_anim)
    {
        this.unpause_ani();
        this.get_anim_character().Play(s_name_anim, 0);
    }

    public void play_ani_face(int index)
    {
        this.get_anim_character().SetTrigger(this.arr_name_ani_face[index]);
    }

    public void play_ani_waitting()
    {
        this.unpause_ani();
        int rand_act = UnityEngine.Random.Range(0, this.arr_name_ani_waitting.Length);
        if (this.get_npc().gameObject.activeInHierarchy) this.get_anim_character().Play(this.arr_name_ani_waitting[rand_act], 0);
    }

    public void play_ani_dance()
    {
        this.unpause_ani();
        this.app.action.play_animation_dance();
    }

    public void play_ani_stop_dance()
    {
        this.unpause_ani();
        int rand_act = UnityEngine.Random.Range(0, this.arr_name_ani_stop_dance.Length);
        this.get_anim_character().Play(this.arr_name_ani_stop_dance[rand_act], 0);
    }

    public void pause_ani()
    {
        this.get_anim_character().enabled = false;
    }

    public void unpause_ani()
    {
        this.get_anim_character().enabled = true;
    }

    public void load_costumes_by_style_character_query()
    {
        this.app.carrot.clear_contain(this.body_all_item_costumes_style);
        this.panel_costumes.SetActive(false);
        this.panel_head.SetActive(false);

        Query SkinQuery = this.app.carrot.db.Collection("character_fashion").WhereEqualTo("type", this.get_npc().s_type_costumes);
        SkinQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot SkinQuerySnapshot = task.Result;
            if (task.IsFaulted)
            {
                this.panel_costumes.SetActive(false);
                this.load_head_by_style_character();
            }

            if (task.IsCompleted)
            {
                if (SkinQuerySnapshot.Count > 0)
                {
                    this.list_costumes = new List<Item_character>();
                    this.panel_costumes.SetActive(true);

                    List<IDictionary> list_costumes_data = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in SkinQuerySnapshot.Documents)
                    {
                        IDictionary data_costumes = documentSnapshot.ToDictionary();
                        data_costumes["id"] = documentSnapshot.Id;
                        this.add_item_to_list_costumes(data_costumes);
                        list_costumes_data.Add(data_costumes);
                    };
                    this.s_data_json_costumes_offline = Carrot.Json.Serialize(list_costumes_data);
                    this.app.s_data_json_costumes_temp = this.s_data_json_costumes_offline;
                    PlayerPrefs.SetString("s_data_json_costumes_offline", this.s_data_json_costumes_offline);
                }
                this.load_head_by_style_character();
            }
        });
    }

    private void load_head_by_style_character()
    {
        if (this.app.s_data_json_head_temp == "")
            this.load_head_by_style_character_query();
        else
            this.load_head_by_style_character_s_data(this.app.s_data_json_head_temp);
    }

    public void load_costumes_by_style_character_s_data(string s_data)
    {
        IList list_costumes = (IList)Carrot.Json.Deserialize(s_data);
        this.app.carrot.clear_contain(this.body_all_item_costumes_style);
        this.panel_costumes.SetActive(false);
        this.panel_head.SetActive(false);

        if (list_costumes.Count > 0)
        {
            this.list_costumes = new List<Item_character>();
            this.panel_costumes.SetActive(true);

            for (int i = 0; i < list_costumes.Count; i++)
            {
                IDictionary data_costumes = (IDictionary)list_costumes[i];
                this.add_item_to_list_costumes(data_costumes);
            };
        }
        this.load_head_by_style_character();
    }

    private void add_item_to_list_costumes(IDictionary data_costumes)
    {
        Item_character item_costumes = this.create_item(this.body_all_item_costumes_style);
        string s_id_skin = data_costumes["id"].ToString();
        if (s_id_skin == this.sel_costumes_style) item_costumes.set_color_bk(this.app.carrot.color_highlight);

        if (data_costumes["buy"] != null)
        {
            string s_buy = data_costumes["buy"].ToString();
            if (s_buy == "1") item_costumes.is_free = false;
            else item_costumes.is_free = true;
        }
        else
        {
            item_costumes.is_free = true;
        }

        if (!item_costumes.is_free)
        {
            if (this.app.setting.check_buy_product(5)) item_costumes.is_free = true;
            if (this.app.setting.check_buy_product(6)) item_costumes.is_free = true;
            if (this.app.setting.check_buy_product(0)) item_costumes.is_free = true;
        }

        Sprite icon_costumes = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_skin);
        if (icon_costumes != null)
        {
            item_costumes.set_icon(icon_costumes);
        }
        else
        {
            if (data_costumes["icon"] != null)
            {
                if (data_costumes["icon"].ToString() != "") this.app.carrot.get_img_and_save_playerPrefs(data_costumes["icon"].ToString(), item_costumes.icon, s_id_skin);
            }
        }

        string s_img = "";
        if (data_costumes["img"] != null) s_img = data_costumes["img"].ToString();

        if (item_costumes.is_free == true)
        {
            item_costumes.btn_buy.SetActive(false);
            if (s_img != "") item_costumes.set_act(() => this.act_get_img_costumes(item_costumes, s_id_skin, s_img));
        }
        else
        {
            item_costumes.btn_buy.SetActive(true);
            item_costumes.set_act(() => this.app.carrot.buy_product(5));
        }
        this.list_costumes.Add(item_costumes);
    }

    private void load_head_by_style_character_query()
    {
        this.app.carrot.clear_contain(this.body_all_item_head_style);
        this.panel_head.SetActive(false);

        Query SkinQuery = this.app.carrot.db.Collection("character_fashion").WhereEqualTo("type", this.get_npc().s_type_head);
        SkinQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot HeadQuerySnapshot = task.Result;
            if (task.IsFaulted) this.panel_head.SetActive(false);

            if (task.IsCompleted)
            {
                if (HeadQuerySnapshot.Count > 0)
                {
                    List<IDictionary> list_head_data = new List<IDictionary>();

                    this.panel_head.SetActive(true);
                    this.list_head = new List<Item_character>();
                    foreach (DocumentSnapshot documentSnapshot in HeadQuerySnapshot.Documents)
                    {
                        IDictionary data_head = documentSnapshot.ToDictionary();
                        data_head["id"] = documentSnapshot.Id;
                        this.add_item_to_list_head(data_head);
                        list_head_data.Add(data_head);
                    };

                    this.s_data_json_head_offline = Carrot.Json.Serialize(list_head_data);
                    this.app.s_data_json_head_temp = this.s_data_json_head_offline;
                    PlayerPrefs.SetString("s_data_json_head_offline", this.s_data_json_head_offline);
                }
            }
        });
    }

    private void load_head_by_style_character_s_data(string s_data)
    {
        this.app.carrot.clear_contain(this.body_all_item_head_style);
        this.panel_head.SetActive(false);

        IList list_head = (IList) Carrot.Json.Deserialize(s_data);
        if (list_head.Count > 0)
        {
            this.panel_head.SetActive(true);
            for (int i = 0; i < list_head.Count; i++)
            {
                IDictionary data_head = (IDictionary)list_head[i];
                this.add_item_to_list_head(data_head);
            }
        }
    } 

    private void add_item_to_list_head(IDictionary data_head)
    {
        string s_id_head = data_head["id"].ToString();
        Item_character item_head = this.create_item(this.body_all_item_head_style);

        if (s_id_head == this.sel_head_style) item_head.set_color_bk(this.app.carrot.color_highlight);

        if (data_head["buy"] != null)
        {
            string s_buy = data_head["buy"].ToString();
            if (s_buy == "1") item_head.is_free = false;
            else item_head.is_free = true;
        }
        else
        {
            item_head.is_free = true;
        }

        if (!item_head.is_free)
        {
            if (this.app.setting.check_buy_product(3)) item_head.is_free = true;
            if (this.app.setting.check_buy_product(6)) item_head.is_free = true;
            if (this.app.setting.check_buy_product(0)) item_head.is_free = true;
        }

        Sprite icon_costumes = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_head);
        if (icon_costumes != null)
        {
            item_head.set_icon(icon_costumes);
        }
        else
        {
            if (data_head["icon"] != null)
            {
                if (data_head["icon"].ToString() != "") this.app.carrot.get_img_and_save_playerPrefs(data_head["icon"].ToString(), item_head.icon, s_id_head);
            }
        }

        string s_img = "";
        if (data_head["img"] != null) s_img = data_head["img"].ToString();

        if (item_head.is_free == true)
        {
            item_head.btn_buy.SetActive(false);
            item_head.set_act(() => act_get_img_head(item_head, s_id_head, s_img));
        }
        else
        {
            item_head.btn_buy.SetActive(true);
            item_head.set_act(() => this.app.carrot.buy_product(3));
        }
        this.list_head.Add(item_head);
    }

    private void act_get_img_costumes(Item_character item_change ,string s_id_costumes, string s_url_img)
    {
        this.reset_item_ui_list(this.list_costumes);
        item_change.set_color_bk(this.app.carrot.color_highlight);
        this.app.carrot.show_loading(get_img_costumes_url(s_id_costumes, s_url_img));
    }

    private IEnumerator get_img_costumes_url(string s_id_costumes, string s_url_img)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(s_url_img))
        {
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                this.sel_costumes_style = s_id_costumes;
                PlayerPrefs.SetString("sel_costumes_style" + this.name_sex, this.sel_costumes_style);
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                this.get_npc().set_skinned_costumes(tex);
                this.app.carrot.hide_loading();
                this.app.carrot.get_tool().PlayerPrefs_Save_texture2D(this.get_npc().s_type_costumes + "_" + this.sel_costumes_style, tex);
            }
        }
    }

    private void act_get_img_head(Item_character item_change, string s_id_header, string s_url_img)
    {
        this.reset_item_ui_list(this.list_head);
        item_change.set_color_bk(this.app.carrot.color_highlight);
        this.app.carrot.show_loading(get_img_head_url(s_id_header, s_url_img));
    }

    private IEnumerator get_img_head_url(string id_head, string s_url_img)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(s_url_img))
        {
            www.SendWebRequest();
            while (!www.isDone) yield return null;

            if (www.result == UnityWebRequest.Result.Success)
            {
                this.sel_head_style = id_head;
                PlayerPrefs.SetString("sel_head_style" + this.name_sex, this.sel_head_style);
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                this.get_npc().set_skinned_head(tex);
                this.app.carrot.hide_loading();
                this.app.carrot.get_tool().PlayerPrefs_Save_texture2D(this.get_npc().s_type_head + "_" + this.sel_head_style, tex);
            }
        }
    }

    private Item_character create_item(Transform tr_father)
    {
        GameObject item_c = Instantiate(this.prefab_character_item);
        item_c.transform.SetParent(tr_father);
        item_c.transform.localScale = new Vector3(1f, 1f, 1f);
        item_c.transform.localPosition = new Vector3(item_c.transform.localPosition.x, item_c.transform.localPosition.y, 0f);
        item_c.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Item_character c_obj = item_c.GetComponent<Item_character>();
        return c_obj;
    }

    public void check_buy_success_character()
    {
        this.reload_ui_character();
    }

    private void reset_item_ui_list(List<Item_character> list_item)
    {
        for(int i = 0; i < list_item.Count; i++) list_item[i].set_color_bk(Color.white);
    }

    public void reload_ui_character()
    {
        this.load_character();
        if (this.app.s_data_json_costumes_temp == "")
            this.load_costumes_by_style_character_query();
        else
            this.load_costumes_by_style_character_s_data(this.app.s_data_json_costumes_temp);
    }
}
