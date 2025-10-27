using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProximityHighlighter : MonoBehaviour
{
    [Header("감지자 설정")]
    public Collider detectorTrigger; // 참고용 필드 (필수는 아님)
    public LayerMask interactableLayer;

    [Header("디버그")]
    public bool debugLog = false;

    private readonly HashSet<InteractableObject> _candidates = new();
    private InteractableObject _current;

    // 릴레이가 호출하는 공개 메서드
    public void HandleTriggerEnter(Collider other)
    {
        if (debugLog) Debug.Log($"[Highlighter] Enter {other.name} (layer {other.gameObject.layer})", other);
        TryAdd(other);
    }

    public void HandleTriggerExit(Collider other)
    {
        if (debugLog) Debug.Log($"[Highlighter] Exit  {other.name}", other);
        TryRemove(other);
    }

    private void TryAdd(Collider col)
    {
        if (((1 << col.gameObject.layer) & interactableLayer) == 0) return;

        var obj = col.GetComponentInParent<InteractableObject>();
        if (obj == null) return;

        _candidates.Add(obj);
        Reevaluate();
    }

    private void TryRemove(Collider col)
    {
        var obj = col.GetComponentInParent<InteractableObject>();
        if (obj == null) return;

        _candidates.Remove(obj);
        if (_current == obj)
        {
            _current.OffHighlight();
            _current = null;
        }
        Reevaluate();
    }

    private void Reevaluate()
    {
        InteractableObject best = null;
        float bestScore = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var c in _candidates)
        {
            if (!c) continue;
            Vector3 d = c.transform.position - p; d.y = 0f;
            float score = d.sqrMagnitude + c.priority * 0.0001f;
            if (score < bestScore) { best = c; bestScore = score; }
        }

        if (best == _current) return;
        if (_current) _current.OffHighlight();
        _current = best;
        if (_current) _current.OnHighlight();
    }
}
