using UnityEngine;

public class Anvil : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float forgeTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Ingot;
    [SerializeField] private Item outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar; //  진행바 연결

    private Item stored;
    private bool isForging;

    public InteractionKind Kind => (stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => forgeTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 쇳물 올려두기" : "쇳물이 필요함";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 검 단조" : "손이 비어야 함";
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

    //  Hold 시작,취소,완료
    public void OnHoldStart(PlayerInteractor p)
    {
        if (stored != null && p.hand.IsEmpty && progressBar)
        {
            progressBar.StartProgress(forgeTime);
            isForging = true;
        }
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (isForging && progressBar)
        {
            progressBar.StopProgress();
            isForging = false;
        }
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        if (stored == null || !p.hand.IsEmpty) return;

        if (progressBar) progressBar.StopProgress();
        isForging = false;

        Destroy(stored.gameObject);
        stored = null;

        var outItem = Instantiate(outputPrefab);
        p.hand.Pick(outItem);
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
