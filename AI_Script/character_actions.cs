using Carrot;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class character_actions : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("Character_Actions")]
    public string url = "";
    public int index_product_buy_act = 8;
    public int index_product_buy_all_act = 9;
    public string[] list_anim_act_defalt;
    private AssetBundle bundle;
    private IList list_animations;
    private IList list_category_animations;
    private Carrot_Box box_list;
    private IList list_name_animation;
    private string s_name_animation_by_temp = "";
    private Carrot_Box_Item item_box_temp=null;

    public void btn_show_category(Carrot_Box_Item item_set_data)
    {
        this.item_box_temp = item_set_data;
        if (this.list_category_animations == null)
            StartCoroutine(this.DownloadAndLoadCaetgoryAndAnimation(()=>this.box_list_category()));
        else
            this.box_list_category();
    }

    public void show_list_category()
    {
        this.btn_show_category(null);
    }

    IEnumerator DownloadAndLoadCaetgoryAndAnimation(UnityAction act_call_back)
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url,1,0))
        {
            this.app.carrot.show_loading();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                this.app.carrot.hide_loading();
                this.bundle = DownloadHandlerAssetBundle.GetContent(request);

                TextAsset jsonFile = bundle.LoadAsset<TextAsset>("data_animations");
                string jsonString = jsonFile.text;
                this.list_category_animations = (IList)Json.Deserialize(jsonString);
                this.list_name_animation = (IList)Json.Deserialize("[]");
                for (int i = 0; i < list_category_animations.Count; i++)
                {
                    IDictionary data_item_anim = (IDictionary)list_category_animations[i];
                    IList data_animations = (IList)data_item_anim["data"];
                    for(int y = 0; y < data_animations.Count; y++)
                    {
                        IDictionary data_anim = (IDictionary) data_animations[y];
                        this.list_name_animation.Add(data_anim["name"].ToString());
                    }
                }
                Debug.Log("Download Data Category and list Animation");
                if (act_call_back != null) act_call_back();
            }
            else
            {
                this.app.carrot.hide_loading();
                Debug.LogError(request.error);
            }
        }
    }

    private void box_list_category()
    {
        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.Create_Box();
        this.box_list.set_title("List Action Category");
        this.box_list.set_icon(this.app.command_storage.sp_icon_action);

        string s_action = PlayerPrefs.GetString("act", "Action");

        for (int i = 0; i < this.list_category_animations.Count; i++)
        {
            IDictionary data_item_anim = (IDictionary)this.list_category_animations[i];
            IList data_animations = (IList)data_item_anim["data"];
            var s_name = data_item_anim["name"].ToString();
            Carrot_Box_Item item_anim = this.box_list.create_item("item_cat_" + i);
            item_anim.set_title(data_item_anim["name"].ToString());
            item_anim.set_tip(data_animations.Count+" "+ s_action);
            item_anim.set_icon(this.app.carrot.icon_carrot_all_category);
            item_anim.set_act(() => this.sel_category(data_item_anim));
        }
    }

    private void sel_category(IDictionary data_anim)
    {
        this.box_list_animation(data_anim);
    }

    private void box_list_animation(IDictionary data_category)
    {
        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.Create_Box();
        this.box_list.set_title(data_category["name"].ToString());
        this.box_list.set_icon(this.app.command_storage.sp_icon_action);

        Carrot_Box_Btn_Item btn_category = this.box_list.create_btn_menu_header(this.app.carrot.icon_carrot_all_category);
        btn_category.set_act(()=>this.btn_show_category(this.item_box_temp));

        bool is_unlock_animation = this.app.setting.check_buy_product(this.index_product_buy_all_act);
        this.list_animations = (IList)data_category["data"];
        for (int i = 0; i < this.list_animations.Count; i++)
        {
            IDictionary data_item_anim = (IDictionary)this.list_animations[i];
            var s_name = data_item_anim["name"].ToString();
            Carrot_Box_Item item_anim = this.box_list.create_item("item_anim_" + i);
            item_anim.set_title(data_item_anim["name"].ToString());
            item_anim.set_tip(data_item_anim["name"].ToString());
            item_anim.set_icon(this.app.command_storage.sp_icon_action);

            bool is_used = false;

            if (this.app.carrot.model_app == ModelApp.Publish)
            {
                if (is_unlock_animation)
                {
                    is_used = true;
                }
                else
                {
                    if (data_item_anim["buy"].ToString() != "0")
                    {
                        if(PlayerPrefs.GetInt("is_user_act_" + s_name) == 1)
                        {
                            is_used = true;
                        }
                        else
                        {
                            is_used = false;
                        }
                    }
                    else
                    {
                        is_used = true;
                    }
                }
            }
            else
            {
                is_used = true;
            }

            if (is_used)
            {
                if(this.item_box_temp!=null)
                    item_anim.set_act(() => this.act_sel_action(s_name));
                else
                    item_anim.set_act(() => this.act_test_anim(s_name));
            }
            else
            {
                Carrot_Box_Btn_Item btn_buy = item_anim.create_item();
                btn_buy.set_icon(this.app.setting.sp_icon_buy);
                btn_buy.set_color(this.app.carrot.color_highlight);
                Destroy(btn_buy.GetComponent<Button>());
                item_anim.set_act(() => this.act_buy_action(s_name));
            }


            Carrot_Box_Btn_Item btn_test = item_anim.create_item();
            btn_test.set_icon(this.app.player_music.icon_play);
            btn_test.set_color(this.app.carrot.color_highlight);
            btn_test.set_act(() => this.act_test_anim(s_name));
        }
    }

    private void act_sel_action(string s_name_anim)
    {
        this.app.carrot.play_sound_click();
        if (this.item_box_temp != null)
        {
            this.item_box_temp.set_type(Box_Item_Type.box_value_txt);
            this.item_box_temp.check_type();
            this.item_box_temp.set_val(s_name_anim);
        }
 
        if (this.box_list != null) this.box_list.close();
    }

    private void act_buy_action(string s_name_anim)
    {
        this.s_name_animation_by_temp = s_name_anim;
        this.app.buy_product(this.index_product_buy_act);
    }

    private void act_test_anim(string s_name_animation)
    {
        this.show_test();
        this.hide_box();
        this.play_act_anim(s_name_animation);
    }

    public void play_act_anim(string s_name_animation)
    {
        if (s_name_animation=="") return;
        Animator animator = this.app.get_character().get_anim_character();
        if (this.check_anim_default(s_name_animation))
        {
            this.app.get_character().play_ani(s_name_animation);
        }
        else
        {
            if (bundle == null)
            {
                Debug.Log("play_act_anim download:" + s_name_animation);
                StartCoroutine(this.DownloadAndLoadCaetgoryAndAnimation(() => this.play_act_anim(s_name_animation)));
            }
            else
            {
                AnimationClip animClip = bundle.LoadAsset<AnimationClip>(s_name_animation);
                if (animClip != null)
                {
                    AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                    overrideController["Run"] = animClip;
                    animator.runtimeAnimatorController = overrideController;
                    animator.Play("Run");
                    Debug.Log("play_act_anim load:" + s_name_animation);
                }
                else
                {
                    Debug.Log("play_act_anim download:" + s_name_animation);
                    StartCoroutine(this.DownloadAndLoadCaetgoryAndAnimation(() => this.play_act_anim(s_name_animation)));
                }
            }

        }
    }

    public void play_act_anim_by_index_default(int index_anim)
    {
        this.app.get_character().unpause_ani();
        Animator animator = this.app.get_character().get_anim_character();
        animator.Play(this.list_anim_act_defalt[index_anim],0);
    }

    private bool check_anim_default(string s_name_animation)
    {
        for (int i = 0; i < this.list_anim_act_defalt.Length; i++)
        {
            if (this.list_anim_act_defalt[i] == s_name_animation) return true;
        }
        return false;
    }

    public void hide_box()
    {
        if (this.box_list != null) this.box_list.gameObject.SetActive(false);
    }

    public void show_box()
    {
        if (this.box_list != null) this.box_list.gameObject.SetActive(true);
    }

    private void show_test()
    {
        this.app.panel_main.SetActive(true);
        this.app.panel_inp_command_test.SetActive(true);
        this.app.panel_inp_func.SetActive(false);
        this.app.panel_inp_msg.SetActive(false);
        this.app.panel_chat_func.SetActive(false);
        this.app.panel_chat_msg.SetActive(true);
        this.app.command_storage.obj_button_next_command_test.SetActive(false);
        this.app.command_storage.obj_button_prev_command_test.SetActive(false);
        this.app.command_storage.obj_button_prev_command_replay.SetActive(false);
        this.app.command_storage.hide_box_add();
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().get_status_portrait()) this.app.panel_menu_right.SetActive(false);
    }

    public IList get_list_all_name_animations()
    {
        return this.list_name_animation;
    }

    public void check_buy_success_action()
    {
        if (this.s_name_animation_by_temp != "")
        {
            PlayerPrefs.SetInt("is_user_act_" + this.s_name_animation_by_temp, 1);
            this.act_sel_action(this.s_name_animation_by_temp);
            this.s_name_animation_by_temp = "";
        }
    }
}
