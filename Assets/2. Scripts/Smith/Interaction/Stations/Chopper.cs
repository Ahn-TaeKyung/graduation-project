using UnityEngine;

public class Chopper : MonoBehaviour, IInteractable
{
    [SerializeField] private float chopTime = 1.5f;
    [SerializeField] private ItemType inputType = ItemType.Log;
    [SerializeField] private Item outputPrefab;

    public InteractionKind Kind => InteractionKind.Hold;
    public float HoldDuration => chopTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool can = (p.hand.Held && p.hand.Held.type == inputType);
        hint = can ? "E 꾹 - 장작 패기" : "나무 필요";
        return can;
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        var log = p.hand.Take();
        Destroy(log.gameObject);
        p.hand.Pick(Instantiate(outputPrefab));
    }

    public void OnTap(PlayerInteractor p) { }
}
