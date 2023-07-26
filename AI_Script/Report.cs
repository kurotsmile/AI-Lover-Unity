using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Report : MonoBehaviour
{
    private string id_chat="";
    private string type_report = "";
    public GameObject prefab_item_panel_btn;

    [Header("Asset Icon")]
    public Sprite icon_report;
    public Sprite icon_report_chat;
    public Sprite icon_report_music;
    public Sprite icon_report_msg;
    public Sprite icon_report_type;
    public Sprite icon_report_other;

    private Carrot.Carrot_Box_Item item_type_report;
    private Carrot.Carrot_Box_Item item_other_report;
    private Carrot.Carrot_Box_Item item_limit_report;
    private Carrot.Carrot_Box box_report;

    public void show(Item_command_chat item_log)
    {
        this.id_chat = item_log.idata_chat["id"].ToString();

        this.box_report=this.GetComponent<App>().carrot.Create_Box("box_report");
        box_report.set_icon(this.icon_report);
        box_report.set_title(PlayerPrefs.GetString("report_title", "Report"));

        Carrot.Carrot_Box_Item item_msg = box_report.create_item();
        item_msg.set_type(Carrot.Box_Item_Type.box_value_txt);
        if (item_log.is_music)
        {
            item_msg.set_icon(this.icon_report_music);
            item_msg.set_title("Report");
            item_msg.set_tip("Report a bug on this song");
            item_msg.set_lang_data("report_title","report_music");
            this.type_report = "music";
        }
        else
        {
            item_msg.set_icon(this.icon_report_chat);
            item_msg.set_title("Report");
            item_msg.set_tip("Report bugs on this chat");
            item_msg.set_lang_data("report_title","report_msg");
            this.type_report = "chat";
        }
        item_msg.set_val(item_log.txt_chat.text);
        item_msg.load_lang_data();

        this.item_type_report= box_report.create_item();
        this.item_type_report.set_icon(this.icon_report_type);
        this.item_type_report.set_type(Carrot.Box_Item_Type.box_value_dropdown);
        this.item_type_report.set_title("Error case");
        this.item_type_report.set_tip("The character's actions follow the response text");
        this.item_type_report.set_lang_data("report_type", "report_type_tip");
        this.item_type_report.load_lang_data();
        this.item_type_report.dropdown_val.ClearOptions();
        for (int i = 0; i < 4; i++) this.item_type_report.dropdown_val.options.Add(new Dropdown.OptionData() { text = PlayerPrefs.GetString("report_error_" + i) });
        this.item_type_report.dropdown_val.onValueChanged.AddListener(this.act_change_val_type);

        this.item_limit_report = box_report.create_item("item_limit");
        this.item_limit_report.set_type(Carrot.Box_Item_Type.box_value_slider);
        this.item_limit_report.set_icon(this.GetComponent<App>().setting.sp_icon_chat_limit);
        this.item_limit_report.set_title("Limit vulgarity and sex");
        this.item_limit_report.set_tip("Set limits for children");
        this.item_limit_report.set_lang_data("report_limit_chat", "chat_limit_tip");
        this.item_limit_report.load_lang_data();
        this.item_limit_report.set_fill_color(this.GetComponent<App>().carrot.color_highlight);
        this.item_limit_report.slider_val.wholeNumbers = true;
        this.item_limit_report.slider_val.minValue = 1;
        this.item_limit_report.slider_val.maxValue = 4;
        this.item_limit_report.set_val(this.GetComponent<App>().setting.get_limit_chat().ToString());
        this.item_limit_report.slider_val.onValueChanged.AddListener(this.act_change_value_limit);

        this.item_other_report=box_report.create_item();
        this.item_other_report.set_icon(this.icon_report_other);
        this.item_other_report.set_type(Carrot.Box_Item_Type.box_value_input);
        this.item_other_report.set_title("Detailed description");
        this.item_other_report.set_tip("Describe specifically the error you're having so that we can fix it");
        this.item_other_report.set_lang_data("report_other", "report_other_tip");
        this.item_other_report.load_lang_data();

        GameObject box_panel_btn=this.box_report.add_item(this.prefab_item_panel_btn);

        Carrot.Carrot_Button_Item btn_done=this.GetComponent<App>().carrot.create_button("Done", box_panel_btn.transform);
        btn_done.set_icon(this.GetComponent<App>().carrot.icon_carrot_done);
        btn_done.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        btn_done.set_bk_color(this.GetComponent<App>().carrot.color_highlight);
        btn_done.set_label_color(Color.white);
        btn_done.txt_val.text = PlayerPrefs.GetString("done", "Done");
        btn_done.set_act_click(() => done());

        Carrot.Carrot_Button_Item btn_cancel = this.GetComponent<App>().carrot.create_button("Cancel",box_panel_btn.transform);
        btn_cancel.set_icon(this.GetComponent<App>().carrot.icon_carrot_cancel);
        btn_cancel.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        btn_cancel.set_bk_color(this.GetComponent<App>().carrot.color_highlight);
        btn_cancel.set_label_color(Color.white);
        btn_cancel.txt_val.text = PlayerPrefs.GetString("cancel", "Cancel");
        btn_cancel.set_act_click(() => box_report.close());

        this.act_change_val_type(0);
    }


    private void act_change_value_limit(float val_limit)
    {
        this.item_limit_report.set_tip(this.GetComponent<App>().setting.get_label_limit(int.Parse(val_limit.ToString())));
    }

    public void done()
    {
        WWWForm frm =this.GetComponent<App>().frm_act("send_report");
        frm.AddField("id_chat", this.id_chat);
        if (this.type_report == "chat")
        {
            if (this.item_type_report.get_val() == "0") frm.AddField("sel_report", 1);
            if (this.item_type_report.get_val() == "1") frm.AddField("sel_report", 2);
            if (this.item_type_report.get_val() == "2") frm.AddField("sel_report", 3);
            if (this.item_type_report.get_val() == "3") frm.AddField("sel_report", 4);
            frm.AddField("type_report", "1");
            if (this.item_type_report.get_val() =="2")
                frm.AddField("value_report", this.item_limit_report.get_val());
            else
                frm.AddField("value_report", this.item_other_report.get_val());
        }
        else
        {
            if (this.item_type_report.get_val() == "0") frm.AddField("sel_report", 5);
            if (this.item_type_report.get_val() == "1") frm.AddField("sel_report", 6);
            if (this.item_type_report.get_val() == "2") frm.AddField("sel_report", 7);
            frm.AddField("value_report", this.item_other_report.get_val());
            frm.AddField("type_report", "0");
        }
        this.GetComponent<App>().carrot.show_msg(PlayerPrefs.GetString("report_title", "Report"), PlayerPrefs.GetString("report_success"));
        if (this.box_report != null) this.box_report.close();
    }

    private void act_change_val_type(int index)
    {
        if (index == 2)
            this.item_limit_report.gameObject.SetActive(true);
        else
            this.item_limit_report.gameObject.SetActive(false);
    }

}
