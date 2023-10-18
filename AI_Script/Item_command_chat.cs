using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Item_command_chat : MonoBehaviour
{
    public Image icon;
    public Text txt_chat;
    public GameObject btn_add_app;
    public IDictionary idata_chat;

    private UnityAction act_click;
    private UnityAction act_add;

    public void on_click_item()
    {
        if (this.act_click != null) this.act_click();
    }

    public void on_click_add()
    {
        if (this.act_add != null) this.act_add();
    }

    public void set_act_click(UnityAction u_act)
    {
        this.act_click = u_act;
    }

    public void set_act_add(UnityAction u_act)
    {
        this.act_add = u_act;
    }
}
