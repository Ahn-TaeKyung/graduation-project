using Fusion;
using UnityEngine;

public class WoodPile : NetworkBehaviour, IInteractable
{
    [SerializeField] private NetworkObject woodPrefab;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool ok = p.hand.IsEmpty;
        hint = ok ? "E - 나무 줍기" : "";
        return ok;
    }

    public void OnTap(PlayerInteractor p)
    {
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
        if (!p.hand.IsEmpty) return;

        var spawned = Runner.Spawn(
            woodPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity,
            p.NetObj.InputAuthority
        );

        p.hand.Pick(spawned.GetComponent<Item>());
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
