using Carrot;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Playlist_Type {online,offline,radio,customer}
public class Music_playlist : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;
    public List<IDictionary> list_music = null;

    [Header("Asset Icon")]
    public Sprite icon;
    public Sprite icon_song;
    public Sprite icon_artist;
    public Sprite icon_album;
    public Sprite icon_year;
    public Sprite icon_genre;
    public Sprite icon_radio;
    public Sprite icon_music_online;
    private int length;

    [Header("Obj Other")]
    public GameObject btn_show_playlist;
    public GameObject btn_show_playlist_home;

    private Carrot.Carrot_Box box_list;
    private Carrot.Carrot_Window_Input box_search_inp;
    private Playlist_Type type;

    void Start()
    {
        this.length = PlayerPrefs.GetInt("music_length");
        this.check_show_btn_playlist();
    }

    private void check_show_btn_playlist()
    {
        if (this.length <= 0)
        {
            this.btn_show_playlist.SetActive(false);
            this.btn_show_playlist_home.SetActive(false);
        }
        else
        {
            this.btn_show_playlist.SetActive(true);
            this.btn_show_playlist_home.SetActive(true);
        }
    }

    public void add_song(string s_id_m,string s_data_music, byte[] data_mp3, byte[] data_avatar)
    {
        if (data_mp3 != null) this.app.carrot.get_tool().save_file("music"+s_id_m, data_mp3);
        if (data_avatar != null) this.app.carrot.get_tool().PlayerPrefs_Save_by_data("music_avatar" + s_id_m, data_avatar);

        PlayerPrefs.SetString("music_" + this.length, s_data_music);
        this.length++;
        PlayerPrefs.SetInt("music_length", this.length);
        this.check_show_btn_playlist();
        this.app.carrot.show_msg(PlayerPrefs.GetString("music_list", "Music List"), PlayerPrefs.GetString("add_music_list_success", "Added to your favorites list, you can listen to this song again without a network connection"));
    }

    public void show_playlist()
    {
        if (this.length > 0) {
            this.type = Playlist_Type.offline;
            if (this.box_list != null) this.box_list.close();
            this.box_list = this.app.carrot.Create_Box(PlayerPrefs.GetString("music_list", "Music Playlist"), this.icon);
            this.head_btn_box(this.box_list);
            for (int i = this.length-1; i >= 0; i--)
            {
                string s_data = PlayerPrefs.GetString("music_"+i);
                if (s_data != "")
                {
                    IDictionary data_music = (IDictionary)Carrot.Json.Deserialize(s_data);

                    string s_id_m = data_music["id"].ToString();
                    string s_name_m = data_music["name"].ToString().Trim();
                    var int_index_m = i;

                    Carrot.Carrot_Box_Item item_music_player = this.box_list.create_item();
                    item_music_player.set_title(s_name_m);

                    if (!this.app.carrot.get_tool().check_file_exist("music" + s_id_m))
                    {
                        item_music_player.set_type(Carrot.Box_Item_Type.box_value_txt);
                        item_music_player.set_val("Error:mp3 file has been lost!");
                        item_music_player.txt_val.color = Color.red;
                    }

                    item_music_player.set_tip(data_music["artist"].ToString());
                    item_music_player.set_act(() => act_play_song_in_playlist(data_music));

                    Sprite sp_icon = this.app.carrot.get_tool().get_sprite_to_playerPrefs("music_avatar" + s_id_m);
                    if (sp_icon == null)
                        item_music_player.set_icon(this.icon_song);
                    else
                        item_music_player.set_icon_white(sp_icon);

                    if (this.app.carrot.is_online())
                    {
                        var link_song_share = "https://carrotstore.web.app/?p=song&id="+ s_id_m;
                        Carrot.Carrot_Box_Btn_Item btn_share = item_music_player.create_item();
                        btn_share.set_icon(this.app.carrot.sp_icon_share);
                        btn_share.set_color(this.app.carrot.color_highlight);
                        btn_share.set_act(() => this.app.carrot.show_share(link_song_share, PlayerPrefs.GetString("share_song", "Share this song so everyone can hear it!")));
                    }

                    Carrot.Carrot_Box_Btn_Item btn_del = item_music_player.create_item();
                    btn_del.set_icon(this.app.carrot.sp_icon_del_data);
                    btn_del.set_color(this.app.carrot.color_highlight);
                    btn_del.set_act(() => this.app.player_music.playlist.delete_item(int_index_m));
                }
            }
            this.box_list.update_color_table_row();
        }
        else
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("music_list", "Music Playlist"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
        }
    }

    private void act_play_song_in_playlist(IDictionary data_music)
    {
        this.app.player_music.act_play_data(data_music, false);
        if (this.box_list != null) this.box_list.close();
    }

    private void act_play_song_online(IDictionary data_music)
    {
        this.app.player_music.act_play_data(data_music, true);
        if (this.box_list != null) this.box_list.close();
    }

    public void delete_item(int index)
    {
        this.app.carrot.get_tool().delete_file("music" + index);
        PlayerPrefs.DeleteKey("music_" + index);
        this.show_playlist();
    }

    public void show_list_music_online()
    {
        if (this.list_music!=null)
        {
            this.type = Playlist_Type.online;
            if (this.box_list != null) this.box_list.close();
            this.box_list = this.app.carrot.Create_Box(PlayerPrefs.GetString("song_history", "List Music History"), this.icon_music_online);
            this.head_btn_box(this.box_list);
            for (int i = 0; i < this.list_music.Count; i++)
            {
                IDictionary data_music = this.list_music[i];
                string s_id_song = data_music["id"].ToString();
                Carrot.Carrot_Box_Item item_history = this.box_list.create_item("song_" + i);

                item_history.set_title(data_music["name"].ToString());
                item_history.set_tip(data_music["artist"].ToString());
                item_history.set_icon(this.icon_song);

                Sprite sp_avatar = this.app.player_music.get_avatar_music(s_id_song);
                if (sp_avatar != null)
                {
                    item_history.set_icon_white(sp_avatar);
                }
                else
                {
                    string s_avatar = data_music["avatar"].ToString();
                    string id_sp_avatar_music = "music_avatar" + data_music["id"].ToString();
                    if (s_avatar != "") this.app.carrot.get_img_and_save_playerPrefs(data_music["avatar"].ToString(), item_history.img_icon, id_sp_avatar_music);
                }
                   

                if (this.app.player_music.get_index_music_play() == i)
                {
                    item_history.GetComponent<Image>().color = this.app.carrot.color_highlight;
                    item_history.txt_tip.color = Color.white;
                }

                item_history.set_act(() => this.act_play_song_online(data_music));
            }
            this.box_list.update_color_table_row();
        }
        else
        {
            this.get_data_list_playlist(this.app.carrot.lang.get_key_lang(), this.show_list_music_online);
        }
    }

    public void btn_show_seach_music()
    {
        this.box_search_inp=this.app.carrot.show_search(act_search_music, PlayerPrefs.GetString("search_song_tip", "Enter the name of the song you want to listen to"));
    }

    private void act_search_music(string key_search)
    {
        this.app.carrot.show_loading();
        if (this.app.carrot.is_online())
        {
            Query SongQuery = this.app.carrot.db.Collection("song").WhereEqualTo("name", key_search);
            SongQuery.Limit(20).GetSnapshotAsync().ContinueWithOnMainThread(task => {
                QuerySnapshot songQuerySnapshot = task.Result;

                if (task.IsFaulted)
                {
                    this.app.carrot.hide_loading();
                    IList list_song = this.get_list_song_online_by_search_key(key_search);
                    if (list_song != null) this.box_list_song(list_song);
                }

                if (task.IsCompleted)
                {
                    this.app.carrot.hide_loading();
                    if (songQuerySnapshot.Count>0)
                    {
                        Debug.Log("Search song oline");
                        IList list_song = (IList)Json.Deserialize("[]");
                        foreach (DocumentSnapshot SongSnapshot in songQuerySnapshot.Documents)
                        {
                            string s_id_song = SongSnapshot.Id;
                            IDictionary data_music = SongSnapshot.ToDictionary();
                            data_music["id"] = s_id_song;
                            list_song.Add(data_music);
                        }
                        this.box_list_song(list_song);
                        Debug.Log("Search song oline");
                    }
                    else
                    {
                        Debug.Log("Search song oline data offline");
                        IList list_song = this.get_list_song_online_by_search_key(key_search);
                        if (list_song.Count != 0)
                            this.box_list_song(list_song);
                        else
                            this.box_list_song(this.get_list_song_offline_by_search_key(key_search));
                    }
                }
            });
        }
        else
        {
            this.app.carrot.hide_loading();
            IList list_song = this.get_list_song_offline_by_search_key(key_search);
            if (list_song != null)
                this.box_list_song(list_song);
            else
                this.app.carrot.show_msg(PlayerPrefs.GetString("search_results", "Search Results"), PlayerPrefs.GetString("no_song_found","No songs found!"), Carrot.Msg_Icon.Alert);
        }
    }

    private void box_list_song(IList list_song)
    {
        if (list_song.Count > 0)
        {
            if(this.box_search_inp!=null) this.box_search_inp.close();
            if (this.box_list != null) this.box_list.close();
            this.box_list = this.app.carrot.Create_Box("search_results");
            this.box_list.set_title(PlayerPrefs.GetString("search_results", "Search Results"));
            this.box_list.set_icon(this.app.carrot.icon_carrot_search);

            for (int i = 0; i < list_song.Count; i++)
            {
                IDictionary data_song = (IDictionary)list_song[i];
                string s_id_song = data_song["id"].ToString();
                Carrot.Carrot_Box_Item item_song = this.box_list.create_item("song_" + s_id_song);
                Sprite sp_avatar = this.app.player_music.get_avatar_music(s_id_song);
                if (sp_avatar != null)
                    item_song.set_icon_white(sp_avatar);
                else
                    item_song.set_icon(this.icon_song);
                item_song.set_tip(data_song["artist"].ToString());
                item_song.set_title(data_song["name"].ToString());
                item_song.set_act(() => this.act_select_song_search_results(data_song));
            }
        }
        else
        {
            if (this.box_search_inp != null) this.box_search_inp.close();
            this.app.carrot.show_msg(PlayerPrefs.GetString("search_results", "Search Results"), "No songs found!", Carrot.Msg_Icon.Alert);
        }
    }

    private IList get_list_song_online_by_search_key(string s_key)
    {
        IList list_song = (IList)Json.Deserialize("[]");
        if (this.list_music != null)
        {
            for (int i = 0; i < this.list_music.Count; i++)
            {
                IDictionary song_data = this.list_music[i];
                if (song_data["name"].ToString().Contains(s_key)) list_song.Add(song_data);
            }
        }
        return list_song;
    }

    private IList get_list_song_offline_by_search_key(string s_key)
    {
        IList list_song = (IList)Json.Deserialize("[]");
        if (this.length > 0)
        {
            for (int i = this.length - 1; i >= 0; i--)
            {
                string s_data = PlayerPrefs.GetString("music_" + i);
                if (s_data != "")
                {
                    IDictionary data_music = (IDictionary)Carrot.Json.Deserialize(s_data);
                    if (data_music["name"].ToString().Contains(s_key))
                    {
                        data_music["song_index"] = i;
                        list_song.Add(data_music);
                    }
                }
            }
        }
        return list_song;
    }

    private void act_select_song_search_results(IDictionary data_song)
    {
        this.app.player_music.act_play_data(data_song, true);
        this.box_search_inp.close();
        if (this.box_list != null) this.box_list.close();
    }

    public void show_list_radio()
    {
        Query RadioQuery = this.app.carrot.db.Collection("radio").Limit(20);
        RadioQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot songQuerySnapshot = task.Result;

            if (task.IsCompleted)
            {
                if (songQuerySnapshot.Count > 0)
                {
                    if (this.box_list != null) this.box_list.close();
                    this.box_list = this.app.carrot.Create_Box("Radio");
                    this.box_list.set_icon(this.icon_radio);
                    List<IDictionary> list_radio = new List<IDictionary>();
                    foreach (DocumentSnapshot SongSnapshot in songQuerySnapshot.Documents)
                    {
                        string s_id_radio = SongSnapshot.Id;
                        IDictionary data_radio = SongSnapshot.ToDictionary();
                        var s_name_radio = data_radio["name"].ToString();
                        var s_url_radio = data_radio["url"].ToString();
                        Carrot.Carrot_Box_Item item_radio = this.box_list.create_item("radio_" + data_radio["id"].ToString());
                        item_radio.set_icon(this.icon_radio);
                        item_radio.set_title(s_name_radio);
                        item_radio.set_tip(s_url_radio);

                        string id_sp_radio = "radio_avatar_" + data_radio["id"].ToString();
                        Sprite sp_radio = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_sp_radio);
                        if (sp_radio != null)
                            item_radio.set_icon_white(sp_radio);
                        else
                        {
                            string s_url_icon = "";
                            if (data_radio["icon"] != null) s_url_icon = data_radio["icon"].ToString();
                            if (s_url_icon != "") this.app.carrot.get_img_and_save_playerPrefs(s_url_icon, item_radio.img_icon, id_sp_radio);
                        }

                        item_radio.set_act(() => this.act_play_radio(item_radio.img_icon.sprite, s_name_radio, s_url_radio));

                        data_radio["id"] = s_id_radio;
                        list_radio.Add(data_radio);
                    };
                    this.box_list.update_color_table_row();
                }
                else
                {
                    this.app.carrot.show_msg("Radio", "Lits None");
                }
            }
        });
    }


    private void act_play_radio(Sprite sp_icon,string s_name,string s_url)
    {
        this.app.player_music.play_radio(sp_icon, s_name, s_url);
        if (this.box_list != null) this.box_list.close();
    }


    private void head_btn_box(Carrot.Carrot_Box box_list)
    {
        if (this.type != Playlist_Type.radio||this.type!=Playlist_Type.customer)
        {
            Carrot.Carrot_Box_Btn_Item btn_search = box_list.create_btn_menu_header(this.app.carrot.icon_carrot_search);
            btn_search.set_act(() =>this.btn_show_seach_music());
        }

        if (this.type != Playlist_Type.offline)
        {
            Carrot.Carrot_Box_Btn_Item btn_playlist = box_list.create_btn_menu_header(this.icon);
            btn_playlist.set_act(() => this.show_playlist());
        }

        if (this.type != Playlist_Type.radio||this.type!=Playlist_Type.customer)
        {
            Carrot.Carrot_Box_Btn_Item btn_radio = box_list.create_btn_menu_header(this.icon_radio);
            btn_radio.set_act(() => this.show_list_radio());
        }

        if (this.type != Playlist_Type.online)
        {
            Carrot.Carrot_Box_Btn_Item btn_history = box_list.create_btn_menu_header(this.icon_music_online);
            btn_history.set_act(() => this.show_list_music_online());
        }
    }

    public void get_data_list_playlist(string lang,UnityAction act_done=null)
    {
        Query ChatQuery = this.app.carrot.db.Collection("song").WhereEqualTo("lang", lang).OrderByDescending("publishedAt").Limit(20);

        ChatQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot songQuerySnapshot = task.Result;

            if (task.IsCompleted)
            {
                if (songQuerySnapshot.Count > 0)
                {
                    this.list_music = new List<IDictionary>();
                    foreach (DocumentSnapshot SongSnapshot in songQuerySnapshot.Documents)
                    {
                        string s_id_song = SongSnapshot.Id;
                        IDictionary data_music = SongSnapshot.ToDictionary();
                        data_music["id"] = s_id_song;
                        this.list_music.Add(data_music);
                    };

                    if (act_done != null) act_done();
                }
                else
                {
                    this.get_data_list_playlist("en", act_done);
                }
            }
        });
    }
}
