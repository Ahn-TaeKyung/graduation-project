using UnityEngine;

public class Anvil : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;          // 재료 올려둘 자리
    [SerializeField] private float forgeTime = 2f;    // 홀드 시간
    [SerializeField] private ItemType inputType = ItemType.Ingot;
    [SerializeField] private Item outputPrefab;

    private Item stored; // 올려둔 재료

    // 상태에 따라 Tap/Hold 전환
    public InteractionKind Kind => (stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => forgeTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            // 1) 비어 있을 땐 손에 재료가 있어야 Tap 가능
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

    // Tap: 재료를 올려둔다
    public void OnTap(PlayerInteractor p)
    {
        if (stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        stored = p.hand.Take();
        stored.transform.SetParent(slot);
        stored.transform.localPosition = Vector3.zero;
        stored.transform.localRotation = Quaternion.identity;
        stored.gameObject.SetActive(true); // 모루 위에 보이게 두어도 되고, 숨기고 싶으면 false
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
}
