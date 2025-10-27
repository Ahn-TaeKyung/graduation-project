using UnityEngine;

public class Anvil : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;          // 재료 올려둘 Transform
    [SerializeField] private float forgeTime = 2f;    // 홀드 시간
    [SerializeField] private ItemType inputType = ItemType.Ingot;
    [SerializeField] private Item outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset; // 전시 미세 위치
    [SerializeField] private Vector3 slotLocalEuler;  // 전시 각도
    [SerializeField] private float placedScale = 1f;  // 전시 스케일

    private Item stored; // 올려둔 재료

    // 상태에 따라 Tap/Hold 전환
    public InteractionKind Kind => (stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => forgeTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            // 1) 비어 있을 때는 손에 재료가 있어야 Tap 가능
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 쇳물 올려두기" : "쇳물이 필요함";
            return ok;
        }
        else
        {
            // 2) 재료가 올라와 있으면 손은 비어야 Hold 가능
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 검 단조" : "손이 비어야 함";
            return ok;
        }
    }

    // Tap: 재료를 올려둔다(전시 상태)
    public void OnTap(PlayerInteractor p)
    {
        if (stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        stored = p.hand.Take();
        PlaceOnSlot(stored);
    }

    // Hold 완료: 결과물 제작
    public void OnHoldComplete(PlayerInteractor p)
    {
        if (stored == null || !p.hand.IsEmpty) return;

        Destroy(stored.gameObject);
        stored = null;

        var outItem = Instantiate(outputPrefab);
        p.hand.Pick(outItem);
    }

    // ===== 유틸 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;
        item.transform.localRotation = Quaternion.Euler(slotLocalEuler);
        item.transform.localScale    = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }
}
