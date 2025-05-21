using Meta.WitAi.TTS.Utilities;
using Oculus.Voice.Dictation;
using PassthroughCameraSamples;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class AIConnector : MonoBehaviour
{
    [SerializeField] private WebCamTextureManager webCamTextureManager;
    [SerializeField] private AppDictationExperience appDictationExperience;
    [SerializeField] private TTSSpeaker speaker;
    [SerializeField] private TextMeshPro screenshotCounterText;
    [Space]
    [SerializeField] private GeminiHandler geminiHandler;

    private bool isRecording = false;
    private Texture2D webcamTexture = null;
    private string dictationText = string.Empty;
    private bool isWaitingForText = false;
    private bool isWaitingForImage = false;
    private float recordingTimer = 0;

    private void Start()
    {
        appDictationExperience.DictationEvents.OnFullTranscription.AddListener(OnFullTranscription);
        appDictationExperience.Deactivate();
    }

    private void Update()
    {
        /*if (OVRInput.GetDown(OVRInput.Button.One))
        {
            StartRecording();
        }
        else if (OVRInput.GetUp(OVRInput.Button.One))
        {
            StopRecording();
        }*/
        if (isRecording)
        {
            recordingTimer += Time.deltaTime;
            screenshotCounterText.text = recordingTimer.ToString("0.0");
        }
    }

    public void StartRecording()
    {
        Logger.Instance.LogInfo("Start recording...");
        recordingTimer = 0;
        isRecording = true;
        appDictationExperience.Activate();
    }

    public void StopRecording()
    {
        Logger.Instance.LogInfo("Stop recording...");
        isRecording = false;
        isWaitingForText = true;
        isWaitingForImage = true;
        appDictationExperience.Deactivate();
        StartCoroutine(CaptureImage());
        StartCoroutine(SendDataToGemini());
    }

    private IEnumerator CaptureImage()
    {
        float time = 4f;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            screenshotCounterText.text = Mathf.CeilToInt(time).ToString("0");
            yield return null;
        }
        screenshotCounterText.text = "";
        yield return new WaitForEndOfFrame();

        var capturedTexture = new Texture2D(512, 512, TextureFormat.RGB24, false);
        var fillColor = Color.gray;
        var fillPixels = Enumerable.Repeat(fillColor, 512 * 512).ToArray();
        capturedTexture.SetPixels(fillPixels);
        capturedTexture.Apply();
        var webCamTex = webCamTextureManager.WebCamTexture;
        capturedTexture = new Texture2D(webCamTex.width, webCamTex.height, TextureFormat.RGBA32, false);
        capturedTexture.SetPixels(webCamTex.GetPixels());
        capturedTexture.Apply();
        webcamTexture = capturedTexture;
        isWaitingForImage = false;
    }

    private void OnFullTranscription(string text)
    {
        if (isWaitingForText)
        {
            Logger.Instance.LogInfo("Received dictation text: " + text);
            isWaitingForText = false;
            dictationText = text;
        }
    }

    private IEnumerator SendDataToGemini()
    {
        yield return new WaitUntil(() => !isWaitingForImage && !isWaitingForText);
        var imageBytes = webcamTexture.EncodeToJPG();
        geminiHandler.SendRequest(imageBytes, dictationText, (response) =>
        {
            if (response != null)
            {
                Logger.Instance.LogInfo("Gemini response: " + response);
                speaker.Speak(response);
            }
            else
            {
                Logger.Instance.LogError("Failed to get a response from Gemini.");
            }
        });
    }
}
