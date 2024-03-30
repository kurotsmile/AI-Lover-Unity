using Carrot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Command_Dev_Type {storage,pending,by_user, by_user_field, by_father,same_key}
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
        if (app.carrot.model_app == ModelApp.Develope)
            this.btn_chat_dev.SetActive(true);
        else
            this.btn_chat_dev.SetActive(false);

        if (app.carrot.user.get_id_user_login() != "")
            this.btn_chat_pass_user.SetActive(true);
        else
            this.btn_chat_pass_user.SetActive(false);
    }

    public void show()
    {
        this.type = Command_Dev_Type.pending;
        this.app.carrot.show_loading();

        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Add_where("status", Query_OP.EQUAL, "pending");

        if (this.order == OrderBy_Type.date_desc)
            q.Add_order("date_create", Query_Order_Direction.DESCENDING);
        else if (this.order == OrderBy_Type.date_asc)
            q.Add_order("date_create", Query_Order_Direction.ASCENDING);
        else if (this.order == OrderBy_Type.name_desc)
            q.Add_order("key", Query_Order_Direction.DESCENDING);
        else
            q.Add_order("key", Query_Order_Direction.ASCENDING);

        this.app.carrot.server.Get_doc(q.ToJson());
    }

    private void Act_get_list_cm_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            List<IDictionary> list_chat = new List<IDictionary>();
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary chat_data = fc.fire_document[i].Get_IDictionary();
                list_chat.Add(chat_data);
            }
            Carrot_Box box = this.box_list(list_chat);
            box.set_icon(this.app.command.icon_info_chat);
            box.set_title("Chat Pending (Dev)");

            Carrot_Box_Btn_Item btn_dev_user = box.create_btn_menu_header(this.app.carrot.user.icon_user_login_true);
            btn_dev_user.set_act(() => this.show_chat_by_user());
        }
    }

    public void delete(string s_id_chat,GameObject obj_item_chat=null)
    {
        this.app.carrot.server.Delete_Doc("chat-" + this.app.carrot.lang.Get_key_lang(), s_id_chat);
        this.app.carrot.Show_msg("Chat", "Delete Success", Msg_Icon.Success);
        if (obj_item_chat != null)
        {
            Destroy(obj_item_chat);
            this.close_box_last();
        }
    }

    public void show_chat_key_same(string s_key_chat)
    {
        this.s_key_chat_temp = s_key_chat;
        this.type = Command_Dev_Type.same_key;
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Add_where("key",Query_OP.EQUAL, s_key_chat);
        q.Set_limit(20);
        this.app.carrot.server.Get_doc(q.ToJson(), Act_show_chat_key_same_done, Act_show_chat_key_same_fail);
    }

    private void Act_show_chat_key_same_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            List<IDictionary> list_chat = new();
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary chat_data = fc.fire_document[i].Get_IDictionary();
                list_chat.Add(chat_data);
            }

            Carrot_Box box = this.box_list(list_chat);
            box.set_title(this.s_key_chat_temp);
            box.set_icon(this.sp_icon_key_same);
        }
    }

    private void Act_show_chat_key_same_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.carrot.Show_msg("Error", s_error, Msg_Icon.Error);
    }

    public void set_all_box_active(bool is_act)
    {
        for(int i = 0; i < this.list_obj_box.Count; i++)
        {
            if(this.list_obj_box[i]!=null) this.list_obj_box[i].SetActive(is_act);
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
        this.type = Command_Dev_Type.by_user;
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Add_where("user.id",Query_OP.EQUAL,this.app.carrot.user.get_id_user_login());
        q.Set_limit(20);
        this.app.carrot.server.Get_doc("chat-" + this.app.carrot.lang.Get_key_lang(), Act_show_chat_pass_by_user_done, Act_show_chat_pass_by_user_fail);
    }

    private void Act_show_chat_pass_by_user_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            List<IDictionary> list_chat = new();
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary chat_data = fc.fire_document[i].Get_IDictionary();
                list_chat.Add(chat_data);
            }
            Carrot_Box box = this.box_list(list_chat);
            box.set_title(app.carrot.L("command_pass", "Published chat"));
            box.set_icon(this.sp_icon_chat_passed);
        }
        else
        {
            this.app.carrot.hide_loading();
            this.app.carrot.Show_msg(app.carrot.L("brain_list", "List command"), app.carrot.L("list_none", "List is empty, no items found!"));
        }
    }

    private void Act_show_chat_pass_by_user_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.carrot.Show_msg("Error", s_error, Msg_Icon.Error);
    }

    public void show_chat_by_father(string s_id_fathe)
    {
        this.type = Command_Dev_Type.by_father;
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Set_limit(20);
        q.Add_where("pater",Query_OP.EQUAL, s_id_fathe);
        if (this.order == OrderBy_Type.date_desc)
            q.Add_order("date_create", Query_Order_Direction.DESCENDING);
        else if (this.order == OrderBy_Type.date_asc)
            q.Add_order("date_create", Query_Order_Direction.ASCENDING);
        else if (this.order == OrderBy_Type.name_desc)
            q.Add_order("key", Query_Order_Direction.DESCENDING);
        else
            q.Add_order("key", Query_Order_Direction.ASCENDING);

        this.app.carrot.server.Get_doc(q.ToJson(), Act_show_chat_by_father_done, Act_show_chat_by_father_fail);
    }

    private void Act_show_chat_by_father_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            List<IDictionary> list_chat = new();
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary chat_data = fc.fire_document[i].Get_IDictionary();
                list_chat.Add(chat_data);
            }
            Carrot_Box box = this.box_list(list_chat);
            box.set_title(app.carrot.L("command_pass", "Published chat"));
            box.set_icon(this.sp_icon_chat_passed);
        }
        else
        {
            this.app.carrot.hide_loading();
            this.app.carrot.Show_msg(app.carrot.L("brain_list", "List command"),app.carrot.L("list_none", "List is empty, no items found!"));
        }
    }

    private void Act_show_chat_by_father_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.carrot.Show_msg("Error", s_error, Msg_Icon.Error);
    }

    public Carrot_Box box_list(IList<IDictionary> list_data)
    {
        if (list_data.Count == 0)
        {
            this.app.carrot.Show_msg(app.carrot.L("brain_list", "List command"),app.carrot.L("list_none", "List is empty, no items found!"));
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
            string id_chat = c["id"].ToString();
            string key_chat = c["key"].ToString();

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
                    btn_father.set_act(() => this.app.command.show_info_chat_by_id(s_id_chat_father));
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

            if (c["index_cm"] != null)
            {
                if (c["index_cm"].ToString() != "")
                {
                    int index_cm = int.Parse(c["index_cm"].ToString());
                    item_chat.set_act(() => this.app.command_storage.show_edit_command(index_cm,item_chat));
                }
                else
                {
                    if(this.app.carrot.model_app==ModelApp.Develope) 
                        item_chat.set_act(() => this.app.command_storage.show_edit_dev(c,item_chat));
                    else
                        item_chat.set_act(() => this.app.command.box_info_chat(c));
                }
            }
            else
            {
                if(this.app.carrot.model_app==ModelApp.Develope) 
                    item_chat.set_act(() => this.app.command_storage.show_edit_dev(c,item_chat));
                else
                    item_chat.set_act(() => this.app.command.box_info_chat(c));
            }
        }

        if (this.type == Command_Dev_Type.pending||this.type==Command_Dev_Type.by_user||this.type==Command_Dev_Type.by_user_field)
        {
            Carrot_Box_Item item_order_by = box.create_item();
            item_order_by.set_icon(this.app.command_storage.sp_icon_random);
            item_order_by.set_title("Sort ("+this.order.ToString()+")");
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

    public void sub_menu(IDictionary data,GameObject obj_focus=null)
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
            Carrot_Box_Item item_same = box_sub_menu.create_item();
            item_same.set_icon(this.sp_icon_key_same);
            item_same.set_title("Chat with the same keyword");
            item_same.set_tip("See a list of Chats that match the same keyword");
            item_same.set_act(() => show_chat_key_same(s_key));

            if(s_status== "passed")
            {
                Carrot_Box_Item item_edit_cm_pass = box_sub_menu.create_item();
                item_edit_cm_pass.set_icon(this.app.carrot.user.icon_user_edit);
                item_edit_cm_pass.set_title("Edit Passed");
                item_edit_cm_pass.set_tip("Chat Update (Dev)");
                if(obj_focus!=null)
                    item_edit_cm_pass.set_act(() => this.app.command_storage.show_edit_pass(data, obj_focus.GetComponent<Carrot_Box_Item>()));
                else
                    item_edit_cm_pass.set_act(() => this.app.command_storage.show_edit_pass(data, null));
            }

            if (data["id"].ToString() != "")
            {
                Carrot_Box_Item item_child = box_sub_menu.create_item();
                item_child.set_icon(this.app.carrot.icon_carrot_database);
                item_child.set_title("List child chat");
                item_child.set_tip("List of child chat sentences of parent chat");
                item_child.set_act(() => show_chat_by_father(data["id"].ToString()));
            }

            if (data["pater"] != null)
            {
                if (data["pater"].ToString() != "")
                {
                    string s_id_chat_father = data["pater"].ToString();
                    Carrot_Box_Item item_father = box_sub_menu.create_item();
                    item_father.set_icon(this.app.command_storage.sp_icon_father);
                    item_father.set_title("Dad chat");
                    item_father.set_tip("View this chat's parent chat information");
                    item_father.set_act(() => this.app.command.show_info_chat_by_id(s_id_chat_father));
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


        if (data["index_cm"] != null)
        {
            if (data["index_cm"].ToString() != "")
            {
                int index_cm = int.Parse(data["index_cm"].ToString());

                if (this.app.carrot.model_app == ModelApp.Develope)
                {
                    if (s_status!="passed")
                    {
                        Carrot_Box_Item item_edit_cm_pending = box_sub_menu.create_item();
                        item_edit_cm_pending.set_icon(this.app.carrot.user.icon_user_edit);
                        item_edit_cm_pending.set_title("Edit Pending to Passed");
                        item_edit_cm_pending.set_tip("Edit the draft and publish this chat (Dev)");
                        if (obj_focus != null)
                            item_edit_cm_pending.set_act(() => this.app.command_storage.show_edit_pending_to_pass(index_cm, obj_focus.GetComponent<Carrot_Box_Item>()));
                        else
                            item_edit_cm_pending.set_act(() => this.app.command_storage.show_edit_pending_to_pass(index_cm, null));
                    }
                }

                Carrot_Box_Item item_edit_offline = box_sub_menu.create_item();
                item_edit_offline.set_icon(this.app.carrot.user.icon_user_edit);
                item_edit_offline.set_title("Edit (Offline)");
                item_edit_offline.set_tip("Edit chat offline");
                item_edit_offline.set_act(() => this.app.command_storage.show_edit_command(index_cm, null));

                Carrot_Box_Item item_del_offline = box_sub_menu.create_item();
                item_del_offline.set_icon(this.app.command_storage.sp_icon_delete);
                item_del_offline.set_title("Delete (Offline)");
                item_del_offline.set_tip("Delete chat offline");
                item_del_offline.set_act(() => this.app.command_storage.delete_cm(index_cm, obj_focus));
            }
        }

        if(data["reports"]!=null)
        {
            IList list_report = (IList)data["reports"];
            Carrot_Box_Item item_report = box_sub_menu.create_item();
            item_report.set_title(PlayerPrefs.GetString("report_title", "Report"));
            item_report.set_tip(list_report.Count + " Report");
            item_report.set_icon(this.app.command.sp_icon_info_report_chat);
            item_report.set_act(() => this.app.report.show_list_report(list_report));
        }

        if (this.app.carrot.model_app == ModelApp.Develope)
        {
            Carrot_Box_Item item_del = box_sub_menu.create_item();
            item_del.set_icon(this.app.carrot.sp_icon_del_data);
            item_del.img_icon.color = Color.red;
            item_del.set_title("Delete (Dev)");
            item_del.set_tip("Delete chat on server");
            item_del.set_act(() => this.delete(data["id"].ToString(), obj_focus));
        }
    }

    public void close_box_last()
    {
        int index_last = this.list_obj_box.Count - 1;
        if (this.list_obj_box[index_last] != null) this.list_obj_box[index_last].GetComponent<Carrot_Box>().close();
    }

    private void show_chat_by_user()
    {
        this.box_inp_text= this.app.carrot.show_input("Show List Chat By User", "Enter Id Username user to view");
        this.box_inp_text.set_act_done(this.done_show_list_by_user);
    }

    private void done_show_list_by_user(string s_username)
    {
        if (this.box_inp_text != null) this.box_inp_text.close();
        this.show_chat_by_user_id(s_username);
    }

    private void show_chat_by_user_id(string s_user_name)
    {
        this.s_key_chat_temp = s_user_name;
        this.s_id_fiel_view_cur = s_user_name;
        this.type = Command_Dev_Type.by_user_field;
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();

        StructuredQuery q = new("chat-" + this.app.carrot.lang.Get_key_lang());
        q.Add_where("user.name", Query_OP.EQUAL, s_user_name);
        q.Set_limit(20);

        if (this.order == OrderBy_Type.date_desc)
            q.Add_order("date_create", Query_Order_Direction.DESCENDING);
        else if (this.order == OrderBy_Type.date_asc)
            q.Add_order("date_create", Query_Order_Direction.ASCENDING);
        else if (this.order == OrderBy_Type.name_desc)
            q.Add_order("key", Query_Order_Direction.DESCENDING);
        else
            q.Add_order("key", Query_Order_Direction.ASCENDING);

        this.app.carrot.server.Get_doc(q.ToJson(), Act_show_chat_by_user_id_done, Act_show_chat_by_user_id_fail);
    }

    private void Act_show_chat_by_user_id_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            List<IDictionary> list_chat = new List<IDictionary>();
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary chat_data = fc.fire_document[i].Get_IDictionary();
                list_chat.Add(chat_data);
            }
            Carrot_Box box = this.box_list(list_chat);
            box.set_title(this.s_key_chat_temp);
            box.set_icon(this.sp_icon_chat_passed);
        }
        else
        {
            this.app.carrot.hide_loading();
            this.app.carrot.Show_msg(app.carrot.L("brain_list", "List command"),app.carrot.L("list_none", "List is empty, no items found!"));
        }
    }

    private void Act_show_chat_by_user_id_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.carrot.Show_msg("Error", s_error, Msg_Icon.Error);
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
