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
        if (Stored == null)
        {
            if (p.hand.Held == null) return;
            if (allowedType != ItemType.None && p.hand.Held.type != allowedType) return;

            var item = p.hand.Take();
            var no = item.GetComponent<NetworkObject>();
            Stored = no;

            PlaceOnSlot(item);
        }
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
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;

        // 🔥 타입별 회전 적용
        Vector3 eulerToUse = slotLocalEuler;
        if (item.type == ItemType.Sword)
            eulerToUse = swordLocalEuler;
        else if (item.type == ItemType.Bow)
            eulerToUse = bowLocalEuler;

        item.transform.localRotation = Quaternion.Euler(eulerToUse);
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
