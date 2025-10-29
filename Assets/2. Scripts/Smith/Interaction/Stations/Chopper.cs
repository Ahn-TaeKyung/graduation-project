using UnityEngine;

public class Chopper : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;             // 통나무 올려둘 위치
    [SerializeField] private float chopTime = 1.5f;      // 홀드 시간
    [SerializeField] private ItemType inputType = ItemType.Log;
    [SerializeField] private Item outputPrefab;

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;    // 전시 미세 위치
    [SerializeField] private Vector3 slotLocalEuler;     // 전시 기본 각도
    [SerializeField] private float placedScale = 1f;     // 전시 스케일
    [SerializeField] private Vector3 logRotationEuler;   // Log 전용 회전값 추가

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar; // 진행바 (선택적으로 연결)

    private Item stored;                                 // 올려둔 통나무
    private bool isChopping;

    // 비어있으면 Tap, 올라와 있으면 Hold
    public InteractionKind Kind => (stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => chopTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 통나무 올려두기" : "나무 필요";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 장작 패기" : "손이 비어야 함";
            return ok;
        }
    }

    // Tap: 손의 통나무를 슬롯 위에 '전시 상태'로 올려둠
    public void OnTap(PlayerInteractor p)
    {
        if (stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        stored = p.hand.Take();
        PlaceOnSlot(stored);
    }

    // Hold 시작/취소/완료
    public void OnHoldStart(PlayerInteractor p)
    {
        if (stored != null && p.hand.IsEmpty && progressBar)
        {
            progressBar.StartProgress(chopTime);
            isChopping = true;
        }
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (isChopping && progressBar)
        {
            progressBar.StopProgress();
            isChopping = false;
        }
    }

    // Hold 완료: 전시 중인 통나무를 소비하고 결과물 지급
    public void OnHoldComplete(PlayerInteractor p)
    {
        if (stored == null || !p.hand.IsEmpty) return;

        if (progressBar) progressBar.StopProgress();
        isChopping = false;

        Destroy(stored.gameObject);
        stored = null;

        var outItem = Instantiate(outputPrefab);
        p.hand.Pick(outItem);
    }

    // ===== 전시 유틸 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;

        //  Log 타입이면 logRotationEuler 사용, 아니면 기본 각도 사용
        if (item.type == ItemType.Log)
            item.transform.localRotation = Quaternion.Euler(logRotationEuler);
        else
            item.transform.localRotation = Quaternion.Euler(slotLocalEuler);

        item.transform.localScale = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }
}
