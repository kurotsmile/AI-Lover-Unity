using Firebase.Extensions;
using Firebase.Firestore;
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
    public Image img_icon_floor;
    public MeshRenderer mesh_floor;
    public Slider slider_zoom;

    [Header("Background Image App")]
    public Sprite icon_list_background;
    public Image img_background_app;
    public Image img_background_app_thumb;
    public Slider slider_color_bk;
    public GameObject btn_delete_bk;

    [Header("Asset Icon")]
    public Sprite icon_floor;

    private Carrot.Carrot_Box box_list;
    private string s_color_bk = "#FFFFFF";

    private int index_type_rotate_character = 0;
    private bool is_view_portrait = true;

    private QuerySnapshot BackgroundQuerySnapshot;
    private QuerySnapshot FloorQuerySnapshot;
    private string s_id_floor = "";

    public void on_start()
    {
        Color32 color_default = this.app.color_bk_default;
        this.s_color_bk = PlayerPrefs.GetString("s_color_bk",ColorUtility.ToHtmlStringRGBA(color_default));
        this.index_type_rotate_character = PlayerPrefs.GetInt("type_rotate_character", 0);

        this.set_color_bk(this.app.carrot.theme.get_color_by_string(this.s_color_bk));

        this.s_id_floor = PlayerPrefs.GetString("s_id_floor", "");
        if (this.s_id_floor != "") this.sel_bk_floor(this.s_id_floor);
        
        this.load_file_background();

        this.check_index_func_rotate_character();
        this.check_func_rotate_character();
    }

    public void on_load()
    {
        if (this.is_view_portrait)
            this.slider_zoom.value=PlayerPrefs.GetFloat("bk_zoom_view_portrait", 10f);
        else
            this.slider_zoom.value=PlayerPrefs.GetFloat("bk_zoom_view_landspace",5f);

        this.check_index_func_rotate_character();
        this.act_sel_type_rotate_character(this.index_type_rotate_character);

        if (this.s_id_floor != "")
        {
            Sprite sp_icon_floor= this.app.carrot.get_tool().get_sprite_to_playerPrefs(this.s_id_floor);
            this.img_icon_floor.sprite = sp_icon_floor;
        }
    }

    public void load_file_background()
    {
        string name_file_bk;
        if (Application.isEditor)
            name_file_bk = Application.dataPath + "/background";
        else
            name_file_bk = Application.persistentDataPath + "/background";

        if (System.IO.File.Exists(name_file_bk))
        {
            Texture2D load_s01_texture;
            byte[] bytes;
            bytes = System.IO.File.ReadAllBytes(name_file_bk);
            load_s01_texture = new Texture2D(1, 1);
            load_s01_texture.LoadImage(bytes);

            Sprite sprite = Sprite.Create(load_s01_texture, new Rect(0, 0, load_s01_texture.width, load_s01_texture.height), new Vector2(0, 0));
            this.img_background_app.sprite = sprite;
            this.img_background_app_thumb.sprite=sprite;
            this.img_background_app.gameObject.SetActive(true);
            this.btn_delete_bk.SetActive(true);

            float color_opacity_bk= PlayerPrefs.GetFloat("color_opacity_bk", 150);
            this.img_background_app.color = new Color32(225, 225, 225, byte.Parse(color_opacity_bk.ToString()));
            this.slider_color_bk.value = color_opacity_bk;
        }
        else
        {
            this.img_background_app_thumb.sprite = this.icon_list_background;
            this.img_background_app.gameObject.SetActive(false);
            this.btn_delete_bk.SetActive(false);
        }
    }

    public void sel_bk_floor(string s_id_floor)
    {
        Sprite sprite_floor= this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_floor);
        if (sprite_floor != null)
        {
            this.img_icon_floor.sprite = sprite_floor;
            this.mesh_floor.materials[0].mainTexture = sprite_floor.texture;
        }
    }

    public void change_zoom_view()
    {
        this.cam.fieldOfView = this.slider_zoom.value;
        if(this.is_view_portrait)
            PlayerPrefs.SetFloat("bk_zoom_view_portrait", this.slider_zoom.value);
        else
            PlayerPrefs.SetFloat("bk_zoom_view_landspace", this.slider_zoom.value);
    }

    public void change_color_im_bk()
    {
        this.img_background_app.color = new Color32(225, 225, 225,byte.Parse(this.slider_color_bk.value.ToString()));
        PlayerPrefs.SetFloat("color_opacity_bk", this.slider_color_bk.value);
    }

    public void show_list_background_image()
    {
        this.app.carrot.show_loading();
        if (this.BackgroundQuerySnapshot==null)
        {
            CollectionReference IconRef = this.app.carrot.db.Collection("background");
            IconRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                this.BackgroundQuerySnapshot = task.Result;
                act_load_list_background(this.BackgroundQuerySnapshot);
            });
        }
        else
        {
            act_load_list_background(this.BackgroundQuerySnapshot);
        }
    }

    private void act_load_list_background(QuerySnapshot query_background)
    {
        this.app.carrot.hide_loading();
        if (this.box_list != null) this.box_list.close();
        this.box_list = this.app.carrot.show_grid(this.icon_list_background);
        this.box_list.set_title("List Background");

        Carrot.Carrot_Box_Btn_Item btn_freshen = this.box_list.create_btn_menu_header(this.app.carrot.sp_icon_restore);
        btn_freshen.set_act(() => show_list_background_image());

        this.box_list.set_item_size(new Vector2(105, 130));

        foreach (DocumentSnapshot document in query_background)
        {
            IDictionary data_bk = (IDictionary)document.ToDictionary();
            data_bk["id"] = document.Id;
            string s_link_icon = data_bk["icon"].ToString();

            Carrot.Carrot_Box_Item item_bk = this.box_list.create_item(document.Id);

            Sprite pic_background = this.app.carrot.get_tool().get_sprite_to_playerPrefs(document.Id);
            if (pic_background != null)
            {
                item_bk.img_icon.sprite = pic_background;
                item_bk.img_icon.color = Color.white;
            }
            else
            {
                this.app.carrot.get_img_and_save_playerPrefs(s_link_icon, item_bk.img_icon, document.Id);
            }
            item_bk.set_act(() => this.set_background(item_bk.img_icon.sprite.texture));
        }
    }

    private void set_background(Texture2D tex2D)
    {
        this.download_background(tex2D);
        if (this.box_list != null) this.box_list.close();
    }

    private void download_background(Texture2D tex2D)
    {
        this.app.carrot.get_tool().save_file("background", tex2D.EncodeToPNG());
        this.load_file_background();
        this.app.carrot.close();
        this.app.get_character().get_npc().gameObject.SetActive(true);
    }

    public void delete_background_image()
    {
        GameObject.Find("app").GetComponent<App>().carrot.get_tool().delete_file("background");
        this.load_file_background();
    }

    public void show_list_photo_camera()
    {
        this.app.carrot.camera_pro.show_list_img(done_camera);
    }

    public void show_camera()
    {
        this.app.carrot.camera_pro.show_camera(done_camera);
    }

    private void done_camera(Texture2D pic)
    {
        this.img_background_app.sprite = GameObject.Find("app").GetComponent<App>().carrot.get_tool().Texture2DtoSprite(pic);
        GameObject.Find("app").GetComponent<App>().carrot.get_tool().save_file("background", pic.EncodeToPNG());
        this.load_file_background();
        GameObject.Find("app").GetComponent<App>().carrot.close();
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

            this.cam.fieldOfView = PlayerPrefs.GetFloat("bk_zoom_view", 5f);;
        }
        this.slider_zoom.value = this.cam.fieldOfView;
    }

    public void show_list_floor()
    {
        if (this.FloorQuerySnapshot == null)
        {
            Query FloorQuery = this.app.carrot.db.Collection("floor");
            FloorQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                this.FloorQuerySnapshot = task.Result;
                this.act_load_floor(this.FloorQuerySnapshot);
            });
        }
        else
        {
            this.act_load_floor(this.FloorQuerySnapshot);
        }
    }

    private void act_load_floor(QuerySnapshot FQuery)
    {
        if (FQuery.Count > 0)
        {
            if (this.box_list != null) this.box_list.close();
            this.box_list = this.app.carrot.show_grid();
            this.box_list.set_title("List Floor");
            this.box_list.set_icon(this.icon_floor);

            foreach (DocumentSnapshot document_floor in FQuery.Documents)
            {
                string s_id_floor = document_floor.Id;
                IDictionary data_floor = document_floor.ToDictionary();
                Carrot.Carrot_Box_Item item_floor = this.box_list.create_item();
                Sprite sp_floor = this.app.carrot.get_tool().get_sprite_to_playerPrefs(s_id_floor);
                if (sp_floor != null)
                    item_floor.set_icon_white(sp_floor);
                else
                    this.app.carrot.get_img_and_save_playerPrefs(data_floor["icon"].ToString(), item_floor.img_icon, s_id_floor);

                item_floor.set_act(() => act_sel_floor(s_id_floor));
            };
        }
    }

    private void act_sel_floor(string s_id)
    {
        PlayerPrefs.SetString("s_id_floor", s_id);
        this.sel_bk_floor(s_id);
        if (this.box_list != null) this.box_list.close();
    }

    public void btn_show_list_color_bk()
    {
        this.app.carrot.theme.show_list_color(act_sel_color_bk);
    }

    private void act_sel_color_bk(Color32 color_set)
    {
        this.s_color_bk = ColorUtility.ToHtmlStringRGBA(color_set);
        PlayerPrefs.SetString("s_color_bk", this.s_color_bk);
        this.set_color_bk(color_set);
    }

    public void btn_show_mix_color_bk()
    {
        this.app.carrot.theme.show_mix_color(this.app.carrot.theme.get_color_by_string(this.s_color_bk), act_sel_color_bk);
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

}
