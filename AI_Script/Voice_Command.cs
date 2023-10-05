using Carrot;
using System.Xml.Linq;
using TextSpeech;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

#if UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#endif

public class Voice_Command : MonoBehaviour
{
	[Header("Obj main")]
	public App app;

    [Header("Voice Obj")]
	public Image img_mic_inp_home;
	public Image img_mic_voice_command;
	public Sprite icon_mic_suport;
	public Sprite icon_mic_nosuport;
	public Sprite icon_mic_pause;
	public Sprite icon_mic_recoding;
	public GameObject panel_voice_command;
	public GameObject panel_voice_input;
	public Button startRecordingButton;
	public Text resultText;
    public Text txt_status;

    private bool is_act = false;
	private bool is_suport=false;
	private bool is_ready=false;

    void Start()
	{
        this.panel_voice_command.SetActive(false);
        this.panel_voice_input.SetActive(true);

        SpeechToText.Instance.Setting("en-US");
        SpeechToText.Instance.onResultCallback = OnFinalResult;

        /*
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
        if (this.app.carrot.is_online())
		{
            if (this.app.carrot.store_public == Carrot.Store.Huawei_store)
            {
                this.img_mic_inp_home.sprite = this.app.carrot.icon_carrot_write;
            }
			else
			{
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) Permission.RequestUserPermission(Permission.Microphone);
                this.panel_voice_command.SetActive(false);
                this.panel_voice_input.SetActive(true);
                if (SpeechRecognizer.ExistsOnDevice())
                {
                    listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);
                    listener.onAvailabilityChanged.AddListener(OnAvailabilityChange);
                    listener.onErrorDuringRecording.AddListener(OnError_recoding);
                    listener.onErrorOnStartRecording.AddListener(OnError_start);
                    listener.onFinalResults.AddListener(OnFinalResult);
                    listener.onPartialResults.AddListener(OnPartialResult);
                    listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
                    listener.onSupportedLanguagesFetched.AddListener(OnSupportedLanguagesFetched);
                    startRecordingButton.enabled = false;
                    SpeechRecognizer.RequestAccess();
                    this.is_suport = true;
                    this.img_mic_inp_home.sprite = this.icon_mic_suport;
                    this.img_mic_voice_command.sprite = this.icon_mic_suport;
                    SpeechRecognizer.GetSupportedLanguages();
                }
                else
                {
                    resultText.text = PlayerPrefs.GetString("voice_command_no_support", "Sorry, but this device doesn't support speech recognition");
                    this.txt_status.text = PlayerPrefs.GetString("voice_command_stop", "Pause to receive commands to listen to the character's response");
                    startRecordingButton.enabled = false;
                    this.is_suport = false;
                    this.img_mic_inp_home.sprite = this.icon_mic_nosuport;
                    this.img_mic_voice_command.sprite = this.icon_mic_nosuport;
                }
            }
		}
		else
		{
			this.img_mic_inp_home.sprite = this.app.carrot.icon_carrot_write;
		}
        */
    }

    public void btn_start()
    {
        /*
		if (this.app.carrot.is_online())
		{
			if (this.app.carrot.store_public == Carrot.Store.Huawei_store)
			{
                this.app.command.inp_command.Select();
                return;
			}

            if (this.is_suport)
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) Permission.RequestUserPermission(Permission.Microphone);
                this.On_start_voice_command();
            }
            else
            {
                resultText.text = PlayerPrefs.GetString("voice_command_no_support", "Sorry, but this device doesn't support speech recognition");
                this.panel_voice_command.SetActive(true);
                this.panel_voice_input.SetActive(false);
            }
		}
		else
		{
			this.app.command.inp_command.Select();
		}
        */
        SpeechToText.Instance.StartRecording(PlayerPrefs.GetString("voice_command_ready", "Say something :-)"));
    }

    public void btn_stop()
    {
        /*
		if(this.is_suport){
			this.On_stop_voice_command();
		}else{
			this.panel_voice_command.SetActive(false);
			this.panel_voice_input.SetActive(true);
		}
        */
        SpeechToText.Instance.StopRecording();
    }

    public void check_start_voice_commad()
    {
        if (this.is_act){
			this.On_start_voice_command();
			this.is_ready=true;
		}
    }

    public void check_stop_voice_commad()
    {
        if (this.is_act){
			 this.On_pause_voice_command();
			 this.is_ready=false;
		}
    }

    public void set_DetectionLanguage(string s_key_lang)
    {
        this.lang_Detection(s_key_lang);
    }

    private void lang_Detection(string s_lang){
        SpeechToText.Instance.Setting(s_lang);
        TextToSpeech.Instance.Setting(s_lang,1, 1);
    }

    void OnFinalResult(string _data)
	{
        this.app.command.send_chat(_data, true);
	}

	public void OnPartialResult(string _data)
	{
        this.app.command.send_chat(_data, true);
    }

	public void OnAvailabilityChange(bool available)
	{
		startRecordingButton.enabled = available;
		if (!available)
		{
			resultText.text =PlayerPrefs.GetString("voice_command_error", "Speech Recognition not available");
        }
		else
		{
			resultText.text =PlayerPrefs.GetString("voice_command_ready", "Say something :-)");
        }
	}



	public void OnEndOfSpeech()
	{
		this.txt_status.text = "";
	}

	public void OnError_start(string error)
	{
		Debug.LogError(error);
    }

	public void OnError_recoding(string error)
	{
		resultText.text = PlayerPrefs.GetString("voice_command_error", "Something went wrong... Try again!");
        this.txt_status.text = "";
		this.img_mic_voice_command.sprite=this.icon_mic_suport;
    }

	private void On_start_voice_command(){
		if (this.GetComponent<App>().setting.get_status_sound_voice()) this.GetComponent<Command>().sound_command.Stop();
		this.img_mic_voice_command.sprite=this.icon_mic_recoding;
        resultText.text = PlayerPrefs.GetString("voice_command_ready", "Say something :-)");
        this.txt_status.text = PlayerPrefs.GetString("voice_command_start", "When it's your turn to talk, speak into the microphone");
        this.panel_voice_command.SetActive(true);
        this.panel_voice_input.SetActive(false);
		this.is_act = true;
	}

	private void On_stop_voice_command(){

        resultText.text = "";
        this.txt_status.text = PlayerPrefs.GetString("voice_command_stop", "Pause to receive commands to listen to the character's response");
        this.panel_voice_command.SetActive(false);
        this.panel_voice_input.SetActive(true);
		this.is_act = false;
		this.StopAllCoroutines();
	}

	private void On_pause_voice_command(){
        resultText.text = "";
        this.txt_status.text = PlayerPrefs.GetString("voice_command_stop", "Pause to receive commands to listen to the character's response");
		this.img_mic_voice_command.sprite=this.icon_mic_pause;
	}

	public void On_try_start_voice_command(){
		if(this.is_suport&&this.is_ready) this.On_start_voice_command();
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

}