using Fusion;
using UnityEngine;

public class Anvil : NetworkBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float forgeTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Ingot;
    [SerializeField] private NetworkObject outputPrefab;

    [Header("Slot Visual")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    // 이 모루 하나에만 적용되는 플래그
    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private bool InUse { get; set; }

    public InteractionKind Kind => (Stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => forgeTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        // 이 모루만 잠긴 상태면 못 씀
        if (InUse)
        {
            hint = "";       // 굳이 "사용 중" 안 띄움
            return false;
        }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 재료 올리기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? $"E 꾹 - {inputType} 단조" : "";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;             // 이 모루만 잠금

        // 빈 모루 → 손에 든 재료 올리기
        if (Stored == null)
        {
            if (p.hand.Held == null) return;
            if (p.hand.Held.type != inputType) return;

            var item = p.hand.Take();
            var no = item.GetComponent<NetworkObject>();
            Stored = no;
            PlaceOnSlot(item);
        }
    }

    public void OnHoldStart(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;
        if (Stored == null) return;
        if (!p.hand.IsEmpty) return;

        InUse = true; // 이 모루만 잠금
        if (progressBar) progressBar.StartProgress(forgeTime);
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;

        InUse = false;
        if (progressBar) progressBar.StopProgress();
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;
        if (Stored == null) { InUse = false; return; }
        if (!p.hand.IsEmpty) { InUse = false; return; }

        if (progressBar) progressBar.StopProgress();

        // 1) 재료 제거
        Runner.Despawn(Stored);
        Stored = null;

        // 2) 결과물 스폰
        var spawned = Runner.Spawn(
            outputPrefab,
            slot.position,
            slot.rotation,
            p.NetObj.InputAuthority
        );
        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);

        // 3) 잠금 해제 
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
    }
}
