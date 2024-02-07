using Carrot;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiAPI : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("AI Chat Obj")]
    public string key_api_default;
    public bool is_active = true;
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    private string key_api;

    public void on_load()
    {
        this.key_api = PlayerPrefs.GetString("key_api_ai_gemini", this.key_api_default);
        if (PlayerPrefs.GetInt("is_active_gemini",0)==0)
            this.is_active= true;
        else
            this.is_active= false;
    }

    IEnumerator PostRequest(string userMessage)
    {
        Debug.Log("Get chat Gemini(" + userMessage + ")");
        if (this.key_api.Trim() == "") this.key_api = this.key_api_default;

        string requestData = "{\"contents\":[{\"parts\":[{\"text\":\""+ userMessage + "\"}]}]}";
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(requestData);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiEndpoint+ "?key=" + this.key_api, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                IDictionary gemini_ai = (IDictionary)Json.Deserialize(www.downloadHandler.text);
                IList candidates = (IList)gemini_ai["candidates"];
                IDictionary candidate = (IDictionary) candidates[0];
                IDictionary content = (IDictionary) candidate["content"];
                IList parts = (IList) content["parts"];
                IDictionary chat_ai = (IDictionary)parts[0];

                chat_ai["id"] = "chat" + this.app.carrot.generateID();
                chat_ai["func"] = "0";
                chat_ai["status"] = "pending";
                chat_ai["key"] = userMessage;
                chat_ai["msg"] = chat_ai["text"].ToString();
                chat_ai["face"] = UnityEngine.Random.Range(0, 18).ToString();
                chat_ai["action"] = UnityEngine.Random.Range(0, this.app.action.list_anim_act_defalt.Length).ToString();

                Color color_icon = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                chat_ai["color"] = "#" + ColorUtility.ToHtmlStringRGBA(color_icon);

                chat_ai["sex_user"] = this.app.setting.get_user_sex();
                chat_ai["sex_character"] = this.app.setting.get_character_sex();
                chat_ai["date_create"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                chat_ai["link"] = "";
                chat_ai["lang"] = this.app.carrot.lang.get_key_lang();
                chat_ai["icon"] = "";
                chat_ai["pater"] = "";
                chat_ai["mp3"] = "";
                chat_ai["user"] = null;

                chat_ai["text"] = null;
                chat_ai["ai"] = "Gemini";

                this.app.command.act_chat(chat_ai);
                this.app.command_storage.add_command_offline(chat_ai);
            }
            else
            {
                Debug.Log($"Error: {www.error}");
                if (this.app.setting.get_index_prioritize() == 0)
                {
                    this.app.command.show_msg_no_chat();
                }else if (this.app.setting.get_index_prioritize() == 1)
                {
                    if(this.app.open_AI.is_active)
                        this.app.open_AI.send_chat(userMessage);
                    else
                        this.app.command.show_msg_no_chat();
                }
                else if (this.app.setting.get_index_prioritize() == 2)
                {
                    this.app.command.show_msg_no_chat();
                }else if (this.app.setting.get_index_prioritize() == 3)
                {
                    if (this.app.open_AI.is_active)
                        this.app.open_AI.send_chat(userMessage);
                    else
                        this.app.command.show_msg_no_chat();
                }
                
            }
        }
    }

    public void send_chat(string s_key)
    {
        IDictionary chat_offline = this.app.command_storage.act_call_cm_offline(s_key, "");
        if (chat_offline != null)
            this.app.command.act_chat(chat_offline);
        else
            StartCoroutine(PostRequest(s_key));
    }

    public void set_key_api(string s_key)
    {
        if (s_key.Trim() != "")
            this.key_api = s_key;
        else
            this.key_api = this.key_api_default;
    }

    public void stop_All_Action()
    {
        this.StopAllCoroutines();
    }
}
