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

    private Carrot_Box box;

    private Command_Dev_Type type;

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
            if (task.IsFaulted)
            {
                this.app.carrot.hide_loading();
            }

            if (task.IsCompleted)
            {
                this.app.carrot.hide_loading();

                if (capitalQuerySnapshot.Count > 0)
                {
                    this.box=this.app.carrot.Create_Box("Chat Pending (Dev)");
                    this.box.set_icon(this.app.command.icon_info_chat);
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }
                    this.box_list(list_chat);
                }
            }
        });
    }

    public void delete(string s_id_chat,GameObject obj_item_chat)
    {
        DocumentReference chatRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).Document(s_id_chat);
        chatRef.DeleteAsync();
        this.app.carrot.show_msg("Chat", "Delete Success", Msg_Icon.Success);
        if(obj_item_chat!=null) Destroy(obj_item_chat);
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
                    this.box = this.app.carrot.Create_Box("chat_key_same");
                    this.box.set_title(s_key_chat);
                    this.box.set_icon(this.sp_icon_key_same);
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }
                    this.box_list(list_chat);
                }
            }
        });
    }

    public Carrot_Box get_box_list()
    {
        return this.box;
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
                    this.box = this.app.carrot.Create_Box("user_chat");
                    this.box.set_title(PlayerPrefs.GetString("command_pass","Published chat"));
                    this.box.set_icon(this.sp_icon_chat_passed);
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        IDictionary chat_data = documentSnapshot.ToDictionary();
                        chat_data["id"] = documentSnapshot.Id;
                        list_chat.Add(chat_data);
                    }
                    this.box_list(list_chat);
                }
                else
                {
                    this.app.carrot.hide_loading();
                    this.app.carrot.show_msg(PlayerPrefs.GetString("brain_list", "List command"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
                }
            }
        });
    }

    private void box_list(IList<IDictionary> list_data)
    {
        for(int i = 0; i < list_data.Count; i++)
        {
            IDictionary c =list_data[i];
            string id_chat = c["id"].ToString();
            string key_chat = c["key"].ToString();

            Carrot_Box_Item item_chat = this.box.create_item("item_chat"+i);

            item_chat.set_title(key_chat);
            if(c["msg"]!=null) item_chat.set_tip(c["msg"].ToString());

            if (c["status"] != null)
            {
                string s_status = c["status"].ToString();
                if (s_status == "passed")
                {
                    if (this.type == Command_Dev_Type.by_user)
                        item_chat.set_icon(this.app.command_storage.icon_command_pass);
                    else
                        item_chat.set_icon(this.app.command.sp_icon_info_add_chat);

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

            Carrot.Carrot_Box_Btn_Item btn_play = item_chat.create_item();
            btn_play.set_icon(this.app.player_music.icon_play);
            btn_play.set_color(this.app.carrot.color_highlight);
            btn_play.set_act(() => this.app.command_storage.play_one_test_command(c));

            if (this.app.carrot.model_app == ModelApp.Develope)
            {
                Carrot_Box_Btn_Item btn_same = item_chat.create_item();
                btn_same.set_icon(this.sp_icon_key_same);
                btn_same.set_color(this.app.carrot.color_highlight);
                btn_same.set_act(() => show_chat_key_same(key_chat));

                Carrot_Box_Btn_Item btn_del = item_chat.create_item();
                btn_del.set_icon(this.app.carrot.sp_icon_del_data);
                btn_del.set_color(Color.red);
                btn_del.set_act(() => this.delete(id_chat, item_chat.gameObject));

                item_chat.set_act(() => this.app.command_storage.show_edit_dev(c));
            }
        }
    }
}
