using System.Linq;
using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemHolder : NetworkBehaviour, IInteractable
{
    [Header("Slot Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private Vector3 slotOffset;
    [SerializeField] private Vector3 slotEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("Allowed Items")]
    [SerializeField] private bool allowAll = false;
    [SerializeField] private ItemType[] allowedTypes = new ItemType[0];
    [SerializeField] private string customAllowedHint;

    [Header("Rotation per Type")]
    [SerializeField] private Vector3 swordRotation = new Vector3(0, 90, 0);
    [SerializeField] private Vector3 bowRotation = new Vector3(0, 45, 0);

    // 🔐 이 홀더 위에 올려진 네트워크 아이템
    [Networked]
    private NetworkObject Stored { get; set; }

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        // 아무것도 안 올라가 있음 → 올리기 모드
        if (Stored == null)
        {
            var held = p.hand.Held;
            if (held == null)
            {
                hint = "들고 있는 아이템이 없음";
                return false;
            }

            if (!IsAllowed(held))
            {
                hint = string.IsNullOrEmpty(customAllowedHint)
                    ? $"허용 아이템만 올릴 수 있음: {BuildAllowedListText()}"
                    : customAllowedHint;
                return false;
            }

            hint = "E - 아이템 올려두기";
            return true;
        }
        // 뭐가 올라가 있음 → 집기 모드
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 아이템 집기" : "손이 비어 있어야 함";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        // 상태 바꾸는 건 권한 있는 쪽만
        if (!Object.HasStateAuthority) return;

        // 1) 비어있을 때 → 플레이어 손에서 가져와 올리기
        if (Stored == null)
        {
            var held = p.hand.Held;
            if (held == null) return;
            if (!IsAllowed(held)) return;

            var no = held.GetComponent<NetworkObject>();
            if (no == null) return;

            // 플레이어 손 비우기
            p.hand.Take();

            // 네트워크에 "이 홀더 위에는 이거 올라감" 이라고 기록
            Stored = no;

            // 모든 클라에서 똑같이 보이게
            PlaceOnSlot(held);
        }
        // 2) 뭔가 올라가 있을 때 → 플레이어 손에 다시 주기
        else
        {
            if (!p.hand.IsEmpty) return;

            var item = Stored.GetComponent<Item>();
            if (item == null) return;

            // 네트워크에서 홀더 비우기
            Stored = null;

            // 플레이어 손에 들려주기
            p.hand.Pick(item);
        }
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }

    // ===== Helper Methods =====

    private bool IsAllowed(Item item)
    {
        if (allowAll) return true;
        if (item == null) return false;
        if (allowedTypes == null || allowedTypes.Length == 0) return false;
        return allowedTypes.Contains(item.type);
    }

    private string BuildAllowedListText()
    {
        if (allowAll) return "모든 아이템";
        if (allowedTypes == null || allowedTypes.Length == 0) return "지정 없음";
        return string.Join(", ", allowedTypes.Select(x => x.ToString()));
    }

    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotOffset;
        item.transform.localRotation = Quaternion.Euler(GetRotationForItem(item));
        item.transform.localScale = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }

    private Vector3 GetRotationForItem(Item item)
    {
        if (item == null) return slotEuler;

        switch (item.type)
        {
            case ItemType.Sword: return swordRotation;
            case ItemType.Bow: return bowRotation;
            default: return slotEuler;
        }
    }


    private void LateUpdate()
    {
        if (Stored == null) return;

        var item = Stored.GetComponent<Item>();
        if (item == null) return;

        
        PlaceOnSlot(item);
    }
}
