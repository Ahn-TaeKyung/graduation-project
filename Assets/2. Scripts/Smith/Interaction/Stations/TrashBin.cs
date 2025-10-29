using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform dropPoint; // 선택: 버릴 위치(이펙트용). 없으면 생략 가능

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool ok = !p.hand.IsEmpty;
        hint = ok ? "E - 버리기" : "버릴 아이템이 없음";
        return ok;
    }

    public void OnTap(PlayerInteractor p)
    {
        var it = p.hand.Take();
        if (it == null) return;

        // 이펙트/사운드가 있다면 여기서 재생
        Destroy(it.gameObject);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
}
