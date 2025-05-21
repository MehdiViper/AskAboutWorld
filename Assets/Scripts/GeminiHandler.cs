using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiHandler : MonoBehaviour
{
    private string apiKey = "GEMINI_API_KEY";

    private void OnEnable()
    {
        var apiHolder = Resources.Load<GeminiAPIHolder>("API/GeminiAPIHolder");
        apiKey = apiHolder.GeminiAPI;
        Resources.UnloadAsset(apiHolder);
    }

    private void OnDisable()
    {
        apiKey = string.Empty;
    }

    public void SendRequest(byte[] image, string text, Action<string> callback)
    {
        StartCoroutine(SendRequestRoutine(image, text, callback));
    }

    private IEnumerator SendRequestRoutine(byte[] image, string text, Action<string> callback)
    {
        string base64Image = Convert.ToBase64String(image);

        string json = $@"
        {{
            ""contents"": [{{
                ""parts"": [
                    {{
                        ""inline_data"": {{
                            ""mime_type"": ""image/jpeg"",
                            ""data"": ""{base64Image}""
                        }}
                    }},
                    {{ ""text"": ""{text}"" }}
                ]
            }}],
            ""generationConfig"": {{
                ""temperature"": ""0.5"",
                ""maxOutputTokens"": ""250"",
                ""topP"": ""0.8"",
                ""topK"": ""10"",
            }}
        }}";

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return www.SendWebRequest();

        // Handle response
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Logger.Instance.LogError("Send Error: " + www.error);
            callback(null);
        }
        else
        {
            //Logger.Instance.LogInfo("Gemini response: " + www.downloadHandler.text);
            var response = JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text);
            if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
            {
                string caption = response.candidates[0].content.parts[0].text;
                callback(caption);
            }
            else
            {
                Logger.Instance.LogError("No valid response from Gemini API.");
                callback(null);
            }
        }
    }
}
