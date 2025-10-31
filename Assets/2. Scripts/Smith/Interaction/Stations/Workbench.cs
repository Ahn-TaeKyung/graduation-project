using Fusion;
using UnityEngine;

public class Workbench : NetworkBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float craftTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Plank;
    [SerializeField] private NetworkObject outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private bool InUse { get; set; }

    public InteractionKind Kind => (Stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => craftTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (InUse)
        {
            hint = "";
            return false;
        }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 재료 올려두기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 제작" : "";
            return ok;
        }
    }

    // ====== TAP ======
    public void OnTap(PlayerInteractor p)
    {
        // 권한 없는 클라는 서버한테 요청만
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
        if (InUse) return;
        if (Stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        var item = p.hand.Take();
        var no = item.GetComponent<NetworkObject>();
        Stored = no;

        PlaceOnSlot(item);
    }

    // ====== HOLD START ======
    public void OnHoldStart(PlayerInteractor p)
    {
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestHoldStart(p.NetObj.InputAuthority);
            return;
        }

        HandleHoldStart(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestHoldStart(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldStart(p);
    }

    void HandleHoldStart(PlayerInteractor p)
    {
        if (InUse) return;
        if (Stored == null) return;
        if (!p.hand.IsEmpty) return;

        InUse = true;
        RPC_Progress(true, craftTime);
    }

    // ====== HOLD CANCEL ======
    public void OnHoldCancel(PlayerInteractor p)
    {
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestHoldCancel(p.NetObj.InputAuthority);
            return;
        }

        HandleHoldCancel(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestHoldCancel(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldCancel(p);
    }

    void HandleHoldCancel(PlayerInteractor p)
    {
        if (!InUse) return;
        InUse = false;
        RPC_Progress(false, 0f);
    }

    // ====== HOLD COMPLETE ======
    public void OnHoldComplete(PlayerInteractor p)
    {
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestHoldComplete(p.NetObj.InputAuthority);
            return;
        }

        HandleHoldComplete(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestHoldComplete(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldComplete(p);
    }

    void HandleHoldComplete(PlayerInteractor p)
    {
        if (!InUse) return;
        if (Stored == null) { InUse = false; RPC_Progress(false, 0f); return; }
        if (!p.hand.IsEmpty) { InUse = false; RPC_Progress(false, 0f); return; }

        // 바 멈추기
        RPC_Progress(false, 0f);

        // 재료 소모
        Runner.Despawn(Stored);
        Stored = null;

        // 결과물 생성
        var spawned = Runner.Spawn(
            outputPrefab,
            slot.position,
            slot.rotation,
            p.NetObj.InputAuthority
        );
        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);

        InUse = false;
    }

    // ===== UTIL =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;
        item.transform.localRotation = Quaternion.Euler(slotLocalEuler);
        item.transform.localScale = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (Stored == null) return;

        var item = Stored.GetComponent<Item>();
        if (item == null) return;

        if (item.transform.parent != slot)
            PlaceOnSlot(item);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Progress(bool on, float duration)
    {
        if (!progressBar) return;
        if (on) progressBar.StartProgress(duration);
        else progressBar.StopProgress();
    }

    // 👇 임시 플레이어 찾기 (원하면 공용 헬퍼로 빼)
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
