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
        Debug.Log("Get chat GPT (" + userMessage + ")");

        IDictionary requestChat = (IDictionary)Json.Deserialize("{}");
        requestChat["model"] = "gpt-3.5-turbo";
        requestChat["temperature"] = 0.3f;

        IList list_message = new ArrayList();
        IDictionary systemMessage = (IDictionary)Json.Deserialize("{}");
        systemMessage["role"] = "system";
        systemMessage["content"] = this.app.tool.Get_ai_assistant_system_prompt();
        list_message.Add(systemMessage);

        IDictionary userChat = (IDictionary)Json.Deserialize("{}");
        userChat["role"] = "user";
        userChat["content"] = userMessage;
        list_message.Add(userChat);
        requestChat["messages"] = list_message;

        string requestData = Json.Serialize(requestChat);
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
                if (chat_ai == null || chat_ai["choices"] == null)
                {
                    this.app.command.show_msg_no_chat();
                    yield break;
                }

                IList choices = (IList)chat_ai["choices"];

                if (choices.Count > 0)
                {
                    IDictionary choice = (IDictionary)choices[0];
                    IDictionary message = (IDictionary)choice["message"];
                    string s_content = message["content"] != null ? message["content"].ToString() : "";
                    chat_ai = this.app.tool.Parse_ai_response_to_chat_data(s_content, userMessage, "Gpt");
                    this.app.command.act_chat(chat_ai);
                }
                else
                {
                    this.app.command.show_msg_no_chat();
                }

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
            if (this.app.gemini_AI.is_active)
                this.app.gemini_AI.send_chat(userMessage);
            else
                this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 1)
        {
            this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 2)
        {
            if (this.app.gemini_AI.is_active)
                this.app.gemini_AI.send_chat(userMessage);
            else
                this.app.command.show_msg_no_chat();
        }
        else if (this.app.setting.get_index_prioritize() == 3)
        {
            this.app.command.show_msg_no_chat();
        }
    }

    public void send_chat(string s_key)
    {
        if (this.key_api.Length != 0)
            StartCoroutine(PostRequest(s_key));
        else
            this.Check_next_ai("No key chat GPT");
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
