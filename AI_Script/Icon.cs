using Carrot;
using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using System.Collections;
using UnityEngine.UI;

public class Icon : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;
    public int index_buy_category_icon;

    [Header("Icon Obj")]
    public Sprite sp_icon_icons;
    private QuerySnapshot IconQuerySnapshot;
    private QuerySnapshot IconCategoryQuerySnapshot = null;
    private IList list_icon_name;
    private Carrot.Carrot_Box box_list_icon;
    private Carrot.Carrot_Box box_list_icon_category;
    private Carrot_Window_Input box_search_icon;

    private Carrot_Box_Item item_icon = null;
    private string s_data_cache = "";
    private string s_id_category_buy_temp;

    public void load()
    {
        this.s_data_cache=PlayerPrefs.GetString("s_data_icon_temp","");
        if (this.s_data_cache != "") 
            this.list_icon_name = (IList)Json.Deserialize(this.s_data_cache);
        else 
            this.list_icon_name = (IList)Json.Deserialize("[]");
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

        this.head_btn(this.box_list_icon_category);

        foreach (DocumentSnapshot documentSnapshot in IconCategoryQuerySnapshot.Documents)
        {
            string s_status_buy = "free";
            IDictionary icon_data = documentSnapshot.ToDictionary();  
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

                    if (s_status_buy != "free") {
                        if (PlayerPrefs.GetInt("is_buy_0", 0) == 1) s_status_buy = "free";
                    }
                }

                if (s_status_buy != "free")
                {
                    Carrot_Box_Btn_Item btn_buy=item_cat.create_item();
                    btn_buy.set_icon(this.app.setting.sp_icon_buy);
                    btn_buy.set_color(this.app.carrot.color_highlight);
                    Destroy(btn_buy.GetComponent<Button>());

                    if(this.app.carrot.model_app==ModelApp.Publish)
                        item_cat.set_act(() => this.act_buy_category(s_key_cat));
                    else
                        item_cat.set_act(() => this.view_list_icon_by_category_key(s_key_cat));
                }
                else
                {
                    item_cat.set_act(() => this.view_list_icon_by_category_key(s_key_cat));
                }
            }
        };
        this.box_list_icon_category.update_color_table_row();
    }

    private void act_buy_category(string s_id_category)
    {
        this.s_id_category_buy_temp = s_id_category;
        this.app.carrot.shop.buy_product(this.index_buy_category_icon);
    }

    public void act_buy_category_success()
    {
        if (this.s_id_category_buy_temp != "")
        {
            this.view_list_icon_by_category_key(this.s_id_category_buy_temp);
            PlayerPrefs.SetInt("is_buy_category_icon_" + this.s_id_category_buy_temp, 1);
            this.s_id_category_buy_temp="";
        }
    }

    private void view_list_icon_by_category_key(string s_key)
    {
        Query IconRef = this.app.carrot.db.Collection("icon");
        IconRef = IconRef.WhereEqualTo("category", s_key);
        IconRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                this.IconQuerySnapshot = task.Result;
                this.act_load_icon_and_emoji(this.IconQuerySnapshot);
            }
        });
    }

    public void set_icon_and_emoji(Sprite sp_cm, string s_color, string s_id)
    {
        this.app.command_storage.set_s_color(s_color);
        this.app.command_storage.set_s_id_icon(s_id);
        this.item_icon.set_val(s_id);
        this.item_icon.set_icon_white(sp_cm);

        UnityEngine.Color color_item;
        ColorUtility.TryParseHtmlString(s_color, out color_item);

        this.item_icon.txt_val.color = color_item;
        if (this.box_list_icon != null) this.box_list_icon.close();
        if (this.box_list_icon_category != null) this.box_list_icon_category.close();
    }

    public void btn_show_list_emoji_and_color(Carrot_Box_Item item_icon_change)
    {
        this.item_icon=item_icon_change;
        this.app.carrot.show_loading();
        if (IconQuerySnapshot == null)
        {
            Query IconQuery = this.app.carrot.db.Collection("icon");
            IconQuery = IconQuery.Limit(50);
            IconQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>{
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
        Carrot_Box_Btn_Item btn_icon_category = this.box_list_icon.create_btn_menu_header(this.app.carrot.icon_carrot_all_category);
        btn_icon_category.set_act(() => list_category_icon());

        this.head_btn(this.box_list_icon);

        foreach (DocumentSnapshot document in query_icon)
        {
            string id_icon = document.Id;
            string s_color = "#FFFFFF";
            IDictionary icon_data = document.ToDictionary();
            Carrot_Box_Item item_icon = this.box_list_icon.create_item();
            item_icon.set_title(document.Id);
            item_icon.set_tip(icon_data["icon"].ToString());
            this.add_icon_to_list(id_icon);
            if (icon_data["color"] != null)
            {
                s_color = icon_data["color"].ToString();
                UnityEngine.Color color_item;
                ColorUtility.TryParseHtmlString(icon_data["color"].ToString(), out color_item);
                item_icon.set_tip(s_color);
                item_icon.txt_tip.color = color_item;
            }

            Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_icon);
            if (sp_icon != null)
                item_icon.set_icon_white(sp_icon);
            else
                if (icon_data["icon"] != null) this.app.carrot.get_img_and_save_playerPrefs(icon_data["icon"].ToString(), item_icon.img_icon, id_icon);

            item_icon.set_act(() => this.set_icon_and_emoji(item_icon.img_icon.sprite, s_color, id_icon));
        }
        PlayerPrefs.SetString("s_data_icon_temp",Json.Serialize(this.list_icon_name));
    }

    public IList get_list_icon_name()
    {
        return this.list_icon_name;
    }

    public int count_icon_name()
    {
        if (this.list_icon_name == null)
            return 0;
        else
            return this.list_icon_name.Count;
    }

    public void add_icon_to_list(string s_id_icon)
    {
        if (!this.list_icon_name.Contains(s_id_icon))
        {
            this.list_icon_name.Add(s_id_icon);
        }
    }

    private void head_btn(Carrot_Box box)
    {
        if (this.list_icon_name.Count > 0)
        {
            Carrot_Box_Btn_Item btn_search= box.create_btn_menu_header(this.app.carrot.icon_carrot_search);
            btn_search.set_act(()=> show_search_icon());
        }
    }

    private void show_search_icon()
    {
        this.box_search_icon=this.app.carrot.show_search(done_act_search, "Enter icon name to search");
    }

    private void done_act_search(string s_name)
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
                        item_result.set_act(() => this.set_icon_and_emoji(item_result.img_icon.sprite, s_color, id_icon));
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
