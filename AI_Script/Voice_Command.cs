using TextSpeech;
using UnityEngine;
using UnityEngine.UI;

public class Voice_Command : MonoBehaviour
{
	[Header("Obj main")]
	public App app;

    [Header("Voice Obj")]
	public Image img_mic_inp_home;
    public Image img_mic_fun_brain;
    public Sprite icon_mic_chat;
    public Sprite icon_mic_live;
    private InputField inp_mic = null;

    void Start()
	{
        SpeechToText.Instance.Setting("en-US");
        SpeechToText.Instance.onResultCallback = OnFinalResult;
        this.check_icon_input_command();
    }

    public void btn_start()
    {
        this.inp_mic = null;
        SpeechToText.Instance.StartRecording(PlayerPrefs.GetString("voice_command_ready", "Say something :-)"));
    }

    public void btn_stop()
    {
        SpeechToText.Instance.StopRecording();
    }

    public void set_DetectionLanguage(string s_key_lang)
    {
        SpeechToText.Instance.Setting(s_key_lang);
        TextToSpeech.Instance.Setting(s_key_lang,this.app.setting.get_voice_speed(), 1);
    }

    void OnFinalResult(string _data)
	{
        if (this.inp_mic == null)
            this.app.command.send_chat(_data, true);
        else
            this.inp_mic.text = _data;
	}

	public void OnPartialResult(string _data)
	{
        if (this.inp_mic == null)
            this.app.command.send_chat(_data, true);
        else
            this.inp_mic.text = _data;
    }

    public void start_inp_mic(InputField inp)
    {
        this.inp_mic = inp; 
        SpeechToText.Instance.StartRecording(PlayerPrefs.GetString("voice_command_ready", "Say something :-)"));
    }

    public void change_mode()
    {
        this.app.carrot.play_sound_click();
        string s_status_live;
        if (this.app.command.mode == Command_Type_Mode.chat)
        {
            this.app.command.mode = Command_Type_Mode.live;
            s_status_live = PlayerPrefs.GetString("setting_on", "On");
            this.app.live.on_live();
        }
        else
        {
            this.app.command.mode = Command_Type_Mode.chat;
            s_status_live = PlayerPrefs.GetString("setting_off", "Off");
            this.app.live.off_live();
        }

        this.app.carrot.show_msg(PlayerPrefs.GetString("chat_narrative", "Chat narration"), s_status_live, Carrot.Msg_Icon.Alert);
        this.check_icon_input_command();
    }

    private void check_icon_input_command()
    {
        if (this.app.command.mode == Command_Type_Mode.chat)
        {
            this.img_mic_inp_home.sprite = this.icon_mic_chat;
            this.img_mic_fun_brain.sprite = this.icon_mic_live;
        }
        else
        {
            this.img_mic_inp_home.sprite = this.icon_mic_live;
            this.img_mic_fun_brain.sprite = this.icon_mic_chat;
        }
    }

    public void on_input_mode_chat()
    {
        this.app.command.mode = Command_Type_Mode.chat;
        this.check_icon_input_command();
    }
}