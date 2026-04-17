using Carrot;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public enum Size_Character { small, large }
public enum Type_Rotate_Character {statics,touch,sensor}
public class Environment : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;
    public Camera cam;
    public MouseOrbitImproved mouseOrbit_Improved;

    public Size_Character size_character;
    private Type_Rotate_Character rotate_character;

    [Header("Ui Emp")]
    public Image[] btn_rotate_type;
    public Image img_color_bk_list;
    public Image img_color_bk_mix;
    public Slider slider_zoom;

    private Carrot.Carrot_Box box_list;
    private string s_color_bk = "#FFFFFF";

    private int index_type_rotate_character = 0;
    private bool is_view_portrait = true;

    public void On_start()
    {
        Color32 color_default = this.app.color_bk_default;
        this.s_color_bk = PlayerPrefs.GetString("s_color_bk",ColorUtility.ToHtmlStringRGBA(color_default));
        this.index_type_rotate_character = PlayerPrefs.GetInt("type_rotate_character", 0);

        this.set_color_bk(this.app.carrot.theme.Get_color_by_string(this.s_color_bk));

        this.clear_legacy_background_data();
        this.clear_legacy_floor_data();

        this.check_index_func_rotate_character();
        this.check_func_rotate_character();
    }

    public void On_load()
    {
        if (this.is_view_portrait)
            this.slider_zoom.value=PlayerPrefs.GetFloat("bk_zoom_view_portrait", 10f);
        else
            this.slider_zoom.value=PlayerPrefs.GetFloat("bk_zoom_view_landspace",5f);

        this.check_index_func_rotate_character();
        this.act_sel_type_rotate_character(this.index_type_rotate_character);
    }

    public void change_zoom_view()
    {
        this.cam.fieldOfView = this.slider_zoom.value;
        if(this.is_view_portrait)
            PlayerPrefs.SetFloat("bk_zoom_view_portrait", this.slider_zoom.value);
        else
            PlayerPrefs.SetFloat("bk_zoom_view_landspace", this.slider_zoom.value);
    }

    public void change_scene_rotation(bool is_portrait)
    {
        this.is_view_portrait = is_portrait;
        this.mouseOrbit_Improved.is_view_portrait = is_portrait;
        if (this.size_character == Size_Character.large)
        {
            if (is_portrait)
            {
                this.cam.transform.localPosition = new Vector3(0f, 1.8f, 10f);
                this.cam.transform.localRotation = Quaternion.Euler(2f, 180f, 0f);
                this.cam.fieldOfView = PlayerPrefs.GetFloat("bk_zoom_view_portrait", 20f);
            }
            else
            {
                this.cam.transform.localPosition = new Vector3(1f, 3f, 14f);
                this.cam.transform.rotation = Quaternion.Euler(8, 180, 360);
                this.cam.fieldOfView = PlayerPrefs.GetFloat("bk_zoom_view_landspace", 20f);
            }
        }

        if (this.size_character == Size_Character.small)
        {
            if(is_portrait)
                this.cam.transform.localPosition = new Vector3(0, 3.32f, 14f);
            else
                this.cam.transform.localPosition = new Vector3(1f, 3f, 14f);

            this.cam.fieldOfView = PlayerPrefs.GetFloat("bk_zoom_view", 20f);;
        }
        this.slider_zoom.value = this.cam.fieldOfView;
    }

    public void btn_show_list_color_bk()
    {
        this.app.carrot.theme.Show_box_list_item_color(act_sel_color_bk);
    }

    private void act_sel_color_bk(Color32 color_set)
    {
        this.s_color_bk = ColorUtility.ToHtmlStringRGBA(color_set);
        PlayerPrefs.SetString("s_color_bk", this.s_color_bk);
        this.set_color_bk(color_set);
    }

    public void btn_show_mix_color_bk()
    {
        this.app.carrot.theme.Show_mix_color(act_sel_color_bk,this.s_color_bk);
    }

    private void set_color_bk(Color32 color_set)
    {
        this.img_color_bk_list.color = color_set;
        this.img_color_bk_mix.color = color_set;
        this.cam.backgroundColor = color_set;
    }

    public void sel_type_rotate_character(int type)
    {
        PlayerPrefs.SetInt("type_rotate_character", type);
        this.index_type_rotate_character = type;
        this.check_index_func_rotate_character();
        this.act_sel_type_rotate_character(type);
    }

    private void act_sel_type_rotate_character(int type)
    {
        this.btn_rotate_type[0].color = Color.black;
        this.btn_rotate_type[1].color = Color.black;
        this.btn_rotate_type[2].color = Color.black;
        this.btn_rotate_type[type].color = this.app.carrot.color_highlight;
        this.check_func_rotate_character();
    }

    private void check_index_func_rotate_character()
    {
        if (this.index_type_rotate_character == 0) this.rotate_character =Type_Rotate_Character.statics;
        if (this.index_type_rotate_character == 1) this.rotate_character =Type_Rotate_Character.touch;
        if (this.index_type_rotate_character == 2) this.rotate_character =Type_Rotate_Character.sensor;
    }

    private void check_func_rotate_character()
    {

        if (this.rotate_character == Type_Rotate_Character.statics)
        {
            this.cam.transform.localPosition = new Vector3(0f, 1.8f, 10f);
            this.cam.transform.localRotation = Quaternion.Euler(2f, 180f, 0f);
        }
        this.mouseOrbit_Improved.set_mode(this.rotate_character);
    }

    public void enable_model_rotate_character(bool is_active)
    {

    }

    public void clear_legacy_background_data()
    {
        this.app.carrot.get_tool().delete_file("background");
        PlayerPrefs.DeleteKey("color_opacity_bk");
        PlayerPrefs.DeleteKey("s_data_json_bk_offline");
    }

    public void clear_legacy_floor_data()
    {
        PlayerPrefs.DeleteKey("s_id_floor");
        PlayerPrefs.DeleteKey("s_data_json_floor_offline");
    }
}
