using Fusion;
using UnityEngine;

public class TrashBin : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform dropPoint;

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
        if (!Object.HasStateAuthority) return; // 서버/호스트만 디스폰
        var it = p.hand.Take();
        if (it == null) return;

        // 이펙트 / 사운드 재생 (옵션)
        if (dropPoint)
            it.transform.position = dropPoint.position;

        Runner.Despawn(it.GetComponent<NetworkObject>());
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
}
