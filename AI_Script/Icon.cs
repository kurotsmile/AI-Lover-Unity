using Carrot;
using Firebase.Firestore;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase.Extensions;
using System.Collections;

public class Icon : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("Icon Obj")]
    public Sprite sp_icon_icons;
    private QuerySnapshot IconQuerySnapshot;
    private QuerySnapshot IconCategoryQuerySnapshot = null;
    private IList list_icon_name;
    private Carrot.Carrot_Box box_list_icon;
    private Carrot.Carrot_Box box_list_icon_category;

    private Carrot_Box_Item item_icon = null;
    private string s_data_cache = "";

    public void load()
    {
        this.s_data_cache=PlayerPrefs.GetString("s_data_icon_temp","");
        if (this.s_data_cache != "")
        {
            this.list_icon_name =(IList) Json.Deserialize(this.s_data_cache);
            Debug.Log("list_icon_name:"+this.list_icon_name.Count);
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

    public void set_icon_and_emoji(Color32 color_cm, Sprite sp_cm, string s_color, string s_id)
    {
        this.app.command_storage.set_s_color(s_color);
        this.app.command_storage.set_s_id_icon(s_id);
        this.item_icon.set_val(s_id);
        this.item_icon.set_icon_white(sp_cm);
        this.item_icon.txt_val.color = color_cm;
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

        this.list_icon_name = new List<string>();

        foreach (DocumentSnapshot document in query_icon)
        {
            string id_icon = document.Id;
            string s_color = "#FFFFFF";
            IDictionary icon_data = document.ToDictionary();
            Carrot_Box_Item item_icon = this.box_list_icon.create_item();
            item_icon.set_title(document.Id);
            item_icon.set_tip(icon_data["icon"].ToString());
            this.list_icon_name.Add(id_icon);
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

        Debug.Log(Json.Serialize(this.list_icon_name));
        PlayerPrefs.SetString("s_data_icon_temp",Json.Serialize(this.list_icon_name));
    }

    public IList get_list_icon_name()
    {
        return this.list_icon_name;
    }
}
