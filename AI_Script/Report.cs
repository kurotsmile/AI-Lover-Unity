using Carrot;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Ai_Chat_Report_Data
{
    public Carrot_Rate_user_data user { get; set; }
    public string comment { get; set; }
    public string date { get; set; }
}

public class Report : MonoBehaviour
{
    public App app;
    private string id_chat="";
    public GameObject prefab_item_panel_btn;

    [Header("Asset Icon")]
    public Sprite icon_report;
    public Sprite icon_report_chat;
    public Sprite icon_report_music;
    public Sprite icon_report_other;

    private Carrot_Box_Item item_other_report;
    private Carrot_Box box_report;

    private int index_report_edit = -1;

    public void show(Item_command_chat item_log)
    {
        this.id_chat = item_log.idata_chat["id"].ToString();

        this.box_report = this.GetComponent<App>().carrot.Create_Box("box_report");
        box_report.set_icon(this.icon_report);
        box_report.set_title(PlayerPrefs.GetString("report_title", "Report"));

        Carrot.Carrot_Box_Item item_msg = box_report.create_item();
        item_msg.set_type(Carrot.Box_Item_Type.box_value_txt);
        item_msg.check_type();

        item_msg.set_icon(this.icon_report_chat);
        item_msg.set_title("Report");
        item_msg.set_tip("Report bugs on this chat");
        item_msg.set_lang_data("report_title", "report_msg");

        item_msg.set_val(item_log.txt_chat.text);
        item_msg.load_lang_data();

        this.item_other_report = box_report.create_item();
        this.item_other_report.set_icon(this.icon_report_other);
        this.item_other_report.set_type(Carrot.Box_Item_Type.box_value_input);
        this.item_other_report.check_type();
        this.item_other_report.set_title("Detailed description");
        this.item_other_report.set_tip("Describe specifically the error you're having so that we can fix it");
        this.item_other_report.set_lang_data("report_other", "report_other_tip");
        this.item_other_report.load_lang_data();

        Carrot_Box_Btn_Panel box_panel_btn = this.box_report.create_panel_btn();

        Carrot.Carrot_Button_Item btn_done = box_panel_btn.create_btn("btn_done");
        btn_done.set_icon(this.GetComponent<App>().carrot.icon_carrot_done);
        btn_done.set_bk_color(this.GetComponent<App>().carrot.color_highlight);
        btn_done.set_label_color(Color.white);
        btn_done.txt_val.text = PlayerPrefs.GetString("done", "Done");
        btn_done.set_act_click(() => done());

        Carrot.Carrot_Button_Item btn_cancel = box_panel_btn.create_btn("btn_cancel");
        btn_cancel.set_icon(this.GetComponent<App>().carrot.icon_carrot_cancel);
        btn_cancel.set_bk_color(this.GetComponent<App>().carrot.color_highlight);
        btn_cancel.set_label_color(Color.white);
        btn_cancel.txt_val.text = PlayerPrefs.GetString("cancel", "Cancel");
        btn_cancel.set_act_click(() => box_report.close());
    }

    public void done()
    {
        this.app.carrot.show_loading();
        this.app.carrot.server.Get_doc_by_path("chat-" + this.app.carrot.lang.get_key_lang(), this.id_chat, Get_data_chat_done, Get_data_chat_fail);
    }

    private void Get_data_chat_done(string s_data)
    {
        Debug.Log("Get_data_chat_done:" + s_data);
        this.app.carrot.hide_loading();
        Fire_Document fd = new(s_data);
        IDictionary data_chat = fd.Get_IDictionary();
        IList reports;
        if (data_chat["reports"] != null) reports = (IList)data_chat["reports"];
        else reports = (IList)Json.Deserialize("[]");

        Ai_Chat_Report_Data report_chat = new Ai_Chat_Report_Data();
        report_chat.comment = this.item_other_report.get_val();
        report_chat.date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        if (this.app.carrot.user.get_id_user_login() != "")
        {
            Carrot_Rate_user_data user_login = new Carrot_Rate_user_data();
            user_login.name = this.app.carrot.user.get_data_user_login("name");
            user_login.id = this.app.carrot.user.get_id_user_login();
            user_login.lang = this.app.carrot.user.get_lang_user_login();
            user_login.avatar = this.app.carrot.user.get_data_user_login("avatar");
            report_chat.user = user_login;
        }

        if (this.index_report_edit != -1)
            reports[this.index_report_edit] = report_chat;
        else
            reports.Add(report_chat);

        this.app.carrot.log("Index Report:" + this.index_report_edit);
        data_chat["reports"] = reports;
        IDictionary chat_data = (IDictionary)Json.Deserialize(JsonConvert.SerializeObject(data_chat));
        string s_json = this.app.carrot.server.Convert_IDictionary_to_json(chat_data);
        this.app.carrot.server.Add_Document_To_Collection("chat-" + this.app.carrot.lang.get_key_lang(),this.id_chat, s_json, Submit_Report_done, Submit_Report_fail);
    }

    private void Submit_Report_done(string s_data)
    {
        this.GetComponent<App>().carrot.show_msg(PlayerPrefs.GetString("report_title", "Report"), PlayerPrefs.GetString("report_success"));
        if (this.box_report != null) this.box_report.close();
    }

    private void Submit_Report_fail(string s_error)
    {
        this.app.carrot.show_msg(PlayerPrefs.GetString("report_title", "Report"), s_error, Msg_Icon.Error);
        if (this.box_report != null) this.box_report.close();
    }

    private void Get_data_chat_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.carrot.show_msg(PlayerPrefs.GetString("report_title", "Report"), string.Format("Document {0} does not exist!",this.id_chat), Msg_Icon.Error);
        if (this.box_report != null) this.box_report.close();
    }

    public void show_list_report(IList list_report)
    {
        Carrot_Box box_list_report = this.app.carrot.Create_Box("report_list");
        box_list_report.set_icon(this.app.command.sp_icon_info_report_chat);
        box_list_report.set_title(PlayerPrefs.GetString("report_title", "Report"));

        for (int i = 0; i < list_report.Count; i++)
        {
            IDictionary report_data = (IDictionary)list_report[i];
            Carrot_Box_Item item_report = box_list_report.create_item("report_" + i);
            item_report.set_icon(this.app.carrot.icon_carrot_bug);
            if (report_data["comment"] != null) item_report.set_title(report_data["comment"].ToString());
            if (report_data["date"] != null) item_report.set_tip(report_data["date"].ToString());
        }
    }
}
