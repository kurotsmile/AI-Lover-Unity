using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Item_character : MonoBehaviour
{
    public string s_id;
    public Image bk;
    public Image icon;
    public GameObject btn_buy;
    public bool is_free;
    private UnityAction act;

    public void click()
    {
        if (act != null) this.act();
    }

    public void set_act(UnityAction act_set)
    {
        this.act = act_set;
    }

    public void set_icon(Sprite icon_set)
    {
        this.icon.sprite = icon_set;
    }

    public void set_color_bk(Color32 color_set)
    {
        this.bk.color = color_set;
    }
}
