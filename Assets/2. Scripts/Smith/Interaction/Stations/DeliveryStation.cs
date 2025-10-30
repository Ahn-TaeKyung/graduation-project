using Fusion;
using UnityEngine;

public class WeaponDeliveryStation : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform dropPoint;   // 선택: 이펙트 찍을 위치

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (p.hand.IsEmpty)
        {
            hint = "납품할 무기가 없음";
            return false;
        }

        var item = p.hand.Held;
        if (!IsDeliverable(item))
        {
            hint = "검/활만 납품 가능";
            return false;
        }

        hint = "E - 납품하기";
        return true;
    }

    public void OnTap(PlayerInteractor p)
    {
        // 쓰레기통이랑 똑같이: 서버/호스트만 실제 처리
        if (!Object.HasStateAuthority) return;

        var it = p.hand.Take();
        if (it == null) return;

        var item = it.GetComponent<Item>();
        if (item != null && IsDeliverable(item))
        {
            // 이름 있으면 넣고
            string playerName = p.name;
            SaveWeapon.Add(item.type.ToString(), playerName);
#if UNITY_EDITOR
            Debug.Log($"[WeaponDeliveryStation] {item.type} 납품됨 by {playerName}");
#endif
        }

        // 이펙트용 위치
        if (dropPoint)
            it.transform.position = dropPoint.position;

        // 네트워크 아이템이면 Despawn, 아니면 Destroy
        var no = it.GetComponent<NetworkObject>();
        if (no != null)
            Runner.Despawn(no);
        else
            Destroy(it.gameObject);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }

    // === Helper ===
    private bool IsDeliverable(Item item)
    {
        if (item == null) return false;
        return item.type == ItemType.Sword || item.type == ItemType.Bow;
    }
}
