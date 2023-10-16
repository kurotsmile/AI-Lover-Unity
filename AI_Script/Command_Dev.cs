using Carrot;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Command_Dev_Type {pending,by_user,same_key}

public class Command_Dev : MonoBehaviour
{
    public App app;
    public Sprite sp_icon_key_same;
    public Sprite sp_icon_translate;
    public Sprite sp_icon_chat_passed;
    public Sprite sp_icon_chat_pending;
    public GameObject btn_chat_dev;
    public GameObject btn_chat_pass_user;

    private Command_Dev_Type type;
    private List<GameObject> list_obj_box = new List<GameObject>();

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
        Query ChatQuery = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang());
        ChatQuery = ChatQuery.WhereEqualTo("status", "pending");
        ChatQuery.Limit(20).GetSnapshotAsync().ContinueWithOnMainThread(task => {  
            QuerySnapshot capitalQuerySnapshot = task.Result;
            if (task.IsFaulted) this.app.carrot.hide_loading();

            if (task.IsCompleted)
            {
                this.app.carrot.hide_loading();

                if (capitalQuerySnapshot.Count > 0)
                {
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }

                    Carrot_Box box=this.box_list(list_chat);
                    box.set_icon(this.app.command.icon_info_chat);
                    box.set_title("Chat Pending (Dev)");
                }
            }
        });
    }

    public void delete(string s_id_chat,GameObject obj_item_chat=null)
    {
        DocumentReference chatRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).Document(s_id_chat);
        chatRef.DeleteAsync();
        this.app.carrot.show_msg("Chat", "Delete Success", Msg_Icon.Success);
        if (obj_item_chat != null)
        {
            Destroy(obj_item_chat);
            this.close_box_last();
        }
    }

    public void show_chat_key_same(string s_key_chat)
    {
        this.type = Command_Dev_Type.same_key;
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        Query ChatQuery = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).WhereEqualTo("key", s_key_chat);
        ChatQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot capitalQuerySnapshot = task.Result;
            if (task.IsFaulted)
            {
                this.app.carrot.hide_loading();
            }

            if (task.IsCompleted)
            {
                this.app.carrot.hide_loading();

                if (capitalQuerySnapshot.Count > 0)
                {
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }

                    Carrot_Box box=this.box_list(list_chat);
                    box.set_title(s_key_chat);
                    box.set_icon(this.sp_icon_key_same);
                }
            }
        });
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
        Query ChatQuery = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang());
        ChatQuery = ChatQuery.WhereEqualTo("user.id",this.app.carrot.user.get_id_user_login());
        ChatQuery.Limit(20).GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot capitalQuerySnapshot = task.Result;
            if (task.IsFaulted)
            {
                this.app.carrot.hide_loading();
            }

            if (task.IsCompleted)
            {
                this.app.carrot.hide_loading();

                if (capitalQuerySnapshot.Count > 0)
                {

                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }
                    Carrot_Box box=this.box_list(list_chat);
                    box.set_title(PlayerPrefs.GetString("command_pass", "Published chat"));
                    box.set_icon(this.sp_icon_chat_passed);
                }
                else
                {
                    this.app.carrot.hide_loading();
                    this.app.carrot.show_msg(PlayerPrefs.GetString("brain_list", "List command"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
                }
            }
        });
    }

    public Carrot_Box box_list(IList<IDictionary> list_data)
    {
        if (list_data.Count == 0)
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("brain_list", "List command"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
            return null;
        }

        Carrot_Box box = this.app.carrot.Create_Box();
        this.list_obj_box.Add(box.gameObject);

        for (int i = 0; i < list_data.Count; i++)
        {
            IDictionary c = list_data[i];
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

            if (c["index_cm"] != null)
            {
                if (c["index_cm"].ToString() != "")
                {
                    int index_cm = int.Parse(c["index_cm"].ToString());
                    item_chat.set_act(() => this.app.command_storage.show_edit_command(index_cm));
                }
            }

            if (this.app.carrot.model_app == ModelApp.Develope)
            {
                Carrot_Box_Btn_Item btn_same = item_chat.create_item();
                btn_same.set_icon(this.sp_icon_key_same);
                btn_same.set_color(this.app.carrot.color_highlight);
                btn_same.set_act(() => show_chat_key_same(key_chat));

                item_chat.set_act(() => this.app.command_storage.show_edit_dev(c));
            }

            Carrot_Box_Btn_Item btn_sub_menu = item_chat.create_item();
            btn_sub_menu.set_icon(this.app.carrot.icon_carrot_all_category);
            btn_sub_menu.set_color(this.app.carrot.color_highlight);
            btn_sub_menu.set_act(() => this.sub_menu(c,item_chat.gameObject));
        }
        return box;
    }

    private void sub_menu(IDictionary data,GameObject obj_focus=null)
    {
        Carrot_Box box_sub_menu = this.app.carrot.Create_Box("sub_menu");
        box_sub_menu.set_icon(this.app.carrot.icon_carrot_all_category);
        if (data["key"]!=null) box_sub_menu.set_title(data["key"].ToString());
        this.list_obj_box.Add(box_sub_menu.gameObject);

        Carrot_Box_Item item_info = box_sub_menu.create_item();
        item_info.set_icon(this.app.command.icon_info_chat);
        item_info.set_title("Info");
        item_info.set_tip("View Info");
        item_info.set_act(() => this.app.command.box_info_chat(data));

        Carrot_Box_Item item_add = box_sub_menu.create_item();
        item_add.set_icon(this.app.command_storage.sp_icon_patert);
        item_add.set_title(PlayerPrefs.GetString("brain_add", "Create a new command"));
        item_add.set_tip("Create a conversation with content that continues this conversation");
        item_add.set_act(() => this.app.command_storage.show_add_command_with_pater(data["msg"].ToString(), data["id"].ToString()));

        Carrot_Box_Item item_play=box_sub_menu.create_item();
        item_play.set_icon(this.app.carrot.game.icon_play_music_game);
        item_play.set_title("Test");
        item_play.set_tip("Test preview of chat");
        item_play.set_act(() => this.app.command_storage.play_one_test_command(data));

        if (data["index_cm"] != null)
        {
            if (data["index_cm"].ToString() != "")
            {
                int index_cm = int.Parse(data["index_cm"].ToString());

                Carrot_Box_Item item_play_list = box_sub_menu.create_item();
                item_play_list.set_icon(this.app.player_music.icon_play);
                item_play_list.set_title("List Test");
                item_play_list.set_tip("Test preview of all chat");
                item_play_list.set_act(() => this.app.command_storage.play_test_command(index_cm));

                Carrot_Box_Item item_del_offline = box_sub_menu.create_item();
                item_del_offline.set_icon(this.app.command_storage.sp_icon_delete);
                item_del_offline.set_title("Delete (Offline)");
                item_del_offline.set_tip("Delete chat offline");
                item_del_offline.set_act(() => this.app.command_storage.delete_cm(index_cm, obj_focus));
            }
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
}
