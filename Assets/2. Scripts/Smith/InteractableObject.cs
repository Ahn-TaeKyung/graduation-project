using UnityEngine;

public enum InteractableType { CounterTop, Crate, Return, Station, Sink }

[DisallowMultipleComponent]
public class InteractableObject : MonoBehaviour
{
    [Header("Type / Highlight")]
    public InteractableType type;
    [SerializeField] private Highlightable highlightable;

    [Header("Optional: 시야/선택 우선순위(낮을수록 우선)")]
    public int priority = 100;

    public bool IsHighlighted { get; private set; }

    void Reset()
    {
        if (!highlightable) highlightable = GetComponentInChildren<Highlightable>();
    }

    public void OnHighlight()
    {
        if (IsHighlighted) return;
        IsHighlighted = true;
        if (highlightable) highlightable.SetHighlighted(true);
    }

    public void OffHighlight()
    {
        if (!IsHighlighted) return;
        IsHighlighted = false;
        if (highlightable) highlightable.SetHighlighted(false);
    }
}
