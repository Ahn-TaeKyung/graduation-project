using Fusion;
using UnityEngine;

public class Crate : NetworkBehaviour, IInteractable
{
    [SerializeField] private NetworkObject itemPrefab;
    [SerializeField] private Animator animator;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        // 손이 비어 있어야 꺼낼 수 있음
        bool ok = p.hand.IsEmpty;
        hint = ok ? "E - 꺼내기" : "";
        return ok;
    }

    public void OnTap(PlayerInteractor p)
    {
        // 클라 → 서버
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestTap(p.NetObj.InputAuthority);
            return;
        }

        HandleTap(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestTap(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleTap(p);
    }

    private void HandleTap(PlayerInteractor p)
    {
        if (!p.hand.IsEmpty) return;

        // 아이템 스폰
        var spawned = Runner.Spawn(
            itemPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity,
            p.NetObj.InputAuthority
        );

        p.hand.Pick(spawned.GetComponent<Item>());

        // 애니는 전체로
        RPC_PlayOpen();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayOpen()
    {
        if (animator == null) return;
        animator.ResetTrigger("Open");
        animator.SetTrigger("Open");
    }

    private PlayerInteractor FindPlayerByRef(PlayerRef who)
    {
        var all = UnityEngine.Object.FindObjectsByType<PlayerInteractor>(UnityEngine.FindObjectsSortMode.None);
        foreach (var pi in all)
        {
            if (pi.Object != null && pi.Object.InputAuthority == who)
                return pi;
        }
        return null;
    }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
    public void OnHoldComplete(PlayerInteractor p) { }

}
