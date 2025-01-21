using UnityEngine;
using System;

namespace TextSpeech
{
    public class TextToSpeech : MonoBehaviour
    {
  
        public Action onStartCallBack;
        public Action onDoneCallback;
        public Action<string> onSpeakRangeCallback;

        public void Setting(string language, float _pitch, float _rate)
        {

        }
        public void StartSpeak(string _message)
        {

        }
        public void StopSpeak()
        {

        }

        public void onSpeechRange(string _message)
        {

        }
        public void onStart(string _message)
        {

        }
        public void onDone(string _message)
        {

        }
        public void onError(string _message)
        {
        }
        public void onMessage(string _message)
        {

        }

        public void onSettingResult(string _params)
        {

        }

    }
}