using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Highlightable : MonoBehaviour
{
    [SerializeField] private List<Renderer> targetRenderers = new();
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField, Range(0f, 5f)] private float highlightIntensity = 1.5f;
    [SerializeField] private bool useEmission = true;

    private readonly Dictionary<Renderer, MaterialPropertyBlock> _mpbs = new();

    void Reset()
    {
        targetRenderers.Clear();
        foreach (var r in GetComponentsInChildren<Renderer>()) targetRenderers.Add(r);
    }

    public void SetHighlighted(bool on)
    {
        foreach (var r in targetRenderers)
        {
            if (!r) continue;

            if (!_mpbs.TryGetValue(r, out var mpb))
            {
                mpb = new MaterialPropertyBlock();
                _mpbs[r] = mpb;
            }

            if (on)
            {
                r.GetPropertyBlock(mpb);

                // ▶ 색상 틴트: Standard/URP + Arnold 둘 다 시도
                mpb.SetColor("_Color",      highlightColor);
                mpb.SetColor("_BaseColor",  highlightColor);
                mpb.SetColor("BaseColor",   highlightColor);   // ← Arnold

                if (useEmission)
                {
                    // ▶ Emission: Standard/URP + Arnold 모두 시도
                    var emis = highlightColor * highlightIntensity;
                    mpb.SetColor("_EmissionColor", emis);
                    mpb.SetColor("EmissionColor",  emis);       // ← Arnold
                }

                r.SetPropertyBlock(mpb);
            }
            else
            {
                // ▶ 오버라이드 완전 제거(가장 안전)
                r.SetPropertyBlock(null);
            }
        }
    }
}
