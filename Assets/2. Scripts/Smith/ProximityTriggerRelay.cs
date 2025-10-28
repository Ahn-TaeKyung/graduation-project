using UnityEngine;

public class ProximityTriggerRelay : MonoBehaviour
{
    [SerializeField] private PlayerProximityHighlighter highlighter;

    void Awake()
    {
        if (!highlighter) highlighter = GetComponentInParent<PlayerProximityHighlighter>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (highlighter) highlighter.HandleTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (highlighter) highlighter.HandleTriggerExit(other);
    }
}