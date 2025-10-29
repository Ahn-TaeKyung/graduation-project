using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProximityHighlighter : MonoBehaviour
{
    [Header("감지자 설정")]
    public Collider detectorTrigger; // 참고용
    public LayerMask interactableLayer;

    [Header("디버그")]
    public bool debugLog = false;

    private readonly HashSet<InteractableObject> _candidates = new();
    private InteractableObject _current;

    // ✅ 추가: Outline 캐시 (현재 하이라이트 중인 OutlineToggle)
    private OutlineToggle _lastOutline;

    // PlayerInteractor가 읽을 현재 대상
    public InteractableObject Current => _current;

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
            // 현재 선택이 빠져나가면 하이라이트/아웃라인 해제
            SafeOffHighlight(_current);
            _current = null;
            SetOutline(null); // 아웃라인도 해제
        }

        Reevaluate();
    }

    private void Reevaluate()
    {
        InteractableObject best = null;
        float bestScore = float.MaxValue;
        Vector3 p = transform.position;

        // 가장 가까운(우선순위 포함) 후보 선택
        foreach (var c in _candidates)
        {
            if (!c) continue; // 파괴됐을 수 있음
            Vector3 d = c.transform.position - p; d.y = 0f;
            float score = d.sqrMagnitude + c.priority * 0.0001f;
            if (score < bestScore) { best = c; bestScore = score; }
        }

        if (best == _current) return; // 변화 없음

        // 이전 대상 해제
        if (_current) SafeOffHighlight(_current);

        // 새 대상 설정
        _current = best;

        // 새 대상 하이라이트 + 아웃라인
        if (_current) SafeOnHighlight(_current);

        // ✅ Outline 토글 갱신
        UpdateOutlineForCurrent();
    }

    // ===== 안전 호출 유틸 =====
    private void SafeOnHighlight(InteractableObject obj)
    {
        try { obj.OnHighlight(); } catch { /* no-op */ }
    }

    private void SafeOffHighlight(InteractableObject obj)
    {
        try { obj.OffHighlight(); } catch { /* no-op */ }
    }

    // ===== Outline 제어 =====
    private void UpdateOutlineForCurrent()
    {
        OutlineToggle now = null;

        if (_current)
        {
            // 현재 대상에서 OutlineToggle을 찾는다 (루트/부모 어디에 붙어 있어도 됨)
            now = _current.GetComponentInParent<OutlineToggle>();
        }

        // 바뀐 경우에만 토글
        if (_lastOutline != now)
        {
            SetOutline(now);
        }
    }

    private void SetOutline(OutlineToggle now)
    {
        // 이전 대상 끄기
        if (_lastOutline) _lastOutline.SetHighlighted(false);

        // 새 대상 켜기
        if (now) now.SetHighlighted(true);

        _lastOutline = now;
    }
}
