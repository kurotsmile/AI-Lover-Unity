using Carrot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Act_List_Func {Add_Box_Item,View_Category,Select_Dance_Animation}
public class character_actions : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("Character_Actions")]
    public int index_product_buy_act = 8;
    public int index_product_buy_all_act = 9;
    public string[] list_anim_act_defalt;

    private IList list_category_animations;
    private Carrot_Box box_list;
    private IList list_name_animation;
    private IDictionary obj_list_dance_animation;
    private Carrot_Box_Item item_box_temp=null;
    private Dictionary<string, AnimationClip> local_animation_clips;

    private string s_act_animation_dance = "002_SIM01_Final";
    private Act_List_Func func;

    public void On_load()
    {
        this.s_act_animation_dance = PlayerPrefs.GetString("act_animation_dance", "002_SIM01_Final");
    }

    public void btn_show_category(Carrot_Box_Item item_set_data)
    {
        this.func = Act_List_Func.Add_Box_Item;
        this.item_box_temp = item_set_data;
        if (this.Ensure_local_animation_data_loaded())
            this.box_list_category();
    }

    public void show_list_category()
    {
        this.func = Act_List_Func.View_Category;
        this.btn_show_category(null);
    }

    private RuntimeAnimatorController Get_base_runtime_animator_controller()
    {
        Animator animator = this.app != null && this.app.get_character() != null ? this.app.get_character().get_anim_character() : null;
        if (animator == null) return null;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller is AnimatorOverrideController overrideController && overrideController.runtimeAnimatorController != null)
            return overrideController.runtimeAnimatorController;

        return controller;
    }

    private bool Is_dance_animation_name(string s_name_animation)
    {
        if (string.IsNullOrEmpty(s_name_animation)) return false;
        string s_name_lower = s_name_animation.ToLowerInvariant();
        return s_name_lower.Contains("dance") || s_name_lower.EndsWith("_final") || s_name_lower.Contains("_sim") || s_name_lower.Contains("_sak") || s_name_lower.Contains("_not");
    }

    private IDictionary Create_animation_category(string s_name, List<string> list_animation_name)
    {
        IDictionary data_category = Json.Deserialize("{}") as IDictionary;
        data_category["name"] = s_name;

        IList list_animation = Json.Deserialize("[]") as IList;
        for (int i = 0; i < list_animation_name.Count; i++)
        {
            IDictionary data_animation = Json.Deserialize("{}") as IDictionary;
            data_animation["name"] = list_animation_name[i];
            data_animation["buy"] = "0";
            list_animation.Add(data_animation);
        }

        data_category["data"] = list_animation;
        return data_category;
    }

    private bool Ensure_local_animation_data_loaded()
    {
        if (this.list_category_animations != null && this.local_animation_clips != null && this.local_animation_clips.Count > 0) return true;

        RuntimeAnimatorController controller = this.Get_base_runtime_animator_controller();
        if (controller == null)
        {
            Debug.LogError("Animator_npc.controller is not ready to build local action list");
            return false;
        }

        this.local_animation_clips = new Dictionary<string, AnimationClip>();
        this.list_name_animation = Json.Deserialize("[]") as IList;
        this.list_category_animations = Json.Deserialize("[]") as IList;
        this.obj_list_dance_animation = null;

        List<string> list_all_animation_name = new List<string>();
        List<string> list_dance_animation_name = new List<string>();

        AnimationClip[] list_clip = controller.animationClips;
        for (int i = 0; i < list_clip.Length; i++)
        {
            AnimationClip clip = list_clip[i];
            if (clip == null || string.IsNullOrEmpty(clip.name)) continue;
            if (this.local_animation_clips.ContainsKey(clip.name)) continue;

            this.local_animation_clips.Add(clip.name, clip);
            this.list_name_animation.Add(clip.name);
            list_all_animation_name.Add(clip.name);
            if (this.Is_dance_animation_name(clip.name)) list_dance_animation_name.Add(clip.name);
        }

        if (list_all_animation_name.Count == 0)
        {
            Debug.LogError("No local animations found in Animator_npc.controller");
            return false;
        }

        IDictionary all_category = this.Create_animation_category("All", list_all_animation_name);
        this.list_category_animations.Add(all_category);

        if (list_dance_animation_name.Count > 0)
        {
            this.obj_list_dance_animation = this.Create_animation_category("Dance", list_dance_animation_name);
            this.list_category_animations.Add(this.obj_list_dance_animation);
        }
        else
        {
            this.obj_list_dance_animation = all_category;
        }

        if (!this.local_animation_clips.ContainsKey(this.s_act_animation_dance))
        {
            IList list_dance_data = this.obj_list_dance_animation["data"] as IList;
            if (list_dance_data != null && list_dance_data.Count > 0)
                this.s_act_animation_dance = ((IDictionary)list_dance_data[0])["name"].ToString();
            else
                this.s_act_animation_dance = list_all_animation_name[0];

            PlayerPrefs.SetString("act_animation_dance", this.s_act_animation_dance);
        }

        Debug.Log("Loaded local animation list from Animator_npc.controller (" + list_all_animation_name.Count + " clips)");
        return true;
    }

    private void box_list_category()
    {
        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.Create_Box();
        this.box_list.set_title(PlayerPrefs.GetString("act","List Action Category"));
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

        if (this.func != Act_List_Func.Select_Dance_Animation)
        {
            Carrot_Box_Btn_Item btn_category = this.box_list.create_btn_menu_header(this.app.carrot.icon_carrot_all_category);
            btn_category.set_act(() => this.btn_show_category(this.item_box_temp));
        }

        IList list_animations = (IList)data_category["data"];
        for (int i = 0; i <list_animations.Count; i++)
        {
            IDictionary data_item_anim = (IDictionary)list_animations[i];
            var s_name = data_item_anim["name"].ToString();
            Carrot_Box_Item item_anim = this.box_list.create_item("item_anim_" + i);
            item_anim.set_title(data_item_anim["name"].ToString());
            item_anim.set_tip(data_item_anim["name"].ToString());
            item_anim.set_icon(this.app.command_storage.sp_icon_action);
            item_anim.set_act(() => this.act_sel_action(s_name));

            if (this.func == Act_List_Func.Select_Dance_Animation)
            {
                if(this.s_act_animation_dance== s_name)
                {
                    Carrot_Box_Btn_Item btn_sel = item_anim.create_item();
                    btn_sel.set_icon(this.app.carrot.icon_carrot_done);
                    btn_sel.set_color(this.app.carrot.color_highlight);
                    btn_sel.set_act(() => this.act_test_anim(s_name));
                    Destroy(btn_sel.GetComponent<Button>());
                }
            }

            Carrot_Box_Btn_Item btn_test = item_anim.create_item();
            btn_test.set_icon(this.app.player_music.icon_play);
            btn_test.set_color(this.app.carrot.color_highlight);
            btn_test.set_act(() => this.act_test_anim(s_name));
        }
        this.box_list.update_color_table_row();
    }

    private void act_sel_action(string s_name_anim)
    {
        this.app.carrot.play_sound_click();
        if (this.func == Act_List_Func.Add_Box_Item)
        {
            if (this.item_box_temp != null)
            {
                this.item_box_temp.set_type(Box_Item_Type.box_value_txt);
                this.item_box_temp.check_type();
                this.item_box_temp.set_val(s_name_anim);
                if (this.box_list != null) this.box_list.close();
            }
        }
        if (this.func == Act_List_Func.View_Category)
        {
            this.act_test_anim(s_name_anim);
        }

        if (this.func == Act_List_Func.Select_Dance_Animation)
        {
            this.s_act_animation_dance = s_name_anim;
            PlayerPrefs.SetString("act_animation_dance", s_name_anim);
            if (this.app.player_music.sound_music.isPlaying) this.play_act_anim(s_name_anim);
            if (this.box_list != null) this.box_list.close();
        }
        
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
        if (!this.Ensure_local_animation_data_loaded()) return;

        this.app.get_character().unpause_ani();
        Animator animator = this.app.get_character().get_anim_character();
        if (this.check_anim_default(s_name_animation))
        {
            this.app.get_character().play_ani(s_name_animation);
        }
        else
        {
            if (this.local_animation_clips != null && this.local_animation_clips.TryGetValue(s_name_animation, out AnimationClip animClip))
            {
                RuntimeAnimatorController controller = this.Get_base_runtime_animator_controller();
                if (controller != null)
                {
                    AnimatorOverrideController overrideController = new AnimatorOverrideController(controller);
                    overrideController["Run"] = animClip;
                    animator.runtimeAnimatorController = overrideController;
                    animator.Play("Run");
                    Debug.Log("play_act_anim local:" + s_name_animation);
                }
            }
            else
            {
                Debug.LogError("Local animation clip not found: " + s_name_animation);
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
        if (this.GetComponent<Carrot_DeviceOrientationChange>().Get_status_portrait()) this.app.panel_menu_right.SetActive(false);
    }

    public IList get_list_all_name_animations()
    {
        this.Ensure_local_animation_data_loaded();
        return this.list_name_animation;
    }

    public void check_buy_success_action()
    {
    }

    public void show_list_animtion_dance()
    {
        this.func = Act_List_Func.Select_Dance_Animation;
        this.app.carrot.play_sound_click();
        if (!this.Ensure_local_animation_data_loaded()) return;
        if(this.obj_list_dance_animation != null)
        {
            this.box_list_animation(this.obj_list_dance_animation);
        }
    }

    public void play_animation_dance()
    {
        this.play_act_anim(this.s_act_animation_dance);
    }
}
