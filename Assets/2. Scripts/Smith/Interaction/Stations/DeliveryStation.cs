using Fusion;
using UnityEngine;

public class DeliveryStation : NetworkBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "OpenTrigger";

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (!p.hand.IsEmpty && p.hand.Held != null)
        {
            var t = p.hand.Held.type;
            if (t == ItemType.Sword || t == ItemType.Bow)
            {
                hint = "E - 무기 납품";
                return true;
            }
        }

        hint = "";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        // 클라이언트면 RPC로 서버에 요청
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestTap(p.NetObj.InputAuthority);
            return;
        }

        HandleTap(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestTap(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleTap(p);
    }

    void HandleTap(PlayerInteractor p)
    {
        if (p.hand.IsEmpty || p.hand.Held == null) return;

        var item = p.hand.Held;

        // 활/검만 허용
        if (item.type != ItemType.Sword && item.type != ItemType.Bow)
            return;

        // 손에서 빼고 제거
        var taken = p.hand.Take();
        var no = taken.GetComponent<NetworkObject>();
        if (no != null)
            Runner.Despawn(no);
        else
            Destroy(taken.gameObject);

        // 서버가 json에 저장
        SaveWeapon.Add(item.type.ToString());

        // 애니메이션 실행
        RPC_PlayOpen();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayOpen()
    {
        if (!animator) return;
        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(openTrigger);
    }

    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
    public void OnHoldComplete(PlayerInteractor p) { }

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
}
