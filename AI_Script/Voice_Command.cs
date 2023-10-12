using TextSpeech;
using UnityEngine;
using UnityEngine.UI;

public class Voice_Command : MonoBehaviour
{
	[Header("Obj main")]
	public App app;

    [Header("Voice Obj")]
	public Image img_mic_inp_home;
	public Sprite icon_mic_suport;
	public Sprite icon_mic_nosuport;
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
        if (this.app.command.mode == Command_Type_Mode.chat)
            this.app.command.mode = Command_Type_Mode.live;
        else
            this.app.command.mode = Command_Type_Mode.chat;

        this.check_icon_input_command();
    }

    private void check_icon_input_command()
    {
        if (this.app.command.mode == Command_Type_Mode.chat)
            this.img_mic_inp_home.sprite = this.icon_mic_suport;
        else
            this.img_mic_inp_home.sprite = this.app.carrot.icon_carrot_write;
    }
}