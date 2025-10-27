using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    [SerializeField] private float craftTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Plank;
    [SerializeField] private Item outputPrefab;

    public InteractionKind Kind => InteractionKind.Hold;
    public float HoldDuration => craftTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool can = (p.hand.Held && p.hand.Held.type == inputType);
        hint = can ? "E 꾹 - 활 제작" : "장작 필요";
        return can;
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        var plank = p.hand.Take();
        Destroy(plank.gameObject);
        p.hand.Pick(Instantiate(outputPrefab));
    }

    public void OnTap(PlayerInteractor p) { }
}
