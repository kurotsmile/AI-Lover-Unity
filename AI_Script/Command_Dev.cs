using Carrot;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum Command_Dev_Type { storage, pending, by_user, by_user_field, by_father, same_key }
public class Command_Dev : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("Asset Icon")]
    public Sprite sp_icon_key_same;
    public Sprite sp_icon_translate;
    public Sprite sp_icon_chat_passed;
    public Sprite sp_icon_chat_pending;
    public Sprite sp_icon_chat_live;

    [Header("Ui Chat Dev")]
    public GameObject btn_chat_dev;
    public GameObject btn_chat_pass_user;

    private Command_Dev_Type type = Command_Dev_Type.storage;
    private List<GameObject> list_obj_box = new List<GameObject>();
    private IList<IDictionary> list_data_test = new List<IDictionary>();
    private Carrot_Window_Input box_inp_text;
    private OrderBy_Type order;
    private string s_id_fiel_view_cur = "";
    private string s_key_chat_temp = "";
    public void set_type(Command_Dev_Type type_cmd)
    {
        this.type = type_cmd;
    }

    public void check()
    {
        this.btn_chat_dev.SetActive(false);
        this.btn_chat_pass_user.SetActive(false);
    }

    public void show()
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    public void delete(string s_id_chat, GameObject obj_item_chat = null)
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    public void show_chat_key_same(string s_key_chat)
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    public void set_all_box_active(bool is_act)
    {
        for (int i = 0; i < this.list_obj_box.Count; i++)
        {
            if (this.list_obj_box[i] != null) this.list_obj_box[i].SetActive(is_act);
        }
    }

    public void close_all_box()
    {
        if (this.list_obj_box.Count > 0)
        {
            for (int i = 0; i < this.list_obj_box.Count; i++)
            {
                if (this.list_obj_box[i] != null) this.list_obj_box[i].GetComponent<Carrot_Box>().close();
            }
            this.list_obj_box = new List<GameObject>();
        }
    }

    public void show_chat_pass_by_user()
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    public void show_chat_by_father(string s_id_fathe)
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    public Carrot_Box box_list(IList<IDictionary> list_data)
    {
        if (list_data.Count == 0)
        {
            this.app.carrot.Show_msg(app.carrot.L("brain_list", "List command"), app.carrot.L("list_none", "List is empty, no items found!"));
            return null;
        }

        this.list_data_test = list_data;
        Carrot_Box box = this.app.carrot.Create_Box();
        this.list_obj_box.Add(box.gameObject);

        for (int i = 0; i < list_data.Count; i++)
        {
            IDictionary c = list_data[i];
            c["index_list"] = i;
            c["type_command"] = "list";
            string id_chat = "";
            string key_chat = c["key"].ToString();
            if (c["id"] != null)
            {
                id_chat = c["id"].ToString();
                if (id_chat == "") id_chat = "chat_" + i;
            }
            else
            {
                id_chat = "chat_" + i;
            }

            Carrot_Box_Item item_chat = box.create_item("item_chat" + i);

            item_chat.set_title(key_chat);
            if (c["msg"] != null) item_chat.set_tip(c["msg"].ToString());

            string s_status = "";
            if (c["status"] != null)
            {
                s_status = c["status"].ToString();
                if (s_status == "passed")
                {
                    if (this.type == Command_Dev_Type.by_user)
                        item_chat.set_icon(this.app.command_storage.icon_command_pass);
                    else
                        item_chat.set_icon(this.app.command.sp_icon_info_add_chat);

                    item_chat.img_icon.color = this.app.carrot.color_highlight;
                }
                else if (s_status == "buy")
                {
                    item_chat.img_icon.sprite = this.app.command_storage.sp_icon_command_purchased;
                    item_chat.img_icon.color = this.app.carrot.color_highlight;
                }
                else
                {
                    item_chat.set_icon(this.sp_icon_chat_pending);
                    item_chat.img_icon.color = Color.black;
                }
            }
            else
            {
                item_chat.set_icon(this.app.command.sp_icon_info_add_chat);
                item_chat.img_icon.color = Color.black;
            }

            if (c["pater"] != null)
            {
                if (c["pater"].ToString() != "")
                {
                    string s_id_chat_father = c["pater"].ToString();
                    Carrot_Box_Btn_Item btn_father = item_chat.create_item();
                    btn_father.set_color(this.app.carrot.color_highlight);
                    btn_father.set_icon(this.app.command_storage.sp_icon_father);
                    btn_father.set_act(() => this.app.command.Show_info_chat_by_id(s_id_chat_father));
                }
            }

            Carrot.Carrot_Box_Btn_Item btn_add = item_chat.create_item();
            btn_add.set_color(this.app.carrot.color_highlight);
            btn_add.set_icon(this.app.command_storage.sp_icon_patert);
            btn_add.set_act(() => this.app.command_storage.show_add_command_with_pater(c["msg"].ToString(), c["id"].ToString()));

            Carrot_Box_Btn_Item btn_sub_menu = item_chat.create_item();
            btn_sub_menu.set_icon(this.app.carrot.icon_carrot_all_category);
            btn_sub_menu.set_color(this.app.carrot.color_highlight);
            btn_sub_menu.set_act(() => this.sub_menu(c, item_chat.gameObject));

            if (c["index_item"] != null)
            {
                int index_item = int.Parse(c["index_item"].ToString());
                item_chat.set_act(() => this.app.command_storage.show_edit_command_sys(index_item, item_chat));
            }
            else
            {
                if (this.app.carrot.model_app == ModelApp.Develope)
                    item_chat.set_act(() => this.app.command_storage.show_edit_dev(c, item_chat));
                else
                    item_chat.set_act(() => this.app.command.box_info_chat(c));
            }
        }

        if (this.type == Command_Dev_Type.pending || this.type == Command_Dev_Type.by_user || this.type == Command_Dev_Type.by_user_field)
        {
            Carrot_Box_Item item_order_by = box.create_item();
            item_order_by.set_icon(this.app.command_storage.sp_icon_random);
            item_order_by.set_title("Sort (" + this.order.ToString() + ")");
            item_order_by.set_tip("Rearrange the data list in random patterns");
            item_order_by.set_act(() => this.act_change_order_sort());
        }
        return box;
    }

    private void act_change_order_sort()
    {
        this.close_box_last();

        if (this.order == OrderBy_Type.date_asc)
            this.order = OrderBy_Type.date_desc;
        else if (this.order == OrderBy_Type.date_desc)
            this.order = OrderBy_Type.name_asc;
        else if (this.order == OrderBy_Type.name_asc)
            this.order = OrderBy_Type.name_desc;
        else
            this.order = OrderBy_Type.date_asc;

        if (this.type == Command_Dev_Type.pending)
            this.show();
        else if (this.type == Command_Dev_Type.by_user)
            this.show_chat_pass_by_user();
        else if (this.type == Command_Dev_Type.by_user_field)
            this.show_chat_by_user_id(this.s_id_fiel_view_cur);
    }

    public void sub_menu(IDictionary data, GameObject obj_focus = null)
    {
        string s_status = "";
        string s_key = "";

        if (data["status"] != null) s_status = data["status"].ToString();
        Carrot_Box box_sub_menu = this.app.carrot.Create_Box("sub_menu");
        box_sub_menu.set_icon(this.app.carrot.icon_carrot_all_category);
        if (data["key"] != null)
        {
            s_key = data["key"].ToString();
            box_sub_menu.set_title(s_key);
        }
        this.list_obj_box.Add(box_sub_menu.gameObject);

        Carrot_Box_Item item_info = box_sub_menu.create_item();
        item_info.set_icon(this.app.command.icon_info_chat);
        item_info.set_title("Info");
        item_info.set_tip("View Info");
        item_info.set_act(() => this.app.command.box_info_chat(data));

        Carrot_Box_Item item_add = box_sub_menu.create_item();
        item_add.set_icon(this.app.command_storage.sp_icon_patert);
        item_add.set_title(app.carrot.L("brain_add", "Create a new command"));
        item_add.set_tip("Create a conversation with content that continues this conversation");
        item_add.set_act(() => this.app.command_storage.show_add_command_with_pater(data["msg"].ToString(), data["id"].ToString()));

        if (this.app.carrot.model_app == ModelApp.Develope)
        {
            if (data["pater"] != null)
            {
                if (data["pater"].ToString() != "")
                {
                    string s_id_chat_father = data["pater"].ToString();
                    Carrot_Box_Item item_father = box_sub_menu.create_item();
                    item_father.set_icon(this.app.command_storage.sp_icon_father);
                    item_father.set_title("Dad chat");
                    item_father.set_tip("View this chat's parent chat information");
                    item_father.set_act(() => this.app.command.Show_info_chat_by_id(s_id_chat_father));
                }
            }
        }

        if (s_status != "test" || s_status != "list_test")
        {
            Carrot_Box_Item item_play = box_sub_menu.create_item();
            item_play.set_icon(this.app.carrot.game.icon_play_music_game);
            item_play.set_title("Test");
            item_play.set_tip("Test preview of chat");
            item_play.set_act(() => this.app.command_storage.play_one_test_command(data));

            if (data["index_list"] != null)
            {
                Carrot_Box_Item item_test_list = box_sub_menu.create_item();
                item_test_list.set_icon(this.app.player_music.icon_play);
                item_test_list.set_title("List Test");
                item_test_list.set_tip("Test preview of all chat");
                item_test_list.set_act(() => this.act_play_test_list(int.Parse(data["index_list"].ToString())));
            }
        }


        if (data["index_item"] != null)
        {
            if (data["index_item"].ToString() != "")
            {
                int index_cm = int.Parse(data["index_item"].ToString());

                if (this.app.carrot.model_app == ModelApp.Develope)
                {
                }

                Carrot_Box_Item item_edit_offline = box_sub_menu.create_item();
                item_edit_offline.set_icon(this.app.carrot.user.icon_user_edit);
                item_edit_offline.set_title("Edit (Offline)");
                item_edit_offline.set_tip("Edit chat offline");
                item_edit_offline.set_act(() => this.app.command_storage.show_edit_command_sys(index_cm, null));

                Carrot_Box_Item item_del_offline = box_sub_menu.create_item();
                item_del_offline.set_icon(this.app.command_storage.sp_icon_delete);
                item_del_offline.set_title("Delete (Offline)");
                item_del_offline.set_tip("Delete chat offline");
                item_del_offline.set_act(() => this.app.command_storage.Delete_cm_sys(index_cm, obj_focus));
            }
        }

        if (data["id"] != null)
        {
            string s_id_chat_report = data["id"].ToString();
            string s_tip_report = "View reports for this chat";
            if (data["reports"] != null)
            {
                IList list_report = (IList)data["reports"];
                s_tip_report = list_report.Count + " Report";
            }
            Carrot_Box_Item item_report = box_sub_menu.create_item();
            item_report.set_title(PlayerPrefs.GetString("report_title", "Report"));
            item_report.set_tip(s_tip_report);
            item_report.set_icon(this.app.command.sp_icon_info_report_chat);
            item_report.set_act(() => this.app.report.show_list_report_by_object_id(s_id_chat_report));
        }

    }

    public void close_box_last()
    {
        int index_last = this.list_obj_box.Count - 1;
        if (this.list_obj_box[index_last] != null) this.list_obj_box[index_last].GetComponent<Carrot_Box>().close();
    }

    private void show_chat_by_user()
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    private void done_show_list_by_user(string s_username)
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    private void show_chat_by_user_id(string s_user_name)
    {
        this.app.carrot.Show_msg("Command Dev", "Server-related dev functions have been removed.");
    }

    #region Command Test 
    public void act_play_test_list(int index_play)
    {
        this.set_all_box_active(false);
        IDictionary data_test = this.list_data_test[index_play];
        data_test["status"] = "test_list";
        this.app.command_storage.act_test_command(data_test);
    }

    public int length_list_test()
    {
        return this.list_data_test.Count;
    }
    #endregion
}
