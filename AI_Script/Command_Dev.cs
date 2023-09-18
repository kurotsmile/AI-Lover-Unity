using Carrot;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Dev : MonoBehaviour
{
    public App app;
    public Sprite icon;
    public GameObject btn_chat_dev;

    private Carrot_Box box;

    public void check()
    {
        if (app.carrot.model_app == ModelApp.Develope)
            this.btn_chat_dev.SetActive(true);
        else
            this.btn_chat_dev.SetActive(false);
    }

    public void show()
    {
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
                    this.box=this.app.carrot.Create_Box("Chat Pending");
                    this.box.set_icon(this.app.command.icon_info_chat);
                    List<IDictionary> list_chat = new List<IDictionary>();
                    foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
                    {
                        string id_chat=documentSnapshot.Id;
                        IDictionary c = documentSnapshot.ToDictionary();
                        c["id"] = id_chat;
                        Carrot_Box_Item item_chat = this.box.create_item("item_chat");

                        item_chat.set_act(() => this.app.command_storage.show_edit_dev(c));
                        item_chat.set_title(c["key"].ToString());
                        item_chat.set_tip(c["msg"].ToString());
                        item_chat.set_icon(this.app.command.sp_icon_info_add_chat);
                        
                        Carrot_Box_Btn_Item btn_del=item_chat.create_item();
                        btn_del.set_icon(this.app.carrot.sp_icon_del_data);
                        btn_del.set_color(Color.red);
                        btn_del.set_act(() => this.delete(id_chat, item_chat.gameObject));
                    }
                }
                else
                {
                    this.app.carrot.hide_loading();
                }
            }
        });
    }

    public void delete(string s_id_chat,GameObject obj_item_chat)
    {
        DocumentReference chatRef = this.app.carrot.db.Collection("chat-" + this.app.carrot.lang.get_key_lang()).Document(s_id_chat);
        chatRef.DeleteAsync();
        this.app.carrot.show_msg("Chat", "Delete Success", Msg_Icon.Success);
        Destroy(obj_item_chat);
    }
}
