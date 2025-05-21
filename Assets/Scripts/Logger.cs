using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private static Logger _instance;
    public static Logger Instance => _instance;

    [SerializeField] public TextMeshProUGUI logText;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void LogInfo(string message)
    {
        logText.text += "Info: " + message + "\n";
        Debug.Log("Logger Info: " + message);
    }

    public void LogError(string message)
    {
        logText.text += "Error: " + message + "\n";
        Debug.LogError("Logger Error: " + message);
    }
}
