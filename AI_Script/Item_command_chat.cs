using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Item_command_chat : MonoBehaviour
{
    public Image icon;
    public Image icon_type;
    public Text txt_chat;
    public Text txt_tip;
    public string url_edit;
    public GameObject btn_add_web;
    public GameObject btn_add_app;
    public int index;
    public bool is_music;
    public IDictionary idata_chat;

    public void btn_click()
    {
        if (this.url_edit != "") Application.OpenURL(this.url_edit);
    }

    public void delete()
    {
        GameObject.Find("app").GetComponent<Command_storage>().delete_cm(this.index);
    }

    public void show_edit_cm()
    {
        GameObject.Find("app").GetComponent<Command_storage>().show_edit_command(this.index);
    }

    public void show_add_cm()
    {
        GameObject.Find("app").GetComponent<Command_storage>().show_new_command(this.txt_chat.text);
    }

    public void show_add_cm_whit_pater()
    {
        string id_chat = this.idata_chat["id"].ToString();
        GameObject.Find("app").GetComponent<Command_storage>().show_add_command_with_pater(this.txt_chat.text, id_chat);
    }

    public void btn_play_command()
    {
        GameObject.Find("app").GetComponent<Command_storage>().play_test_command(this.index);
    }

    public void btn_view_chat_log()
    {
        if (this.idata_chat != null)
        {
            if (idata_chat["msg"] != null)
                GameObject.Find("app").GetComponent<Command>().act_chat(this.idata_chat);
            else
                GameObject.Find("app").GetComponent<App>().player_music.act_play_data(this.idata_chat, true);
        }
        else
        {
            GameObject.Find("app").GetComponent<Command>().act_chat(this.idata_chat);
        }
    }

}
