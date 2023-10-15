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

    public Image img_btn_list_online;
    public Image img_btn_list_offline;
    public Image img_btn_list_radio;

    public GameObject button_link_ytb;
    private int sel_index_music=-1;
    public GameObject button_prev;
    public GameObject button_next;
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
    private IDictionary data_cur = null;

    public void act_play_data(IDictionary data_music)
    {
        this.data_song = Carrot.Json.Serialize(data_music);
        this.data_cur = data_music;

        this.button_download_mp3.SetActive(false);
        this.slider_download_music.gameObject.SetActive(false);

        this.panel_info_album.SetActive(false);
        this.panel_info_artrist.SetActive(false);
        this.panel_info_king_of_music.SetActive(false);
        this.panel_info_year.SetActive(false);

        this.button_add_playlist.SetActive(false);
        this.button_share_song.SetActive(false);
        this.button_lyrics.SetActive(false);
        this.button_link_ytb.SetActive(false);

        this.img_btn_list_offline.color = Color.white;
        this.img_btn_list_online.color = Color.white;
        this.img_btn_list_radio.color = Color.white;

        this.avatar_music.sprite = icon_music_default;
        this.avatar_music_mini.sprite = icon_music_default;

        this.sound_music.Stop();
        if (this.radio.isAudioPlaying) this.radio.Stop();

        this.id_music = data_music["id"].ToString();
        this.sel_index_music = int.Parse(data_music["index"].ToString());

        if (data_music["name"] != null)
        {
            this.txt_name_song_full.text = data_music["name"].ToString();
            this.txt_name_song_mini.text = data_music["name"].ToString();
        }

        if (data_music["type"].ToString() == "radio")
        {
            this.is_radio = true;
            string id_sp_radio = "radio_avatar_" + data_music["id"].ToString();
            Sprite sp_radio = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_sp_radio);
            if (sp_radio != null)
            {
                this.avatar_music.sprite = sp_radio;
                this.avatar_music_mini.sprite = sp_radio;
            }
            else
            {
                string s_url_icon = "";
                if (data_music["icon"] != null) s_url_icon = data_music["icon"].ToString();
                if (s_url_icon != "") this.app.carrot.get_img_and_save_playerPrefs(s_url_icon, this.avatar_music, id_sp_radio);
            }
            this.radio.Restart(2f);
            this.radio.Station.Name = data_music["name"].ToString();
            this.radio.Station.Url = data_music["url"].ToString();
            this.radio.Play();
            this.slider_timer_music.value = this.slider_timer_music.maxValue;
            this.img_play.sprite = this.icon_pause;
            this.img_play_mini.sprite = this.icon_pause;
            this.txt_time_music.text = "***";
            if (this.app.get_index_sel_func() != 2) this.panel_player_mini.SetActive(true);
            this.app.get_character().play_ani_stop_dance();
            this.img_btn_list_radio.color = this.app.carrot.color_highlight;
        }
        else
        {
            this.is_radio = false;
            this.slider_timer_music.value = 0;
            this.panel_player_mini.SetActive(true);
            this.panel_info_more.SetActive(true);

            this.is_buy_mp3_present = false;
            this.txt_time_music.text = "";

            this.data_lyrics = "";

            if (data_music["type"].ToString() == "offline")
            {
                this.is_online_music = false;
                this.img_btn_list_offline.color = this.app.carrot.color_highlight;
            }
            else
            {
                this.is_online_music = true;
                this.img_btn_list_online.color = this.app.carrot.color_highlight;
            }

            if (this.is_online_music)
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
                    string id_sp_avatar_music = "music_avatar" + data_music["id"].ToString();
                    if (s_avatar != "") this.app.carrot.get_img_and_save_playerPrefs(data_music["avatar"].ToString(), this.avatar_music, id_sp_avatar_music, act_load_done_avatar_music);
                }
            }

            if (data_music["type"].ToString()== "offline")
            {
                string id_index = data_music["index_del"].ToString();

                byte[] data_song = this.app.carrot.get_tool().get_data_to_playerPrefs("music_" + id_index + "_mp3");

                /*
                var memStream = new System.IO.MemoryStream(data_song);
                var mpgFile = new Crosstales.NLayer.MpegFile(memStream);
                var samples = new float[mpgFile.Length];
                mpgFile.ReadSamples(samples, 0, (int)mpgFile.Length);
                
                var clip = AudioClip.Create("music_song", samples.Length, mpgFile.Channels, mpgFile.SampleRate,false);
                clip.SetData(samples, 0);
                */

                AudioClip clip = Crosstales.Common.Audio.WavMaster.ToAudioClip(data_song);
                this.slider_timer_music.maxValue = clip.length;
                this.sound_music.clip = clip;
                this.sound_music.Play();
                this.slider_download_music.gameObject.SetActive(false);
                this.app.get_character().play_ani_dance();
                if (this.app.get_index_sel_func() == 2)
                    this.panel_player_mini.SetActive(false);
                else
                    this.panel_player_mini.SetActive(true);

            }
            else
            {
                if (data_music["mp3"] != null)
                {
                    StartCoroutine(get_mp3_form_url(data_music["mp3"].ToString()));
                }
            }
            this.check_icon_play_pause();
        }
        this.check_hide_btn_prev();
    }

    private void act_load_done_avatar_music(Texture2D tex)
    {
        Sprite sp_avatar_music = this.app.carrot.get_tool().Texture2DtoSprite(tex);
        this.avatar_music_mini.sprite = sp_avatar_music;
    }

    IEnumerator get_mp3_form_url(string s_url_audio)
    {
        Debug.Log(s_url_audio);
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
            if (this.app.carrot.is_online())
            {
                if (this.playlist.list_music_cur == null)
                    this.play_new_song();
                else
                    this.play_random_song();
            }
            else
            {
                IDictionary song_offline = this.playlist.get_radom_one_song_offline();
                if (song_offline != null) this.act_play_data(song_offline);
            }

        }
        else
        {
            if (this.sound_music.isPlaying)
                this.sound_music.Pause();
            else
                this.sound_music.Play();

            this.check_icon_play_pause();
        }
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
        if (this.data_cur["type"].ToString() == "radio")
        {
            if (this.radio.isAudioPlaying)
            {
                this.img_play.sprite = this.icon_pause;
                this.img_play_mini.sprite = this.icon_pause;
            }
            else
            {
                this.img_play.sprite = this.icon_play;
                this.img_play_mini.sprite = this.icon_play;
            }
        }
        else
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
        if (this.sel_index_music < 0) this.sel_index_music = this.playlist.list_music_cur.Count - 1;
        this.sound_music.Pause();
        this.act_play_data(this.playlist.list_music_cur[this.sel_index_music]);
        this.check_hide_btn_prev();
    }

    public void btn_next()
    {
        this.sel_index_music++;
        if (this.sel_index_music >= this.playlist.list_music_cur.Count) this.sel_index_music = 0;
        this.sound_music.Pause();
        this.act_play_data(this.playlist.list_music_cur[this.sel_index_music]);
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
        this.button_prev.SetActive(true);
        this.button_next.SetActive(true);

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
        int rand_index = UnityEngine.Random.Range(0,this.playlist.list_music_cur.Count);
        this.act_play_data(this.playlist.list_music_cur[rand_index]);
    }

    public void show_lyrics()
    {
        this.box_list=this.app.carrot.Create_Box(PlayerPrefs.GetString("song_lyrics", "Song Lyrics"), this.button_lyrics.GetComponent<Image>().sprite);
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
        this.playlist.add_song(this.id_music,this.data_song,Crosstales.Common.Audio.WavMaster.FromAudioClip(this.sound_music.clip), this.data_avatar);
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

    public string get_s_id_music_cur_play()
    {
        return this.id_music;
    }

    public string StripHTML(string input)
    {
        return Regex.Replace(input, "<.*?>", String.Empty);
    }
}
