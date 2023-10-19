using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Command_Live : MonoBehaviour
{

    [Header("Main Obj")]
    public App app;

    [Header("Ui Live")]
    public Image img_icon_live_chat_box;
    public GameObject obj_btn_play_chat_live;

    [Header("Live Obj")]
    public Sprite icon;

    private int index_cur_cm = 0;
    private int lenth_cm = 0;
    private bool is_active = false;

    private Item_command_chat item_live_temp;

    public void on_live()
    {
        this.is_active = true;
        this.obj_btn_play_chat_live.SetActive(true);
        this.app.command.clear_log_chat();
    }

    public void off_live()
    {
        this.is_active = false;
        this.obj_btn_play_chat_live.SetActive(false);
    }

    public void play()
    {
        this.index_cur_cm = 0;
        foreach(Transform tr in this.app.command.area_body_log_command)
        {
            if(tr.gameObject.name== "user_chat"||tr.gameObject.name== "music_item") Destroy(tr.gameObject);
        }
        this.lenth_cm = this.app.command.area_body_log_command.childCount;
        this.act_live_chat(this.index_cur_cm);
        this.img_icon_live_chat_box.sprite = this.app.player_music.icon_pause;
    }

    public void next()
    {
        this.index_cur_cm++;
        if(this.index_cur_cm >= this.lenth_cm-2) this.index_cur_cm = 0;
        this.act_live_chat(this.index_cur_cm);
    }

    public void stop() 
    {
        this.img_icon_live_chat_box.sprite = this.app.player_music.icon_play;
    }

    private void act_live_chat(int index)
    {
        Item_command_chat live_item = this.app.command.area_body_log_command.GetChild(index).GetComponent<Item_command_chat>();
        this.app.command.act_chat(live_item.idata_chat,false);
    }

    public bool get_status_active()
    {
        return this.is_active;
    }

    public void show_edit_item_chat_live(Item_command_chat item_c)
    {
        this.item_live_temp = item_c;
        this.app.command_storage.show_edit_live(item_c.idata_chat);
    }

    public void update_data_item_chat_live(IDictionary idata)
    {
        this.item_live_temp.idata_chat = idata;
        this.item_live_temp.txt_chat.text = idata["msg"].ToString();
    }
}
