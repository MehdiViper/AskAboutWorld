using UnityEngine;

[CreateAssetMenu(fileName = "GeminiAPIHolder", menuName = "Scriptable Objects/GeminiAPIHolder")]
public class GeminiAPIHolder : ScriptableObject
{
    [SerializeField] private string geminiAPI;
    public string GeminiAPI => geminiAPI;
}
