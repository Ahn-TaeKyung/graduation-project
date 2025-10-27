using UnityEngine;

public class Anvil : MonoBehaviour, IInteractable
{
    [SerializeField] private float forgeTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Ingot;
    [SerializeField] private Item outputPrefab;

    public InteractionKind Kind => InteractionKind.Hold;
    public float HoldDuration => forgeTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool can = (p.hand.Held && p.hand.Held.type == inputType);
        hint = can ? "E 꾹 - 단조" : "쇳물이 필요함";
        return can;
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        var inItem = p.hand.Take();
        Destroy(inItem.gameObject);
        p.hand.Pick(Instantiate(outputPrefab));
    }

    public void OnTap(PlayerInteractor p) { }
}
