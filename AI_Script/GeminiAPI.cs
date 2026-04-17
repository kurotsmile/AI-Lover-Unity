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
        if (PlayerPrefs.GetInt("is_active_gemini", 0) == 0)
            this.is_active = true;
        else
            this.is_active = false;
    }

    IEnumerator PostRequest(string userMessage)
    {
        Debug.Log("Get chat Gemini(" + userMessage + ")");
        if (this.key_api.Trim() == "") this.key_api = this.key_api_default;

        IDictionary requestChat = (IDictionary)Json.Deserialize("{}");
        IList contents = new ArrayList();

        IDictionary systemContent = (IDictionary)Json.Deserialize("{}");
        IList systemParts = new ArrayList();
        IDictionary systemPart = (IDictionary)Json.Deserialize("{}");
        systemPart["text"] = this.app.tool.Get_ai_assistant_system_prompt();
        systemParts.Add(systemPart);
        systemContent["role"] = "user";
        systemContent["parts"] = systemParts;
        contents.Add(systemContent);

        IDictionary userContent = (IDictionary)Json.Deserialize("{}");
        IList userParts = new ArrayList();
        IDictionary userPart = (IDictionary)Json.Deserialize("{}");
        userPart["text"] = userMessage;
        userParts.Add(userPart);
        userContent["role"] = "user";
        userContent["parts"] = userParts;
        contents.Add(userContent);

        requestChat["contents"] = contents;
        string requestData = Json.Serialize(requestChat);
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(requestData);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiEndpoint + "?key=" + this.key_api, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                IDictionary gemini_ai = (IDictionary)Json.Deserialize(www.downloadHandler.text);
                if (gemini_ai == null || gemini_ai["candidates"] == null)
                {
                    this.app.command.show_msg_no_chat();
                    yield break;
                }

                IList candidates = (IList)gemini_ai["candidates"];
                if (candidates.Count == 0)
                {
                    this.app.command.show_msg_no_chat();
                    yield break;
                }

                IDictionary candidate = (IDictionary)candidates[0];
                if (candidate == null || candidate["content"] == null)
                {
                    this.app.command.show_msg_no_chat();
                    yield break;
                }

                IDictionary content = (IDictionary)candidate["content"];
                if (content["parts"] == null)
                {
                    this.app.command.show_msg_no_chat();
                    yield break;
                }

                IList parts = (IList)content["parts"];
                IDictionary chat_ai = parts.Count > 0 ? (IDictionary)parts[0] : null;
                string s_content = chat_ai != null && chat_ai["text"] != null ? chat_ai["text"].ToString() : "";
                chat_ai = this.app.tool.Parse_ai_response_to_chat_data(s_content, userMessage, "Gemini");

                this.app.command.act_chat(chat_ai);
                //this.app.command_storage.add_command_offline(chat_ai);
            }
            else
            {
                Debug.Log($"Error: {www.error}");
                this.Check_next_ai(userMessage);
            }
        }
    }

    private void Check_next_ai(string userMessage)
    {
        if (this.app.setting.get_index_prioritize() == 0)
        {
            this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 1)
        {
            if (this.app.open_AI.is_active)
                this.app.open_AI.send_chat(userMessage);
            else
                this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 2)
        {
            this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 3)
        {
            if (this.app.open_AI.is_active)
                this.app.open_AI.send_chat(userMessage);
            else
                this.app.command.show_msg_no_chat();
        }
    }

    public void send_chat(string s_key)
    {
        if (this.key_api.Length != 0)
            StartCoroutine(PostRequest(s_key));
        else
            Check_next_ai("No key Gemini");
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
