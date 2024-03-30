using Carrot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Playlist_Type {online,offline,radio,music_search_result}
public class Music_playlist : MonoBehaviour
{
    [Header("Obj Main")]
    public App app;

    public List<IDictionary> list_music_cur = null;
    private List<IDictionary> list_music_online = null;

    [Header("Asset Icon")]
    public Sprite icon;
    public Sprite icon_song;
    public Sprite icon_artist;
    public Sprite icon_album;
    public Sprite icon_year;
    public Sprite icon_genre;
    public Sprite icon_radio;
    public Sprite icon_music_online;
    public Sprite icon_mp3_file;
    private int length;

    [Header("Obj Other")]
    public GameObject btn_show_playlist;
    public GameObject btn_show_playlist_home;

    private Carrot.Carrot_Box box_list;
    private Carrot.Carrot_Window_Input box_search_inp;
    private Playlist_Type type;
    private OrderBy_Type type_order = OrderBy_Type.date_asc;

    private string s_buy_id_song = "";
    private string s_buy_link_mp3_song="";
    private string s_lang_cur = "";
    private string s_data_key_query_music = "";
    private string s_key_search_temp = "";
    private string s_data_json_radio_offline = "";

    private UnityAction act_done_get_new_playlist = null;
    private GameObject obj_item_more_temp = null;

    public void On_load()
    {
        if (this.app.carrot.is_offline()) this.s_data_json_radio_offline = PlayerPrefs.GetString("s_data_json_radio_offline");
        this.s_data_key_query_music = PlayerPrefs.GetString("key_query_music","");
        this.length = PlayerPrefs.GetInt("music_length");
        this.Check_show_btn_playlist();
    }

    private void Check_show_btn_playlist()
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

    public void Add_song(string s_id_m,string s_data_music, byte[] data_mp3, byte[] data_avatar)
    {
        if (data_mp3 != null) this.app.carrot.get_tool().PlayerPrefs_Save_by_data("music_" + this.length + "_mp3", data_mp3);
        if (data_avatar != null) this.app.carrot.get_tool().PlayerPrefs_Save_by_data("music_avatar" + s_id_m, data_avatar);

        PlayerPrefs.SetString("music_" + this.length, s_data_music);
        this.length++;
        PlayerPrefs.SetInt("music_length", this.length);
        this.Check_show_btn_playlist();
        this.app.carrot.Show_msg(app.carrot.L("music_list", "Music List"), app.carrot.L("add_music_list_success", "Added to your favorites list, you can listen to this song again without a network connection"));
    }

    public void show_playlist()
    {
        if (this.length > 0) {
            this.type = Playlist_Type.offline;
            if (this.box_list != null) this.box_list.close();
            this.list_music_cur = this.get_list_offline();
            this.Box_list_song(this.list_music_cur);
        }
        else
        {
            this.app.carrot.Show_msg(app.carrot.L("music_list", "Music Playlist"), app.carrot.L("list_none", "List is empty, no items found!"));
        }
    }

    private List<IDictionary> get_list_offline()
    {
        List<IDictionary> list_music_offline = new();
        for (int i = this.length - 1; i >= 0; i--)
        {
            string s_data = PlayerPrefs.GetString("music_" + i);
            if (s_data != "")
            {
                IDictionary data_music = (IDictionary)Json.Deserialize(s_data);
                data_music["type"] = "offline";
                data_music["index_del"] = i;
                list_music_offline.Add(data_music);
            }
        }
        return list_music_offline;
    }

    public IDictionary get_radom_one_song_offline()
    {
        List<IDictionary> list_song = this.get_list_offline();
        if (list_song.Count == 0) return null;
        int index_random = Random.Range(0, list_song.Count);
        return list_song[index_random];
    }

    private void act_play_song(IDictionary data_music)
    {
        this.app.player_music.act_play_data(data_music);
        if(this.box_search_inp!=null) this.box_search_inp.close();
        if (this.box_list != null) this.box_list.close();
    }

    public void delete_item(int index)
    {
        PlayerPrefs.DeleteKey("music_" + index);
        PlayerPrefs.DeleteKey("music_" + index+ "_mp3");
        this.show_playlist();
    }

    public void show_list_music_online()
    {
        if (this.app.carrot.is_online())
        {
            if (this.list_music_online != null)
            {
                this.type = Playlist_Type.online;
                this.Box_list_song(this.list_music_online);
            }
            else
            {
                this.get_data_list_playlist(this.app.carrot.lang.Get_key_lang(), this.show_list_music_online);
            }
        }
        else
        {
            this.show_playlist();
        }
    }

    public void btn_show_seach_music()
    {
        this.box_search_inp=this.app.carrot.show_search(Act_search_music,app.carrot.L("search_song_tip", "Enter the name of the song you want to listen to"));
    }

    private void Act_search_music(string key_search)
    {
        this.s_key_search_temp = key_search;
        this.type = Playlist_Type.music_search_result;
        this.app.carrot.show_loading();
        if (this.app.carrot.is_online())
        {
            StructuredQuery q = new("song");
            q.Set_where("name",Query_OP.EQUAL,key_search);
            q.Set_limit(20);
            this.app.carrot.server.Get_doc(q.ToJson(), Act_search_music_done, Act_search_music_fail);
        }
        else
        {
            this.app.carrot.hide_loading();
            IList list_song = this.get_list_song_offline_by_search_key(key_search);
            if (list_song != null)
                this.Box_list_song(list_song);
            else
                this.app.carrot.Show_msg(app.carrot.L("search_results", "Search Results"), app.carrot.L("no_song_found","No songs found!"), Msg_Icon.Alert);
        }
    }

    private void Act_search_music_done(string s_data)
    {
        Fire_Collection fc = new(s_data);
        this.app.carrot.hide_loading();
        if (fc.is_null)
        {
            Debug.Log("Search song oline data offline");
            IList list_song = this.get_list_song_online_by_search_key(this.s_key_search_temp);
            if (list_song.Count != 0)
                this.Box_list_song(list_song);
            else
                this.Box_list_song(this.get_list_song_offline_by_search_key(this.s_key_search_temp));
        }
        else
        {
            IList list_song = (IList)Json.Deserialize("[]");
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary data_music = fc.fire_document[i].Get_IDictionary();
                data_music["type"] = "online";
                list_song.Add(data_music);
            }
            this.Box_list_song(list_song);
        }
    }

    private void Act_search_music_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        IList list_song = this.get_list_song_online_by_search_key(this.s_key_search_temp);
        if (list_song != null) this.Box_list_song(list_song);
    }

    private void Box_list_song(IList list_song)
    {
        if (list_song.Count > 0)
        {
            if(this.box_search_inp!=null) this.box_search_inp.close();
            if (this.box_list != null) this.box_list.close();

            this.box_list = this.app.carrot.Create_Box("box_list");

            if (this.type == Playlist_Type.music_search_result)
            {
                this.box_list.set_title(app.carrot.L("search_results", "Search Results"));
                this.box_list.set_icon(this.app.carrot.icon_carrot_search);
            }

            if (this.type == Playlist_Type.offline)
            {
                this.box_list.set_title(app.carrot.L("music_list", "Music Playlist"));
                this.box_list.set_icon(this.icon);
            }

            if (this.type == Playlist_Type.online)
            {
                this.box_list.set_title(app.carrot.L("song_history", "List Music History"));
                this.box_list.set_icon(this.icon_music_online);
            }

            if (this.type == Playlist_Type.radio)
            {
                this.box_list.set_title(app.carrot.L("radio", "Radio"));
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

                if (data_song["type"].ToString() == "radio")
                {
                    Carrot.Carrot_Box_Item item_song = this.box_list.create_item("song_" + s_id_song);
                    var s_url_radio = data_song["url"].ToString();
                    item_song.set_icon(this.icon_radio);
                    item_song.set_title(s_name_m);
                    item_song.set_tip(s_url_radio);

                    string id_sp_radio = "radio_avatar_" + s_id_song;
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
                    item_song.set_act(() => this.act_play_song(data_song));
                }
                else
                {
                    this.Item_song(data_song);
                }
            }

            if (this.app.carrot.is_online())
            {
                if (this.type == Playlist_Type.online)
                {
                    Carrot_Box_Item item_more_music = this.box_list.create_item();
                    item_more_music.set_icon(this.icon);
                    item_more_music.set_title("Get more songs");
                    item_more_music.set_tip("Get more songs into playlists");
                    item_more_music.set_act(() => this.Act_get_more_song(item_more_music.gameObject));
                    item_more_music.set_lang_data("get_more_songs", "get_more_songs_tip");
                    item_more_music.load_lang_data();

                    Carrot_Box_Item item_order_music = this.box_list.create_item();
                    item_order_music.set_icon(this.app.command_storage.sp_icon_random);
                    item_order_music.set_title(PlayerPrefs.GetString("sort", "Sort")+" ("+this.type_order.ToString()+")");
                    item_order_music.set_tip(PlayerPrefs.GetString("sort_tip", "Randomize the order of songs in the list"));
                    item_order_music.set_act(() => this.show_list_song_by_random_order(item_order_music));
                }
            }
            this.box_list.update_color_table_row();
        }
        else
        {
            if (this.box_search_inp != null) this.box_search_inp.close();
            this.app.carrot.Show_msg(app.carrot.L("search_results", "Search Results"), "No songs found!", Carrot.Msg_Icon.Alert);
        }
    }

    private void Item_song(IDictionary data_song)
    {
        string s_id_song = data_song["id"].ToString();
        Carrot.Carrot_Box_Item item_song = this.box_list.create_item("song_" + s_id_song);
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
                this.app.carrot.get_img_and_save_playerPrefs(data_song["avatar"].ToString(), item_song.img_icon, id_sp_avatar_music);
            else
                item_song.set_icon(this.icon_song);
        }

        item_song.set_tip(data_song["artist"].ToString());
        item_song.set_title(data_song["name"].ToString());

        if (this.app.carrot.is_online())
        {
            var link_song_share = this.app.carrot.mainhost+"/?p=song&id="+s_id_song;
            Carrot.Carrot_Box_Btn_Item btn_share = item_song.create_item();
            btn_share.set_icon(this.app.carrot.sp_icon_share);
            btn_share.set_color(this.app.carrot.color_highlight);
            btn_share.set_act(() => this.app.carrot.show_share(link_song_share, app.carrot.L("share_song", "Share this song so everyone can hear it!")));

            if (PlayerPrefs.GetInt("is_buy_song_" + s_id_song, 0) == 1)
            {
                Carrot.Carrot_Box_Btn_Item btn_download = item_song.create_item();
                btn_download.set_icon(this.app.carrot.icon_carrot_download);
                btn_download.set_color(this.app.carrot.color_highlight);
                btn_download.set_act(() => Application.OpenURL(data_song["mp3"].ToString()));
            }
            else
            {
                Carrot.Carrot_Box_Btn_Item btn_buy = item_song.create_item();
                btn_buy.set_icon(this.icon_mp3_file);
                btn_buy.set_color(this.app.carrot.color_highlight);
                btn_buy.set_act(() => this.act_buy_mp3(s_id_song, data_song["mp3"].ToString()));
            }
        }

        if (this.type == Playlist_Type.offline)
        {
            Carrot.Carrot_Box_Btn_Item btn_del = item_song.create_item();
            btn_del.set_icon(this.app.carrot.sp_icon_del_data);
            btn_del.set_color(this.app.carrot.color_highlight);
            btn_del.set_act(() => this.app.player_music.playlist.delete_item(int.Parse(data_song["index_del"].ToString())));
        }

        item_song.set_act(() => this.act_play_song(data_song));
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
        if (this.s_data_json_radio_offline == "")
        {
            this.app.carrot.show_loading();
            this.type = Playlist_Type.radio;
            StructuredQuery q = new("radio");
            q.Set_limit(20);
            this.app.carrot.server.Get_doc(q.ToJson(), Act_show_list_radio_done, Act_show_list_radio_fail);
        }
        else
        {
            this.Act_load_list_radio(this.s_data_json_radio_offline);
        }
    }

    private void Act_show_list_radio_done(string s_data)
    {
        PlayerPrefs.SetString("s_data_json_radio_offline",s_data);
        this.s_data_json_radio_offline = s_data;
        this.Act_load_list_radio(s_data);
    }

    private void Act_show_list_radio_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        if (this.s_data_json_radio_offline != "") 
            this.Act_load_list_radio(this.s_data_json_radio_offline);
        else
            this.app.carrot.Show_msg("Radio", s_error);
    }

    private void Act_load_list_radio(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            if (this.box_list != null) this.box_list.close();
            this.box_list = this.app.carrot.Create_Box("Radio");
            this.box_list.set_icon(this.icon_radio);
            List<IDictionary> list_radio = new();

            for (int i = 0; i < fc.fire_document.Length; i++)
            {
                IDictionary data_radio = fc.fire_document[i].Get_IDictionary();
                data_radio["type"] = "radio";
                list_radio.Add(data_radio);
            }

            this.list_music_cur = list_radio;
            this.Box_list_song(list_radio);
        }
        else
        {
            this.app.carrot.Show_msg("Radio", "Lits None");
        }
    }

    public void get_data_list_playlist(string lang="",UnityAction act_done=null)
    {
        this.app.carrot.show_loading();
        this.s_lang_cur = lang;
        this.act_done_get_new_playlist = act_done;
        StructuredQuery q = new("song");
        q.Set_limit(20);
        q.Add_order("publishedAt", Query_Order_Direction.DESCENDING);
        if (lang != "") q.Add_where("lang", Query_OP.EQUAL, lang);
        this.app.carrot.server.Get_doc(q.ToJson(), get_data_list_playlist_done, get_data_list_playlist_fail);
    }

    private void get_data_list_playlist_fail(string s_error)
    {
        this.app.carrot.hide_loading();
    }

    private void get_data_list_playlist_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);

        if (!fc.is_null)
        {
            this.list_music_online = new List<IDictionary>();
            int index_song = 0;
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary data_music = fc.fire_document[i].Get_IDictionary();
                data_music["type"] = "online";
                data_music["index"] = index_song;
                this.list_music_online.Add(data_music);
                index_song++;
            }
            this.list_music_cur = this.list_music_online;
            act_done_get_new_playlist?.Invoke();
        }
        else
        {
            this.get_data_list_playlist("en", this.act_done_get_new_playlist);
        }
    }

    private void act_buy_mp3(string s_id_song,string s_link_mp3)
    {
        this.s_buy_id_song = s_id_song;
        this.s_buy_link_mp3_song = s_link_mp3;
        this.app.carrot.shop.buy_product(1);
    }

    public void On_pay_success()
    {
        if (this.s_buy_id_song != "")
        {
            Application.OpenURL(this.s_buy_link_mp3_song);
            PlayerPrefs.SetInt("is_buy_song_" + this.s_buy_id_song, 1);
            this.s_buy_id_song = "";
            this.s_buy_link_mp3_song = "";
        }
    }

    private void Act_get_more_song(GameObject obj_item_more)
    {
        this.app.carrot.play_sound_click();
        this.app.carrot.show_loading();
        this.obj_item_more_temp = obj_item_more;

        StructuredQuery q = new("song");
        q.Set_limit(20);
        q.Add_order("publishedAt", Query_Order_Direction.DESCENDING);
        this.app.carrot.server.Get_doc(q.ToJson(), Act_get_more_song_done, Act_get_more_song_fail);
    }

    private void Act_get_more_song_done(string s_data)
    {
        Fire_Collection fc = new(s_data);

        int index_next = this.list_music_cur.Count;
        this.app.carrot.hide_loading();
        if (!fc.is_null)
        {
            for (int i = 0; i < fc.fire_document.Length; i++)
            {
                IDictionary data_music = fc.fire_document[i].Get_IDictionary();
                data_music["type"] = "online";
                data_music["index"] = index_next;
                index_next++;
                this.Item_song(data_music);
            }
            if (this.box_list != null) this.box_list.update_color_table_row();
        }
        if (this.obj_item_more_temp != null) Destroy(this.obj_item_more_temp);
    }

    private void Act_get_more_song_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        if(this.obj_item_more_temp!=null)Destroy(this.obj_item_more_temp);
    }

    private void show_list_song_by_random_order(Carrot_Box_Item item_order_music)
    {
        if (this.type_order == OrderBy_Type.date_asc) this.type_order = OrderBy_Type.date_desc;
        else if(this.type_order == OrderBy_Type.date_desc) this.type_order = OrderBy_Type.name_desc;
        else if (this.type_order == OrderBy_Type.name_desc) this.type_order = OrderBy_Type.name_asc;
        else this.type_order = OrderBy_Type.date_asc;
        item_order_music.set_title("Sort (" + this.type_order.ToString() + ")");
        this.get_data_list_by_order(this.s_lang_cur);
    }

    private void get_data_list_by_order(string lang = "")
    {
        this.app.carrot.show_loading();

        StructuredQuery q = new("song");
        q.Set_limit(20);
        if (lang == "")
            q.Add_order("publishedAt", Query_Order_Direction.DESCENDING);
        else
            q.Add_where("lang",Query_OP.EQUAL,lang);

        if(this.type_order==OrderBy_Type.date_desc)
            q.Add_order("publishedAt", Query_Order_Direction.DESCENDING);
        else if(this.type_order==OrderBy_Type.date_asc)
            q.Add_order("publishedAt", Query_Order_Direction.ASCENDING);
        else if(this.type_order==OrderBy_Type.name_desc)
            q.Add_order("name", Query_Order_Direction.DESCENDING);
        else
            q.Add_order("name", Query_Order_Direction.ASCENDING);

        this.app.carrot.server.Get_doc(q.ToJson(), Act_get_data_list_by_order_done, Act_get_data_list_by_order_fail);
    }

    private void Act_get_data_list_by_order_done(string s_data)
    {
        this.app.carrot.hide_loading();
        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            this.list_music_online = new List<IDictionary>();
            int index_song = 0;
            for(int i=0;i<fc.fire_document.Length;i++)
            {
                IDictionary data_music = fc.fire_document[i].Get_IDictionary();
                data_music["type"] = "online";
                data_music["index"] = index_song;
                this.list_music_online.Add(data_music);
                index_song++;
            };

            this.list_music_cur = this.list_music_online;
            this.Box_list_song(this.list_music_cur);
        }
    }

    private void Act_get_data_list_by_order_fail(string s_error)
    {
        this.app.carrot.hide_loading();
    }

    public bool Check_query_key(string s_key)
    {
        if (this.s_data_key_query_music == "") return false;

        string[] list_key_query_search = this.s_data_key_query_music.Split(";");
        if (list_key_query_search.Length==0) return false;

        for(int i = 0; i < list_key_query_search.Length; i++)
        {
            string s_query_search = list_key_query_search[i].ToString();
            if (s_key.Contains(s_query_search))
            {
                if (s_key.IndexOf(s_query_search) == 0)
                {
                    if (s_key.Length > s_query_search.Length)
                    {
                        string s_name_song = s_key.Replace(s_query_search, "");
                        s_name_song = s_name_song.Trim();
                        Debug.Log("check_query_key true :" + s_name_song);
                        if(this.app.carrot.is_online())
                            this.get_song_by_name(s_name_song);
                        else
                            this.Search_and_play_offline(s_name_song);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void get_song_by_name(string s_name)
    {
        this.app.carrot.show_loading();
        StructuredQuery q = new StructuredQuery("song");
        q.Add_where("name", Query_OP.EQUAL, s_name);
        q.Set_limit(1);
        this.app.carrot.server.Get_doc(q.ToJson(), Get_song_by_name_done, Get_song_by_name_fail);
    }

    private void Get_song_by_name_done(string s_data)
    {
        this.app.carrot.hide_loading();

        Fire_Collection fc = new(s_data);
        if (!fc.is_null)
        {
            IDictionary data_music = fc.fire_document[0].Get_IDictionary();
            data_music["type"] = "online";
            data_music["index"] = 0;
            this.app.player_music.act_play_data(data_music);
            this.app.command.send_chat_no_father("found_song");
            return;
        }
        else
        {
            this.app.command.send_chat_no_father("no_found_song");
        }
    }

    private void Get_song_by_name_fail(string s_error)
    {
        this.app.carrot.hide_loading();
        this.app.command.send_chat_no_father("no_found_song");
    }

    public string Get_data_key_query_music()
    {
        return this.s_data_key_query_music;
    }

    public void Set_data_key_query_music(string s_keys)
    {
        this.s_data_key_query_music = s_keys;
        PlayerPrefs.SetString("key_query_music", s_keys);
    }

    private void Search_and_play_offline(string s_key)
    {
        for (int i = this.length - 1; i >= 0; i--)
        {
            string s_data = PlayerPrefs.GetString("music_" + i);
            if (s_data != "")
            {
                IDictionary data_music = (IDictionary)Carrot.Json.Deserialize(s_data);
                if (data_music["name"].ToString() == s_key)
                {
                    data_music["type"] = "offline";
                    data_music["index_del"] = i;
                    this.app.player_music.act_play_data(data_music);
                }
            }
        }
    }
}
