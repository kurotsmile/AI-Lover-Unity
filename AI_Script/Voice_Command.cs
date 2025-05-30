using KKSpeech;
using TextSpeech;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_ANDROID
using UnityEngine.Windows.Speech;
#endif

public class Voice_Command : MonoBehaviour
{
    [Header("Obj main")]
    public App app;
    public SpeechRecognizerListener listener;

    [Header("Voice Obj")]
    public Image img_mic_inp_home;
    public Image img_mic_fun_brain;
    private InputField inp_mic = null;

    [Header("Assets")]
    public Sprite icon_voice;

#if !UNITY_ANDROID
    private DictationRecognizer dictationRecognizer;
#endif

    void Start()
    {
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
#if !UNITY_ANDROID
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += (text, confidence) => {
                Debug.LogFormat("Content: {0}, level: {1}", text, confidence);
                this.OnFinalResult(text);
            };

            dictationRecognizer.DictationComplete += (completionCause) => {
                this.img_mic_inp_home.color = Color.white;
            };
#endif
        }
        else
        {
            if (SpeechRecognizer.ExistsOnDevice())
            {
                listener.onFinalResults.AddListener(OnFinalResult);
                listener.onPartialResults.AddListener(OnPartialResult);
                listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
                SpeechRecognizer.RequestAccess();
                SpeechRecognizer.SetDetectionLanguage(this.app.carrot.lang.Val("key_voice","en-US"));
            }
        }
    }

#if !UNITY_ANDROID
    void OnDestroy()
    {
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
            if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
                dictationRecognizer.Dispose();
            }
        }
    }
#endif

    public void btn_start()
    {
        this.inp_mic = null;
        this.app.textToSpeech.StopSpeak();
        
        if (this.app.carrot.os_app == Carrot.OS.Window)
        {
#if !UNITY_ANDROID
            dictationRecognizer.Start();
#endif
            this.img_mic_inp_home.color = this.app.carrot.color_highlight;
        }
        else
        {
            if (SpeechRecognizer.IsRecording())
            {
#if UNITY_IOS && !UNITY_EDITOR
                        SpeechRecognizer.StopIfRecording();
#elif UNITY_ANDROID && !UNITY_EDITOR
                        SpeechRecognizer.StopIfRecording();
#endif
                this.img_mic_inp_home.color =Color.white;
            }
            else
            {
                SpeechRecognizer.StartRecording(true);
                this.img_mic_inp_home.color = this.app.carrot.color_highlight;
            }
        }
    }

    public void btn_stop()
    {
        if (SpeechRecognizer.IsRecording())
        {
    #if UNITY_IOS && !UNITY_EDITOR
                SpeechRecognizer.StopIfRecording();
    #elif UNITY_ANDROID && !UNITY_EDITOR
                SpeechRecognizer.StopIfRecording();
    #endif
        }
        this.img_mic_inp_home.color =Color.white;
    }

    public void set_DetectionLanguage(string s_key_lang)
    {
        SpeechRecognizer.SetDetectionLanguage(this.app.carrot.lang.Val("key_voice","en-US"));
        //TextToSpeech.Instance.Setting(s_key_lang, this.app.setting.get_voice_speed(), 1);
    }

    public void OnFinalResult(string _data)
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
        SpeechRecognizer.StartRecording(true);
    }

    public void OnEndOfSpeech()
    {
        this.img_mic_inp_home.color = Color.white;
    }
}