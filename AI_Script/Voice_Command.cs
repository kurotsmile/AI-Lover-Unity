using Carrot;
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
        TextToSpeech.Instance.Setting(s_key_lang, 1, 1);
    }

    void OnFinalResult(string _data)
	{
        if (this.inp_mic == null)
        {
            this.app.command.send_chat(_data, true);
        }
        else
        {
            this.inp_mic.text = _data;
        }
	}

	public void OnPartialResult(string _data)
	{
        if (this.inp_mic == null)
        {
            this.app.command.send_chat(_data, true);
        }
        else
        {
            this.inp_mic.text = _data;
        }
    }

    public void show_list_SupportedLanguages()
    {
        Carrot_Box box_supported_langs = this.app.carrot.Create_Box("list_supported_langs");
        box_supported_langs.set_title("list Supported Languages");
        /*
        foreach (LanguageOption l in this.languageOptions)
        {
            Carrot_Box_Item item_lang = box_supported_langs.create_item("item_lang_" + l.id);
            item_lang.set_icon(this.app.carrot.lang.icon);
            item_lang.set_title(l.displayName);
            item_lang.set_tip(l.displayName);
        }*/
    }


    public void start_inp_mic(InputField inp)
    {
        this.inp_mic = inp;
        SpeechToText.Instance.StartRecording(PlayerPrefs.GetString("voice_command_ready", "Say something :-)"));
    }
}