using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material[] colorMats;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // 혼자일 때는 0, 둘 이상일 때는
            // 호스트=0, 나머지=1 로 가게끔
            int idx = 0;

            
            if (!Object.HasInputAuthority)
                idx = 1;

            // 혹은 세 번째부터는 모듈러로
            idx = idx % Mathf.Max(1, colorMats.Length);

            RPC_SetColor(idx);
        }

        ApplyColor();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetColor(int idx)
    {
        ColorIndex = idx;
        ApplyColor();
    }

    void ApplyColor()
    {
        if (!targetRenderer || colorMats == null || colorMats.Length == 0)
            return;

        int i = Mathf.Clamp(ColorIndex, 0, colorMats.Length - 1);
        targetRenderer.material = colorMats[i];
    }
}
