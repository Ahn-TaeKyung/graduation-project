using Fusion;
using UnityEngine;

public class Chopper : NetworkBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float chopTime = 1.5f;
    [SerializeField] private ItemType inputType = ItemType.Log;
    [SerializeField] private NetworkObject outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;
    [SerializeField] private Vector3 logRotationEuler;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private bool InUse { get; set; }

    public InteractionKind Kind => (Stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => chopTime;

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
            hint = ok ? "E - 통나무 올려두기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 장작 패기" : "";
            return ok;
        }
    }

    // ===== TAP =====
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
    private void RPC_RequestTap(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleTap(p);
    }

    private void HandleTap(PlayerInteractor p)
    {
        if (InUse) return;
        if (Stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        var item = p.hand.Take();
        var no = item.GetComponent<NetworkObject>();
        Stored = no;

        PlaceOnSlot(item);
    }

    // ===== HOLD START =====
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
    private void RPC_RequestHoldStart(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldStart(p);
    }

    private void HandleHoldStart(PlayerInteractor p)
    {
        if (InUse) return;
        if (Stored == null) return;
        if (!p.hand.IsEmpty) return;

        InUse = true;
        RPC_Progress(true, chopTime);
    }

    // ===== HOLD CANCEL =====
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
    private void RPC_RequestHoldCancel(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldCancel(p);
    }

    private void HandleHoldCancel(PlayerInteractor p)
    {
        if (!InUse) return;
        InUse = false;
        RPC_Progress(false, 0f);
    }

    // ===== HOLD COMPLETE =====
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
    private void RPC_RequestHoldComplete(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleHoldComplete(p);
    }

    private void HandleHoldComplete(PlayerInteractor p)
    {
        if (!InUse) return;
        if (Stored == null || !p.hand.IsEmpty)
        {
            InUse = false;
            RPC_Progress(false, 0f);
            return;
        }

        // 바 끄기
        RPC_Progress(false, 0f);

        // 1) 올려둔 통나무 제거
        Runner.Despawn(Stored);
        Stored = null;

        // 2) 결과물 생성
        var spawned = Runner.Spawn(
            outputPrefab,
            slot.position,
            slot.rotation,
            p.NetObj.InputAuthority
        );

        // 3) 손에 들리기
        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);

        InUse = false;
    }

    // ===== 전시 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;

        if (item.type == ItemType.Log)
            item.transform.localRotation = Quaternion.Euler(logRotationEuler);
        else
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
        if (!item) return;
        if (item.transform.parent != slot)
            PlaceOnSlot(item);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Progress(bool on, float duration)
    {
        if (!progressBar) return;
        if (on) progressBar.StartProgress(duration);
        else progressBar.StopProgress();
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
}
