using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Carrot;
using System;

public class OpenAIChatbot : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    [Header("AI Chat Obj")]
    public string[] lis_api_key;
    public bool is_active = true;

    private const string openaiEndpoint = "https://api.openai.com/v1/chat/completions";

    private string key_api;

    public void on_load()
    {
        this.key_api = PlayerPrefs.GetString("key_api_ai_gpt", this.get_key_api_random());
        if (PlayerPrefs.GetInt("is_active_gpt", 0) == 0)
            this.is_active = true;
        else
            this.is_active = false;
    }

    IEnumerator PostRequest(string userMessage)
    {
        if (this.key_api.Trim() == "") this.key_api = this.get_key_api_random();

        string requestData = "{\"model\": \"gpt-3.5-turbo\",\"messages\":[{\"role\": \"user\",\"content\": \""+userMessage+"\"}]}";
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(requestData);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(openaiEndpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {key_api}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                IDictionary chat_ai = (IDictionary)Json.Deserialize(www.downloadHandler.text);
                IList choices = (IList)chat_ai["choices"];

                if (choices.Count > 0)
                {
                    IDictionary choice = (IDictionary)choices[0];
                    IDictionary message = (IDictionary)choice["message"];

                    chat_ai["func"] = "0";
                    chat_ai["status"] = "pending";
                    chat_ai["key"] = userMessage;
                    chat_ai["msg"] = message["content"].ToString();
                    chat_ai["face"] = UnityEngine.Random.Range(0, 18).ToString();
                    chat_ai["action"] = UnityEngine.Random.Range(0, 41).ToString();

                    Color color_icon = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    chat_ai["color"]= "#" + ColorUtility.ToHtmlStringRGBA(color_icon);

                    chat_ai["sex_user"] = this.app.setting.get_user_sex();
                    chat_ai["sex_character"] = this.app.setting.get_character_sex();
                    chat_ai["date_create"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    chat_ai["link"] = "";
                    chat_ai["lang"] = this.app.carrot.lang.get_key_lang();
                    chat_ai["icon"] = "";
                    chat_ai["pater"] = "";
                    chat_ai["mp3"] = "";
                    chat_ai["user"] =null;

                    chat_ai["usage"] = null;
                    chat_ai["choices"] = null;
                    chat_ai["created"] = null;

                    this.app.command.act_chat(chat_ai);
                    this.app.command_storage.add_command_offline(chat_ai);
                }
                else
                {
                    this.app.command.show_msg_no_chat();
                }

            }
            else
            {
                Debug.LogError($"Error: {www.error}");
                if(this.app.gemini_AI.is_active)
                    this.app.gemini_AI.send_chat(userMessage);
                else
                    this.app.command.show_msg_no_chat();
            }
        }
    }

    public void send_chat(string s_key)
    {
        IDictionary chat_offline = this.app.command_storage.act_call_cm_offline(s_key,"");
        if (chat_offline != null)
            this.app.command.act_chat(chat_offline);
        else
            StartCoroutine(PostRequest(s_key));
    }

    private string get_key_api_random()
    {
        int index_random = UnityEngine.Random.Range(0, this.lis_api_key.Length);
        return this.lis_api_key[index_random];
    }

    public void set_key_api(string s_key)
    {
        if (s_key.Trim() != "")
            this.key_api = s_key;
        else
            this.key_api = this.get_key_api_random();
    }

    public void stop_All_Action()
    {
        this.StopAllCoroutines();
    }
}