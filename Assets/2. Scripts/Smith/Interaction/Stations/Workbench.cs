using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float craftTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Plank;
    [SerializeField] private Item outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    private Item stored;

    public InteractionKind Kind => (stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => craftTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 재료 올려두기" : "재료 필요";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 제작" : "손이 비어야 함";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        if (stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        stored = p.hand.Take();
        PlaceOnSlot(stored);
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        if (stored == null || !p.hand.IsEmpty) return;

        Destroy(stored.gameObject);
        stored = null;

        var outItem = Instantiate(outputPrefab);
        p.hand.Pick(outItem);
    }
    public void OnHoldStart(PlayerInteractor p)
    {
        if (stored != null && p.hand.IsEmpty && progressBar)
            progressBar.StartProgress(craftTime);
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (progressBar) progressBar.StopProgress();
    }

    // ===== 유틸 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;
        item.transform.localRotation = Quaternion.Euler(slotLocalEuler);
        item.transform.localScale    = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }
}
