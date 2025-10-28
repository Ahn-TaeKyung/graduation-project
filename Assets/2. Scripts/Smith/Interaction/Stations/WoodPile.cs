using UnityEngine;

public class WoodPile : MonoBehaviour, IInteractable
{
    [SerializeField] private Item logPrefab;
    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        hint = p.hand.IsEmpty ? "E - 나무 줍기" : "손이 비어야 함";
        return p.hand.IsEmpty;
    }

    public void OnTap(PlayerInteractor p)
    {
        var item = Instantiate(logPrefab);
        p.hand.Pick(item);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
}
