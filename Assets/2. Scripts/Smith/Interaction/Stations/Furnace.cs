using UnityEngine;

public class Furnace : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform slot;
    [SerializeField] private float smeltTime = 3f;
    [SerializeField] private ItemType inputType = ItemType.Ore;
    [SerializeField] private Item outputPrefab;

    private Item stored;
    private float timer;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            hint = (p.hand.Held && p.hand.Held.type == inputType) ? "E - 화로에 넣기" : "광석 필요";
            return (p.hand.Held && p.hand.Held.type == inputType);
        }
        else if (timer >= smeltTime)
        {
            hint = p.hand.IsEmpty ? "E - 주조물 꺼내기" : "손이 비어야 함";
            return p.hand.IsEmpty;
        }
        hint = "용해 중...";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (stored == null)
        {
            stored = p.hand.Take();
            stored.transform.SetParent(slot);
            stored.transform.localPosition = Vector3.zero;
            stored.gameObject.SetActive(false);
            timer = 0;
        }
        else if (timer >= smeltTime && p.hand.IsEmpty)
        {
            var outItem = Instantiate(outputPrefab);
            p.hand.Pick(outItem);
            Destroy(stored.gameObject);
            stored = null;
            timer = 0;
        }
    }

    private void Update()
    {
        if (stored != null && timer < smeltTime) timer += Time.deltaTime;
    }

    public void OnHoldComplete(PlayerInteractor p) { }
}
