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

    public void OnTap(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;

        if (Stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        var item = p.hand.Take();
        var no = item.GetComponent<NetworkObject>();
        Stored = no;

        PlaceOnSlot(item);
    }

    public void OnHoldStart(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;
        if (Stored == null) return;
        if (!p.hand.IsEmpty) return;

        InUse = true;
        //  전체 클라에 "bar 켜!" 보내기
        RPC_Progress(true, craftTime);
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;

        InUse = false;
        //  전체에 "bar 꺼" 보내기
        RPC_Progress(false, 0f);
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;
        if (Stored == null) { InUse = false; RPC_Progress(false, 0f); return; }
        if (!p.hand.IsEmpty) { InUse = false; RPC_Progress(false, 0f); return; }

        // 끝났으니까 bar 멈춤
        RPC_Progress(false, 0f);

        // 1) 재료 제거
        Runner.Despawn(Stored);
        Stored = null;

        // 2) 결과물 생성
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
}
