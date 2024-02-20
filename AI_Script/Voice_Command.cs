using TextSpeech;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

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

    private DictationRecognizer dictationRecognizer;

    void Start()
	{
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += (text, confidence) => {
                Debug.LogFormat("Content: {0}, level: {1}", text, confidence);
                this.OnFinalResult(text);
            };

            dictationRecognizer.DictationComplete += (completionCause) => {
                this.img_mic_inp_home.color = Color.white;
            };
        }
        else
        {
            SpeechToText.Instance.Setting("en-US");
            SpeechToText.Instance.onResultCallback = OnFinalResult;
            this.check_icon_input_command();
        }
    }

    void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
        }
    }

    public void btn_start()
    {
        this.inp_mic = null;
        this.app.textToSpeech.StopSpeak();
        this.img_mic_inp_home.color = this.app.carrot.color_highlight;
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
            dictationRecognizer.Start();
        }
        else
        {
            SpeechToText.Instance.StartRecording(PlayerPrefs.GetString("voice_command_ready", "Say something :-)"));
        }
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
        this.img_mic_inp_home.color = Color.white;

        if (this.inp_mic == null)
            this.app.command.send_chat(_data, true);
        else
            this.inp_mic.text = _data;
	}

	public void OnPartialResult(string _data)
	{
        this.img_mic_inp_home.color = Color.white;
        if (this.inp_mic == null)
            this.app.command.send_chat(_data, true);
        else
            this.inp_mic.text = _data;
    }

    public void start_inp_mic(InputField inp)
    {
        this.inp_mic = inp;
        this.app.textToSpeech.StopSpeak();
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