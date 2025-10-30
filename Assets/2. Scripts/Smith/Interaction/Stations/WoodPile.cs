using Fusion;
using UnityEngine;

public class WoodPile : NetworkBehaviour, IInteractable
{
    [SerializeField] private NetworkObject logPrefab;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        hint = p.hand.IsEmpty ? "E - 나무 줍기" : "손이 비어야 함";
        return p.hand.IsEmpty;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return; // 서버/호스트만 스폰
        if (!p.hand.IsEmpty) return;

        var spawned = Runner.Spawn(
            logPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity,
            p.NetObj.InputAuthority
        );

        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
}
