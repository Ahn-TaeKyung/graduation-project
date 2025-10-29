using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class ItemHolder : MonoBehaviour, IInteractable
{
    [Header("Slot Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private Vector3 slotOffset;
    [SerializeField] private Vector3 slotEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("Allowed Items")]
    [Tooltip("체크 시 어떤 아이템이든 올릴 수 있음 (allowedTypes 무시)")]
    [SerializeField] private bool allowAll = false;
    [Tooltip("allowAll이 꺼져 있을 때 허용할 타입들")]
    [SerializeField] private ItemType[] allowedTypes = new ItemType[0];
    [Tooltip("비허용 아이템일 때 보여줄 커스텀 문구 (비워두면 자동 생성됨)")]
    [SerializeField] private string customAllowedHint;

    [Header("Rotation per Type")]
    [Tooltip("Sword가 올라올 때 회전값")]
    [SerializeField] private Vector3 swordRotation = new Vector3(0, 90, 0);
    [Tooltip("Bow가 올라올 때 회전값")]
    [SerializeField] private Vector3 bowRotation = new Vector3(0, 45, 0);

    private Item stored;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            if (p.hand.Held == null)
            {
                hint = "들고 있는 아이템이 없음";
                return false;
            }

            if (!IsAllowed(p.hand.Held))
            {
                hint = string.IsNullOrEmpty(customAllowedHint)
                    ? $"허용 아이템만 올릴 수 있음: {BuildAllowedListText()}"
                    : customAllowedHint;
                return false;
            }

            hint = "E - 아이템 올려두기";
            return true;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 아이템 집기" : "손이 비어 있어야 함";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        if (stored == null)
        {
            if (p.hand.Held == null) return;
            if (!IsAllowed(p.hand.Held)) return;

            stored = p.hand.Take();
            PlaceOnSlot(stored);
        }
        else
        {
            if (!p.hand.IsEmpty) return;
            p.hand.Pick(stored);
            stored = null;
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
            case ItemType.Sword:
                return swordRotation;
            case ItemType.Bow:
                return bowRotation;
            default:
                return slotEuler;
        }
    }
}
