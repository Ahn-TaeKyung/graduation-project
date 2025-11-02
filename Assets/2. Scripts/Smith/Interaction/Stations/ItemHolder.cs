using Fusion;
using UnityEngine;

public class ItemHolder : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform slot;
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("Optional per-item rotation")]
    [SerializeField] private Vector3 swordLocalEuler = new Vector3(0, 0, 90);
    [SerializeField] private Vector3 bowLocalEuler   = new Vector3(0, 45, 45);

    [SerializeField] private ItemType allowedType = ItemType.None;

    [Networked] private NetworkObject Stored { get; set; }

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (Stored == null)
        {
            bool ok = p.hand.Held && (allowedType == ItemType.None || p.hand.Held.type == allowedType);
            hint = ok ? "E - 올려두기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 가져가기" : "";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        // 상태 권한 없는 쪽은 RPC로 요청
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
        // 비어있을 때 → 올려두기
        if (Stored == null)
        {
            if (p.hand.Held == null) return;
            if (allowedType != ItemType.None && p.hand.Held.type != allowedType) return;

            var item = p.hand.Take();
            var no = item.GetComponent<NetworkObject>();
            Stored = no;

            PlaceOnSlot(item);
        }
        // 차 있을 때 → 가져가기
        else
        {
            if (!p.hand.IsEmpty) return;

            var item = Stored.GetComponent<Item>();
            Stored = null;

            p.hand.Pick(item);
        }
    }

    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot, worldPositionStays: false);
        ApplySlotTransform(item);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;

        //  여기서 NetworkTransform 건드리던 코드 제거 (버전마다 달라서)
        // if (item.TryGetComponent(out NetworkTransform nt)) { ... }

        item.gameObject.SetActive(true);
    }

    // 위치/회전/스케일을 한군데서만 계산
    private void ApplySlotTransform(Item item)
    {
        item.transform.localPosition = slotLocalOffset;

        Vector3 eulerToUse = slotLocalEuler;
        if (item.type == ItemType.Sword)
            eulerToUse = swordLocalEuler;
        else if (item.type == ItemType.Bow)
            eulerToUse = bowLocalEuler;

        item.transform.localRotation = Quaternion.Euler(eulerToUse);
        item.transform.localScale = Vector3.one * Mathf.Max(0.0001f, placedScale);
    }

    private void LateUpdate()
    {
        if (Stored == null) return;

        var item = Stored.GetComponent<Item>();
        if (!item) return;

        // 매 프레임 강제 고정
        if (item.transform.parent != slot)
            item.transform.SetParent(slot, worldPositionStays: false);

        ApplySlotTransform(item);
    }

    private PlayerInteractor FindPlayerByRef(PlayerRef who)
    {
        //  풀네임으로 호출
        var all = UnityEngine.Object.FindObjectsByType<PlayerInteractor>(FindObjectsSortMode.None);
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
