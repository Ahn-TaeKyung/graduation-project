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

            r.GetPropertyBlock(mpb);

            if (on)
            {
                // 베이스 컬러(표준:_Color, URP:_BaseColor) 둘 다 시도
                mpb.SetColor("_Color", highlightColor);
                mpb.SetColor("_BaseColor", highlightColor);

                if (useEmission)
                {
                    // 표준/URP 공통으로 자주 쓰이는 이름
                    mpb.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
            }
            else
            {
                // 원복: 명시적으로 블랙/기본값
                mpb.SetColor("_EmissionColor", Color.black);
                // 베이스 컬러는 머티리얼 원래 값으로 돌리기 위해 Clear
                mpb.Clear();
            }

            r.SetPropertyBlock(mpb);
        }
    }
}