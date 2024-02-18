using Carrot;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering.LookDev;
using System.Xml.Linq;

public class Icon : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;
    public int index_buy_category_icon;

    [Header("Icon Obj")]
    public Sprite sp_icon_icons;
    private IList list_icon_name;
    private Carrot.Carrot_Box box_list_icon;
    private Carrot.Carrot_Box box_list_icon_category;
    private Carrot_Window_Input box_search_icon;

    private Carrot_Box_Item item_icon = null;

    private string s_id_category_buy_temp = "";
    private string s_data_cache="";
    private string s_data_json_icon_offline = "";
    private string s_data_json_icon_category_offline = "";

    public void On_load()
    {
        this.s_data_cache=PlayerPrefs.GetString("s_data_icon_temp","");
        if (this.s_data_cache != "") 
            this.list_icon_name = (IList)Json.Deserialize(this.s_data_cache);
        else 
            this.list_icon_name = (IList)Json.Deserialize("[]");

        if (this.app.carrot.is_offline())
        {
            this.s_data_json_icon_offline = PlayerPrefs.GetString("s_data_json_icon_offline");
            this.s_data_json_icon_category_offline = PlayerPrefs.GetString("s_data_json_icon_category_offline");
        }
    }

    private void List_category_icon()
    {
        if (this.s_data_json_icon_category_offline == "")
        {
            this.app.carrot.show_loading();
            StructuredQuery q = new("icon_category");
            q.Set_limit(20);
            this.app.carrot.server.Get_doc(q.ToJson(), Act_list_category_icon_done, Act_list_category_icon_fail);
        }
        else
        {
            this.Act_load_list_category_icon(this.s_data_json_icon_category_offline);
        }
    }

    private void Act_list_category_icon_done(string s_data)
    {
        this.s_data_json_icon_category_offline = s_data;
        PlayerPrefs.SetString("s_data_json_icon_category_offline",s_data);
        this.Act_load_list_category_icon(s_data);
    }

    private void Act_list_category_icon_fail(string s_error)
    {
        this.app.carrot.hide_loading();
    }

    private void Act_load_list_category_icon(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);

        if (!fc.is_null)
        {
            if (this.box_list_icon_category != null) this.box_list_icon_category.close();
            this.box_list_icon_category = this.app.carrot.Create_Box();
            this.box_list_icon_category.set_icon(this.app.carrot.icon_carrot_all_category);
            this.box_list_icon_category.set_title("Bundle of object styles");

            this.Head_btn(this.box_list_icon_category);

            for (int i = 0; i < fc.fire_document.Length; i++)
            {
                string s_status_buy = "free";
                IDictionary icon_data = fc.fire_document[i].Get_IDictionary();
                Carrot_Box_Item item_cat = this.box_list_icon_category.create_item("item_icon");
                item_cat.set_icon(this.sp_icon_icons);

                if (icon_data["buy"] != null) s_status_buy = icon_data["buy"].ToString();

                if (icon_data["key"] != null)
                {
                    string s_key_cat = icon_data["key"].ToString();
                    item_cat.set_title(s_key_cat);
                    item_cat.set_tip(s_key_cat);

                    if (s_status_buy != "free")
                    {
                        if (PlayerPrefs.GetInt("is_buy_category_icon_" + s_key_cat, 0) == 1) s_status_buy = "free";

                        if (s_status_buy != "free")
                        {
                            if (PlayerPrefs.GetInt("is_buy_0", 0) == 1) s_status_buy = "free";
                        }
                    }

                    if (s_status_buy != "free")
                    {
                        Carrot_Box_Btn_Item btn_buy = item_cat.create_item();
                        btn_buy.set_icon(this.app.setting.sp_icon_buy);
                        btn_buy.set_color(this.app.carrot.color_highlight);
                        Destroy(btn_buy.GetComponent<Button>());

                        if (this.app.carrot.model_app == ModelApp.Publish)
                            item_cat.set_act(() => this.Act_buy_category(s_key_cat));
                        else
                            item_cat.set_act(() => this.View_list_icon_by_category_key(s_key_cat));
                    }
                    else
                    {
                        item_cat.set_act(() => this.View_list_icon_by_category_key(s_key_cat));
                    }
                }
            };
            this.box_list_icon_category.update_color_table_row(); ;
        }
    }


    private void Act_buy_category(string s_id_category)
    {
        this.s_id_category_buy_temp = s_id_category;
        this.app.carrot.shop.buy_product(this.index_buy_category_icon);
    }

    public void Act_buy_category_success()
    {
        if (this.s_id_category_buy_temp != "")
        {
            this.View_list_icon_by_category_key(this.s_id_category_buy_temp);
            PlayerPrefs.SetInt("is_buy_category_icon_" + this.s_id_category_buy_temp, 1);
            this.s_id_category_buy_temp="";
        }
    }

    private void View_list_icon_by_category_key(string s_key)
    {
        this.app.carrot.show_loading();
        StructuredQuery q = new("icon");
        q.Add_where("category", Query_OP.EQUAL, s_key);
        this.app.carrot.server.Get_doc(q.ToJson(), Act_view_list_icon_by_category_key_done);
    }

    private void Act_view_list_icon_by_category_key_done(string s_data)
    {
        this.app.carrot.hide_loading();
        this.Act_load_icon_and_emoji(s_data);
    }

    public void Set_icon_and_emoji(Sprite sp_cm, string s_color, string s_id)
    {
        this.app.command_storage.set_s_color(s_color);
        this.app.command_storage.set_s_id_icon(s_id);
        this.item_icon.set_val(s_id);
        this.item_icon.set_icon_white(sp_cm);

        ColorUtility.TryParseHtmlString(s_color, out Color color_item);

        this.item_icon.txt_val.color = color_item;
        if (this.box_list_icon != null) this.box_list_icon.close();
        if (this.box_list_icon_category != null) this.box_list_icon_category.close();
    }

    public void Btn_show_list_emoji_and_color(Carrot_Box_Item item_icon_change)
    {
        this.item_icon=item_icon_change;
        this.app.carrot.show_loading();
        if (this.s_data_json_icon_offline =="")
        {
            StructuredQuery q = new("icon");
            q.Set_limit(50);
            this.app.carrot.server.Get_doc(q.ToJson(), Act_show_list_emoji_and_color_done);
        }
        else
        {
            this.Act_load_icon_and_emoji(this.s_data_json_icon_offline);
        }
    }

    private void Act_show_list_emoji_and_color_done(string s_data)
    {
        this.s_data_json_icon_offline = s_data;
        this.Act_load_icon_and_emoji(s_data);
    }

    private void Act_load_icon_and_emoji(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (this.box_list_icon != null) this.box_list_icon.close();
        this.box_list_icon = this.app.carrot.Create_Box();
        this.box_list_icon.set_icon(this.sp_icon_icons);
        this.box_list_icon.set_title(PlayerPrefs.GetString("setting_bubble_icon", "Icons and colors"));
        Carrot_Box_Btn_Item btn_icon_category = this.box_list_icon.create_btn_menu_header(this.app.carrot.icon_carrot_all_category);
        btn_icon_category.set_act(() => List_category_icon());

        this.Head_btn(this.box_list_icon);

        for(int i=0;i<fc.fire_document.Length;i++)
        {
            string s_color = "#FFFFFF";
            IDictionary icon_data = fc.fire_document[i].Get_IDictionary();
            Carrot_Box_Item item_icon = this.box_list_icon.create_item();
            item_icon.set_title(icon_data["id"].ToString());
            item_icon.set_tip(icon_data["icon"].ToString());
            this.Add_icon_to_list(icon_data["id"].ToString());
            if (icon_data["color"] != null)
            {
                s_color = icon_data["color"].ToString();
                ColorUtility.TryParseHtmlString(icon_data["color"].ToString(), out Color color_item);
                item_icon.set_tip(s_color);
                item_icon.txt_tip.color = color_item;
            }

            Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs(icon_data["id"].ToString());
            if (sp_icon != null)
                item_icon.set_icon_white(sp_icon);
            else
                if (icon_data["icon"] != null) this.app.carrot.get_img_and_save_playerPrefs(icon_data["icon"].ToString(), item_icon.img_icon, icon_data["id"].ToString());

            item_icon.set_act(() => this.Set_icon_and_emoji(item_icon.img_icon.sprite, s_color, icon_data["id"].ToString()));
        }
        PlayerPrefs.SetString("s_data_icon_temp",Json.Serialize(this.list_icon_name));
    }

    public IList Get_list_icon_name()
    {
        return this.list_icon_name;
    }

    public int Count_icon_name()
    {
        if (this.list_icon_name == null)
            return 0;
        else
            return this.list_icon_name.Count;
    }

    public void Add_icon_to_list(string s_id_icon)
    {
        if (!this.list_icon_name.Contains(s_id_icon))
        {
            this.list_icon_name.Add(s_id_icon);
        }
    }

    private void Head_btn(Carrot_Box box)
    {
        if (this.list_icon_name.Count > 0)
        {
            Carrot_Box_Btn_Item btn_search= box.create_btn_menu_header(this.app.carrot.icon_carrot_search);
            btn_search.set_act(()=> Show_search_icon());
        }
    }

    private void Show_search_icon()
    {
        this.box_search_icon=this.app.carrot.show_search(Done_act_search, "Enter icon name to search");
    }

    private void Done_act_search(string s_name)
    {
        IList list_name_result = (IList)Json.Deserialize("[]");

        for (int i = 0; i < this.list_icon_name.Count; i++)
        {
            string id_icon = this.list_icon_name[i].ToString();
            if (id_icon.Contains(s_name)) list_name_result.Add(id_icon);
        }

        if (list_name_result.Count > 0)
        {
            if (this.box_search_icon != null) this.box_search_icon.close();
            if (this.box_list_icon != null) this.box_list_icon.close();

            this.box_list_icon = this.app.carrot.Create_Box("result_search");
            this.box_list_icon.set_icon(this.app.command_storage.sp_icon_icons);
            this.box_list_icon.set_title("Icon search results");

            for (int i = 0; i < list_name_result.Count; i++)
            {
                string id_icon = list_name_result[i].ToString();
                if (id_icon.Contains(s_name))
                {
                    Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_icon);
                    if (sp_icon != null)
                    {
                        UnityEngine.Color color_icon = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        string s_color ="#"+ColorUtility.ToHtmlStringRGBA(color_icon);
                        Carrot_Box_Item item_result = this.box_list_icon.create_item("item_result_" + i);
                        item_result.set_title(id_icon);
                        item_result.set_tip(s_color);
                        item_result.txt_tip.color = color_icon;
                        item_result.set_icon_white(sp_icon);
                        item_result.set_act(() => this.Set_icon_and_emoji(item_result.img_icon.sprite, s_color, id_icon));
                    }
                }
            }
        }
        else
        {
            this.app.carrot.show_msg("None");
        }
    }
}
