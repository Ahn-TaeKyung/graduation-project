using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material[] colorMats;

    public override void Spawned()
    {
        // 1) 내 거면 내가 색을 고른다
        if (Object.HasInputAuthority)
        {
            // 들어온 순서대로 색 주기
            int idx = Runner.LocalPlayer.PlayerId;   // ← 이게 제일 직관적
            RPC_SetColor(idx);
        }

        // 2) 지금 알고 있는 값으로 일단 그린다
        ApplyColor();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_SetColor(int idx)
    {
        ColorIndex = idx;
        ApplyColor();
    }

    void ApplyColor()
    {
        if (!targetRenderer || colorMats == null || colorMats.Length == 0) return;

        int i = Mathf.Clamp(ColorIndex, 0, colorMats.Length - 1);
        targetRenderer.material = colorMats[i];
    }
}
