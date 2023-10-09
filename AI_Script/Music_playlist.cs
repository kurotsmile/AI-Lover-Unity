using Carrot;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Playlist_Type {online,offline,radio,music_search_result}
public class Music_playlist : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    public List<IDictionary> list_music_cur = null;

    private List<IDictionary> list_music_online = null;
    private List<IDictionary> list_music_offline = null;

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
        if (data_mp3 != null) this.app.carrot.get_tool().save_file("music"+ s_id_m, data_mp3);
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

            this.list_music_offline = new List<IDictionary>();
            for (int i = this.length-1; i >= 0; i--)
            {
                string s_data = PlayerPrefs.GetString("music_"+i);
                if (s_data != "")
                {
                    IDictionary data_music = (IDictionary)Carrot.Json.Deserialize(s_data);
                    data_music["type"] = "offline";
                    this.list_music_offline.Add(data_music);
                }
            }

            this.list_music_cur = this.list_music_offline;
            this.box_list_song(this.list_music_offline);
        }
        else
        {
            this.app.carrot.show_msg(PlayerPrefs.GetString("music_list", "Music Playlist"), PlayerPrefs.GetString("list_none", "List is empty, no items found!"));
        }
    }

    private void act_play_song(IDictionary data_music)
    {
        this.app.player_music.act_play_data(data_music);
        if(this.box_search_inp!=null) this.box_search_inp.close();
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
        if (this.list_music_online!=null)
        {
            this.type = Playlist_Type.online;
            this.box_list_song(this.list_music_online);
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
        this.type = Playlist_Type.music_search_result;
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
                        IList list_song = (IList)Json.Deserialize("[]");
                        foreach (DocumentSnapshot SongSnapshot in songQuerySnapshot.Documents)
                        {
                            IDictionary data_music = SongSnapshot.ToDictionary();
                            data_music["id"] = Get16BitHash(SongSnapshot.Id);
                            data_music["id_web"] = SongSnapshot.Id;
                            data_music["type"] = "online";
                            list_song.Add(data_music);
                        }
                        this.box_list_song(list_song);
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

            this.box_list = this.app.carrot.Create_Box("box_list");

            if (this.type == Playlist_Type.music_search_result)
            {
                this.box_list.set_title(PlayerPrefs.GetString("search_results", "Search Results"));
                this.box_list.set_icon(this.app.carrot.icon_carrot_search);
            }

            if (this.type == Playlist_Type.offline)
            {
                this.box_list.set_title(PlayerPrefs.GetString("music_list", "Music Playlist"));
                this.box_list.set_icon(this.icon);
            }

            if (this.type == Playlist_Type.online)
            {
                this.box_list.set_title(PlayerPrefs.GetString("song_history", "List Music History"));
                this.box_list.set_icon(this.icon_music_online);
            }

            if (this.type == Playlist_Type.radio)
            {
                this.box_list.set_title(PlayerPrefs.GetString("radio", "Radio"));
                this.box_list.set_icon(this.icon_radio);
            }

            Carrot.Carrot_Box_Btn_Item btn_search = box_list.create_btn_menu_header(this.app.carrot.icon_carrot_search);
            btn_search.set_act(() => this.btn_show_seach_music());
            if (this.type != Playlist_Type.music_search_result) btn_search.set_icon_color(this.app.carrot.color_highlight);

            Carrot.Carrot_Box_Btn_Item btn_playlist = box_list.create_btn_menu_header(this.icon);
            btn_playlist.set_act(() => this.show_playlist());
            if (this.type != Playlist_Type.offline) btn_playlist.set_icon_color(this.app.carrot.color_highlight);

            Carrot.Carrot_Box_Btn_Item btn_history = box_list.create_btn_menu_header(this.icon_music_online);
            btn_history.set_act(() => this.show_list_music_online());
            if (this.type != Playlist_Type.online) btn_history.set_icon_color(this.app.carrot.color_highlight);

            for (int i = 0; i < list_song.Count; i++)
            {
                IDictionary data_song = (IDictionary)list_song[i];
                data_song["index"] = i;
                string s_id_song = data_song["id"].ToString();
                string s_name_m = data_song["name"].ToString().Trim();
                var int_index_m = i;
                Carrot.Carrot_Box_Item item_song = this.box_list.create_item("song_" + s_id_song);

                if (data_song["type"].ToString()== "radio")
                {
                    var s_url_radio = data_song["url"].ToString();
                    item_song.set_icon(this.icon_radio);
                    item_song.set_title(s_name_m);
                    item_song.set_tip(s_url_radio);

                    string id_sp_radio = "radio_avatar_"+s_id_song;
                    Sprite sp_radio = this.app.carrot.get_tool().get_sprite_to_playerPrefs(id_sp_radio);
                    if (sp_radio != null)
                    {
                        item_song.set_icon_white(sp_radio);
                    }
                    else
                    {
                        string s_url_icon = "";
                        if (data_song["icon"] != null) s_url_icon = data_song["icon"].ToString();
                        if (s_url_icon != "") this.app.carrot.get_img_and_save_playerPrefs(s_url_icon, item_song.img_icon, id_sp_radio);
                    }
                }
                else
                {
                    Sprite sp_avatar = this.app.player_music.get_avatar_music(s_id_song);
                    if (sp_avatar != null)
                    {
                        item_song.set_icon_white(sp_avatar);
                    }
                    else
                    {
                        string s_avatar = data_song["avatar"].ToString();
                        string id_sp_avatar_music = "music_avatar" + data_song["id"].ToString();
                        if (s_avatar != "")
                        {
                            this.app.carrot.get_img_and_save_playerPrefs(data_song["avatar"].ToString(), item_song.img_icon, id_sp_avatar_music);
                        }
                        else
                        {
                            item_song.set_icon(this.icon_song);
                        }
                    }

                    item_song.set_tip(data_song["artist"].ToString());
                    item_song.set_title(data_song["name"].ToString());

                    if (this.app.carrot.is_online())
                    {
                        if (data_song["id_web"] != null)
                        {
                            var link_song_share = "https://carrotstore.web.app/?p=song&id=" + data_song["id_web"].ToString();
                            Carrot.Carrot_Box_Btn_Item btn_share = item_song.create_item();
                            btn_share.set_icon(this.app.carrot.sp_icon_share);
                            btn_share.set_color(this.app.carrot.color_highlight);
                            btn_share.set_act(() => this.app.carrot.show_share(link_song_share, PlayerPrefs.GetString("share_song", "Share this song so everyone can hear it!")));
                        }
                    }

                    if (this.type == Playlist_Type.offline)
                    {
                        if (!this.app.carrot.get_tool().check_file_exist("music" + s_id_song))
                        {
                            item_song.set_type(Carrot.Box_Item_Type.box_value_txt);
                            item_song.set_val("Error:mp3 file has been lost!");
                            item_song.txt_val.color = Color.red;
                        }

                        Carrot.Carrot_Box_Btn_Item btn_del = item_song.create_item();
                        btn_del.set_icon(this.app.carrot.sp_icon_del_data);
                        btn_del.set_color(this.app.carrot.color_highlight);
                        btn_del.set_act(() => this.app.player_music.playlist.delete_item(int_index_m));
                    }
                }

                if (this.app.player_music.get_id_song_current() == s_id_song)
                {
                    item_song.GetComponent<Image>().color = this.app.carrot.color_highlight;
                    item_song.txt_tip.color = Color.white;
                }

                item_song.set_act(() => this.act_play_song(data_song));
            }

            this.box_list.update_color_table_row();
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
        if (this.list_music_online != null)
        {
            for (int i = 0; i < this.list_music_online.Count; i++)
            {
                IDictionary song_data = this.list_music_online[i];
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

    public void show_list_radio()
    {
        this.type = Playlist_Type.radio;
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
                        IDictionary data_radio = SongSnapshot.ToDictionary();
                        data_radio["id"]= Get16BitHash(SongSnapshot.Id);
                        data_radio["id_web"] = SongSnapshot.Id;
                        data_radio["type"] = "radio";
                        list_radio.Add(data_radio);
                    }
                    this.list_music_cur = list_radio;
                    this.box_list_song(list_radio);
                }
                else
                {
                    this.app.carrot.show_msg("Radio", "Lits None");
                }
            }
        });
    }

    public void get_data_list_playlist(string lang="",UnityAction act_done=null)
    {
        this.app.carrot.show_loading();
        Query ChatQuery = null;
        if (lang=="")
        {
            ChatQuery = this.app.carrot.db.Collection("song").OrderByDescending("publishedAt").Limit(20);
        }
        else
        {
            ChatQuery = this.app.carrot.db.Collection("song").WhereEqualTo("lang", lang).OrderByDescending("publishedAt").Limit(20);
        }

        ChatQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot songQuerySnapshot = task.Result;

            if (task.IsFaulted)
            {
                this.app.carrot.hide_loading();
            }

            if (task.IsCompleted)
            {
                this.app.carrot.hide_loading();
                if (songQuerySnapshot.Count > 0)
                {
                    this.list_music_online = new List<IDictionary>();
                    int index_song = 0;
                    foreach (DocumentSnapshot SongSnapshot in songQuerySnapshot.Documents)
                    {
                        IDictionary data_music = SongSnapshot.ToDictionary();
                        data_music["id"] =Get16BitHash(SongSnapshot.Id);
                        data_music["id_web"] = SongSnapshot.Id;
                        data_music["type"] = "online";
                        data_music["index"] = index_song;
                        this.list_music_online.Add(data_music);
                        index_song++;
                    };

                    this.list_music_cur = this.list_music_online;
                    if (act_done != null) act_done();
                }
                else
                {
                    this.get_data_list_playlist("en", act_done);
                }
            }
        });
    }

    public static Int16 Get16BitHash(string s)
    {
        return (Int16)(s.GetHashCode() & 0xFFFF);
    }
}
