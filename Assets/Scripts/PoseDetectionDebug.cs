using Oculus.Interaction;
using UnityEngine;

public class PoseDetectionDebug : MonoBehaviour
{
    public SelectorUnityEventWrapper selectorUnityEventWrapper;
    [Space]
    public Renderer debugRenderer;

    private void Start()
    {
        debugRenderer.material.color = Color.red;
        selectorUnityEventWrapper.WhenSelected.AddListener(OnSelected);
        selectorUnityEventWrapper.WhenUnselected.AddListener(OnUnselected);
    }

    private void OnSelected()
    {
        debugRenderer.material.color = Color.green;
    }

    private void OnUnselected()
    {
        debugRenderer.material.color = Color.red;
    }
}
