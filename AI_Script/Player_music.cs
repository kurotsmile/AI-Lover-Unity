using Crosstales.Radio;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player_music : MonoBehaviour
{
    [Header("Obj Main")]
    public Music_playlist playlist;
    public RadioPlayer radio;
    public App app;

    [Header("Ui")]
    public GameObject panel_player_mini;
    public Text txt_name_song_mini;
    public Text txt_name_song_full;
    public AudioSource sound_music;
    public Slider slider_download_music;
    public Slider slider_timer_music;
    private string id_music = "";
    private string link_youtube = "";
    private string link_store = "";
    private string data_lyrics = "";
    private string data_song = "";
    public Sprite icon_play;
    public Sprite icon_pause;
    public Sprite icon_music_default;
    public Image avatar_music;
    public Image avatar_music_mini;
    public Image img_play;
    public Image img_play_mini;
    public GameObject button_link_ytb;
    private int sel_index_music=-1;
    public GameObject button_prev;
    public GameObject button_next;
    public GameObject button_history;
    public GameObject button_lyrics;
    public GameObject button_download_mp3;
    public GameObject button_share_song;
    public GameObject button_add_playlist;
    public GameObject prefab_item_lyrics;
    private byte[] data_mp3;
    private byte[] data_avatar;
    private string s_link_download_mp3;
    public bool is_buy_mp3_present=false;
    private bool is_radio = false;
    private bool is_online_music = false;

    [Header("Info More Music")]
    public GameObject panel_info_more;
    public GameObject panel_info_album;
    public GameObject panel_info_artrist;
    public GameObject panel_info_king_of_music;
    public GameObject panel_info_year;

    public Text txt_info_album;
    public Text txt_info_artrist;
    public Text txt_info_king_of_music;
    public Text txt_info_year;
    public Text txt_time_music;

    private Carrot.Carrot_Box box_list;

    private void hide_all_info()
    {
        this.panel_info_album.SetActive(false);
        this.panel_info_artrist.SetActive(false);
        this.panel_info_king_of_music.SetActive(false);
        this.panel_info_year.SetActive(false);
    }

    public void act_play_data(IDictionary data_music, bool is_music_online)
    {
        this.data_song = Carrot.Json.Serialize(data_music);
        Debug.Log(Carrot.Json.Serialize(data_music));
        this.is_radio = false;
        this.hide_all_info();
        this.sound_music.Stop();
        this.slider_timer_music.value = 0;
        this.avatar_music.sprite = icon_music_default;
        this.avatar_music_mini.sprite = icon_music_default;
        this.panel_player_mini.SetActive(true);
        this.id_music = data_music["id"].ToString();
        if (data_music["name"] != null)
        {
            this.txt_name_song_full.text = data_music["name"].ToString();
            this.txt_name_song_mini.text = data_music["name"].ToString();
        }
        else
        {
            this.txt_name_song_full.text = "No Name";
            this.txt_name_song_mini.text = "No Name";
        }

        this.panel_info_more.SetActive(true);
        this.button_link_ytb.SetActive(false);
        this.button_lyrics.SetActive(false);
        this.button_download_mp3.SetActive(false);
        this.button_share_song.SetActive(false);
        this.is_buy_mp3_present = false;
        this.button_prev.SetActive(true);
        this.button_next.SetActive(true);
        this.txt_time_music.text = "";

        this.data_lyrics = "";

        this.button_add_playlist.SetActive(false);
        this.is_online_music = is_music_online;

        if (is_music_online)
        {
            this.button_download_mp3.SetActive(true);
            this.app.command.add_item_log_music(data_music["name"].ToString(), data_music);
        }

        if (data_music["artist"] != null)
        {
            this.txt_info_artrist.text = data_music["artist"].ToString();
            this.panel_info_artrist.SetActive(true);
        }

        if (data_music["album"] != null)
        {
            this.txt_info_album.text = data_music["album"].ToString();
            this.panel_info_album.SetActive(true);
        }

        if (data_music["genre"] != null)
        {
            this.txt_info_king_of_music.text = data_music["genre"].ToString();
            this.panel_info_king_of_music.SetActive(true);
        }

        if (data_music["year"] != null)
        {
            this.txt_info_year.text = data_music["year"].ToString();
            this.panel_info_year.SetActive(true);
        }

        if (data_music["link_ytb"] != null)
        {
            this.link_youtube = data_music["link_ytb"].ToString();
            this.button_link_ytb.SetActive(true);
        }

        if (data_music["link_store"] != null)
        {
            this.link_store = data_music["link_store"].ToString();
            this.button_share_song.SetActive(true);
        }

        if (data_music["lyrics"] != null)
        {
            this.button_lyrics.SetActive(true);
            this.data_lyrics = this.StripHTML(data_music["lyrics"].ToString());
        }

        this.slider_download_music.gameObject.SetActive(true);
        this.check_icon_play_pause();
        Sprite sp_avatar_music = this.app.carrot.get_tool().get_sprite_to_playerPrefs(this.id_music);
        if (sp_avatar_music != null)
        {
            this.avatar_music.sprite = sp_avatar_music;
            this.avatar_music_mini.sprite = sp_avatar_music;
        }
        else
        {
            if (data_music["avatar"] != null)
            {
                string s_avatar = data_music["avatar"].ToString();
                string id_sp_avatar_music= "music_avatar"+data_music["id"].ToString();
                if (s_avatar != "") this.app.carrot.get_img_and_save_playerPrefs(data_music["avatar"].ToString(), this.avatar_music, id_sp_avatar_music, act_load_done_avatar_music);
            }
        }
        if (data_music["mp3"] != null)
        {
            Debug.Log("ID music:" + this.id_music);
            if (this.app.carrot.get_tool().check_file_exist("music"+this.id_music))
            {
                Debug.Log("File Ready ID music:" + this.id_music);
                string path_file_mp3;
                if (Application.isEditor)
                    path_file_mp3 = Application.dataPath + "/music" + data_music["id"].ToString();
                else
                    path_file_mp3 = Application.persistentDataPath + "/music" + data_music["id"].ToString();
                StartCoroutine(get_mp3_form_url(path_file_mp3));
            }
            else
                StartCoroutine(get_mp3_form_url(data_music["mp3"].ToString()));
        }
    }

    private void act_load_done_avatar_music(Texture2D tex)
    {
        Sprite sp_avatar_music = this.app.carrot.get_tool().Texture2DtoSprite(tex);
        this.avatar_music_mini.sprite = sp_avatar_music;
    }

    IEnumerator get_mp3_form_url(string s_url_audio)
    {
        this.s_link_download_mp3 = s_url_audio;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(s_url_audio, AudioType.MPEG))
        {
            www.SendWebRequest();
            while (!www.isDone)
            {
                slider_download_music.value = www.downloadProgress;
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                this.data_mp3 = www.downloadHandler.data;
                this.sound_music.clip = DownloadHandlerAudioClip.GetContent(www);
                this.slider_timer_music.maxValue = this.sound_music.clip.length;
                this.sound_music.Play();
                this.slider_download_music.gameObject.SetActive(false);
                this.app.get_character().play_ani_dance();
                if (this.app.get_index_sel_func() == 2)
                    this.panel_player_mini.SetActive(false);
                else
                    this.panel_player_mini.SetActive(true);
                this.check_icon_play_pause();
                if(this.is_online_music) this.button_add_playlist.SetActive(true);
            }
        }
    }

    public void check_show_mini_player_where_music(int index_sel_func_app)
    {
        if (index_sel_func_app == 2)
        {
            this.panel_player_mini.SetActive(false);
            if (this.sound_music.clip != null)
            {
                this.panel_info_more.SetActive(true);
                this.txt_time_music.gameObject.SetActive(true);
            }
            else
            {
                this.panel_info_more.SetActive(false);
                this.button_link_ytb.SetActive(false);
                this.button_lyrics.SetActive(false);
                this.button_add_playlist.SetActive(false);
                this.button_download_mp3.SetActive(false);
                this.button_share_song.SetActive(false);
                this.txt_name_song_full.text = PlayerPrefs.GetString("no_song_play_title", "No song");
            }
        }
        else
        {
            if (this.sound_music.clip != null)
            {
                this.panel_player_mini.SetActive(true);
                this.txt_time_music.gameObject.SetActive(false);
            }
        }
    }

    public void open_link_youtube()
    {
        Application.OpenURL(this.link_youtube);
    }

    public void btn_play_or_pause()
    {
        if (this.sound_music.clip == null)
        {
            this.play_new_song();
        }
        else
        {
            if (this.sound_music.isPlaying)
                this.sound_music.Pause();
            else
                this.sound_music.Play();
        }
        this.check_icon_play_pause();
    }

    public void btn_play()
    {
        this.sound_music.UnPause();
        this.check_icon_play_pause();
    }

    public void btn_pause()
    {
        this.sound_music.Pause();
        this.check_icon_play_pause();
    }

    private void check_icon_play_pause()
    {
            if (this.sound_music.isPlaying)
            {
                this.img_play.sprite = this.icon_pause;
                this.img_play_mini.sprite = this.icon_pause;
                if (this.sound_music.clip != null) GameObject.Find("app").GetComponent<App>().get_character().unpause_ani();
            }
            else
            {
                this.img_play.sprite = this.icon_play;
                this.img_play_mini.sprite = this.icon_play;
                if (this.sound_music.clip != null) GameObject.Find("app").GetComponent<App>().get_character().pause_ani();
            }
    }

    public void btn_stop()
    {
        this.avatar_music.sprite = icon_music_default;
        this.avatar_music_mini.sprite = icon_music_default;
        this.slider_timer_music.value = 0;
        this.slider_download_music.value = 0;
        this.sound_music.clip = null;
        this.sound_music.Stop();
        this.panel_player_mini.SetActive(false);
        this.app.get_character().play_ani_stop_dance();
        if (this.is_radio) this.radio.Stop();
        this.StopAllCoroutines();
        this.app.carrot.stop_all_act();
    }

    public void btn_prev()
    {
        this.sel_index_music--;
        if (this.sel_index_music < 0) this.sel_index_music = this.playlist.list_music.Count - 1;
        this.sound_music.Pause();
        this.act_play_data(this.playlist.list_music[this.sel_index_music],true);
        this.check_hide_btn_prev();
    }

    public void btn_next()
    {
        this.sel_index_music++;
        if (this.sel_index_music >= this.playlist.list_music.Count) this.sel_index_music = 0;
        this.sound_music.Pause();
        this.act_play_data(this.playlist.list_music[this.sel_index_music], true);
        this.check_hide_btn_prev();
    }

    public void btn_link_carrotstore_muisc()
    {
        if (this.link_store != "")
        {
            Application.OpenURL(this.link_store);
        }
        else
        {
            if(GameObject.Find("app").GetComponent<App>().carrot.store_public==Carrot.Store.Google_Play) Application.OpenURL("https://play.google.com/store/apps/details?id=com.CarrotApp.musicforlife");
            if(GameObject.Find("app").GetComponent<App>().carrot.store_public==Carrot.Store.Amazon_app_store) Application.OpenURL("https://www.amazon.com/gp/mas/dl/android?p=com.CarrotApp.musicforlife");
            if(GameObject.Find("app").GetComponent<App>().carrot.store_public==Carrot.Store.Microsoft_Store) Application.OpenURL("https://www.microsoft.com/store/productId/9PMH34Z5TWZ2");
            if(GameObject.Find("app").GetComponent<App>().carrot.store_public==Carrot.Store.Carrot_store) Application.OpenURL("http://carrotstore.com/music");
            if(GameObject.Find("app").GetComponent<App>().carrot.store_public==Carrot.Store.Samsung_Galaxy_Store) Application.OpenURL("https://galaxystore.samsung.com/detail/com.CarrotApp.musicforlife");
        }
    }

    private void check_hide_btn_prev()
    {
        if (this.sel_index_music >= 1)
        { 
            this.button_prev.GetComponent<Button>().interactable = true;
            this.button_prev.GetComponent<Image>().color = Color.white;
        }
        else
        {
            this.button_prev.GetComponent<Button>().interactable = false;
            this.button_prev.GetComponent<Image>().color = new Color(225, 225, 225, 96);
        }
    }

    void Update()
    {
        if (!this.is_radio)
        {
            if (this.sound_music.isPlaying)
            {
                this.txt_time_music.text = string.Format("{0}:{1:00}", (int)this.sound_music.time / 60, (int)this.sound_music.time % 60);
                this.slider_timer_music.value = this.sound_music.time;
                if (this.sound_music.time >= (this.slider_timer_music.maxValue - 0.1f))
                {
                    this.sound_music.Stop();
                    this.check_icon_play_pause();
                }
            }
        }
    }

    public void play_new_song()
    {
        get_new_song(this.app.carrot.lang.get_key_lang());
    }

    private void get_new_song(string s_lang=null)
    {
        this.playlist.get_data_list_playlist(s_lang,this.play_random_song);
    }

    private void play_random_song()
    {
        int rand_index = UnityEngine.Random.Range(0,this.playlist.list_music.Count);
        this.act_play_data(this.playlist.list_music[rand_index], true);
    }

    public void play_radio(Sprite sp_avatar,string s_name,string s_url_stream)
    {
        this.sound_music.Stop();
        if (this.radio.isAudioPlaying) this.radio.Stop();
        this.is_radio = true;
        this.hide_all_info();
        this.txt_name_song_full.text = s_name;
        this.txt_name_song_mini.text= s_name;
        this.avatar_music.sprite = sp_avatar;
        this.avatar_music_mini.sprite = sp_avatar;
        this.GetComponent<RadioPlayer>().Restart(2f);
        this.GetComponent<RadioPlayer>().Station.Name=s_name;
        this.GetComponent<RadioPlayer>().Station.Url=s_url_stream;
        this.GetComponent<RadioPlayer>().Play();
        this.slider_download_music.gameObject.SetActive(false);
        this.slider_timer_music.value = this.slider_timer_music.maxValue;
        this.img_play.sprite = this.icon_pause;
        this.img_play_mini.sprite = this.icon_pause;
        this.button_prev.SetActive(false);
        this.button_next.SetActive(false);
        this.button_download_mp3.SetActive(false);
        this.button_add_playlist.SetActive(false);
        this.button_share_song.SetActive(false);
        this.txt_time_music.text = "***";
        this.button_lyrics.SetActive(false);
        if(GameObject.Find("app").GetComponent<App>().get_index_sel_func()!=2)this.panel_player_mini.SetActive(true);
        GameObject.Find("app").GetComponent<App>().get_character().play_ani_stop_dance();
    }


    public void show_lyrics()
    {
        this.box_list=GameObject.Find("app").GetComponent<App>().carrot.Create_Box(PlayerPrefs.GetString("song_lyrics", "Song Lyrics"), this.button_lyrics.GetComponent<Image>().sprite);
        GameObject item_lyrics_obj = Instantiate(this.prefab_item_lyrics);
        item_lyrics_obj.transform.SetParent(this.box_list.area_all_item);
        item_lyrics_obj.transform.localScale = new Vector3(1f, 1f, 1f);
        item_lyrics_obj.transform.localPosition = new Vector3(item_lyrics_obj.transform.localPosition.x, item_lyrics_obj.transform.localPosition.y, 0f);
        item_lyrics_obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        item_lyrics_obj.GetComponent<Text>().text = this.data_lyrics;
        Canvas.ForceUpdateCanvases();
    }

    public void add_song_to_playlist()
    {
        this.button_add_playlist.SetActive(false);
        this.playlist.add_song(this.id_music,this.data_song,this.data_mp3,this.data_avatar);
    }

    public void load_file_mp3_and_play(string id_song, IDictionary data_music)
    {
        string name_file_mp3 = "music" + id_song;
        if (Application.isEditor)
            name_file_mp3 = Application.dataPath + "/" + name_file_mp3;
        else
            name_file_mp3 = Application.persistentDataPath + "/" + name_file_mp3;
        if (System.IO.File.Exists(name_file_mp3))
            StartCoroutine(this.get_mp3_form_url("file://"+name_file_mp3));
        else
            this.act_play_data(data_music, true);
    }

    public void load_avatar_offline(string s_id_m)
    {
        Sprite sp_avatar_music = get_avatar_music(s_id_m);
        if (sp_avatar_music != null)
        {
            this.avatar_music.sprite = sp_avatar_music;
            this.avatar_music_mini.sprite = sp_avatar_music;
        }
        else
        {
            this.avatar_music.sprite = this.icon_music_default;
            this.avatar_music_mini.sprite = this.icon_music_default;
        }

    }

    public Sprite get_avatar_music(string s_id_m)
    {
        Sprite sp_avatar_music = this.app.carrot.get_tool().get_sprite_to_playerPrefs("music_avatar"+s_id_m);
        if (sp_avatar_music != null)
            return sp_avatar_music;
        else
            return null;
    }

    public void act_download_mp3()
    {
        if (this.app.setting.check_buy_product(7))
            Application.OpenURL(this.s_link_download_mp3);
        else if (this.is_buy_mp3_present)
            Application.OpenURL(this.s_link_download_mp3);
        else
            this.app.buy_product(1);
    }
     
    public void act_download_mp3_form_shop()
    {
        this.is_buy_mp3_present = true;
        Application.OpenURL(this.s_link_download_mp3);
    }

    public void btn_share_music()
    {
        this.app.carrot.show_share(this.link_store, PlayerPrefs.GetString("share_song","Share this song so everyone can hear it"));
    }

    public string get_id_song_current()
    {
        return this.id_music;
    }

    public void error_radio(string s_error,Int32 index_error,string s_error_msg)
    {
        //this.app.carrot.show_msg(s_error, index_error + " :" + s_error_msg, Carrot.Msg_Icon.Error);
    }

    public int get_index_music_play()
    {
        return this.sel_index_music;
    }

    public string StripHTML(string input)
    {
        return Regex.Replace(input, "<.*?>", String.Empty);
    }
}
