using Fusion;
using UnityEngine;

public class Chopper : NetworkBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;             // 통나무 올려둘 위치
    [SerializeField] private float chopTime = 1.5f;      // 홀드 시간
    [SerializeField] private ItemType inputType = ItemType.Log;
    [SerializeField] private NetworkObject outputPrefab; //  네트워크 프리팹으로 변경

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;
    [SerializeField] private Vector3 logRotationEuler;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    //  이 도끼 하나에만 적용되는 상태
    [Networked] private NetworkObject Stored { get; set; }   // 올려둔 통나무
    [Networked] private bool InUse { get; set; }             // 이 Chopper만 잠금

    // 비어있으면 Tap, 올라와 있으면 Hold
    public InteractionKind Kind => (Stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => chopTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        
        if (InUse)
        {
            hint = "";        
            return false;
        }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 통나무 올려두기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 장작 패기" : "";
            return ok;
        }
    }

    // Tap: 손의 통나무를 슬롯 위에 '전시 상태'로 올려둠
    public void OnTap(PlayerInteractor p)
    {
        // 서버/호스트만 상태 바꿔
        if (!Object.HasStateAuthority) return;
        if (InUse) return;

        if (Stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        // 손에서 빼오기
        var item = p.hand.Take();
        var no = item.GetComponent<NetworkObject>();
        Stored = no;

        // 모든 클라에서 보이게 전시
        PlaceOnSlot(item);
    }

    // Hold 시작
    public void OnHoldStart(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;

        if (Stored != null && p.hand.IsEmpty)
        {
            InUse = true; // 이 Chopper만 잠금
            if (progressBar) progressBar.StartProgress(chopTime);
        }
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;

        InUse = false;
        if (progressBar) progressBar.StopProgress();
    }

    // Hold 완료: 전시 중인 통나무를 소비하고 결과물 지급
    public void OnHoldComplete(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;
        if (Stored == null || !p.hand.IsEmpty)
        {
            InUse = false;
            return;
        }

        if (progressBar) progressBar.StopProgress();

        // 1) 올려둔 통나무 제거 (네트워크에서)
        Runner.Despawn(Stored);
        Stored = null;

        // 2) 결과물 생성 (네트워크로)
        var spawned = Runner.Spawn(
            outputPrefab,
            slot.position,
            slot.rotation,
            p.NetObj.InputAuthority   // 이 플레이어가 권한자
        );

        // 3) 손에 들려주기
        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);

        // 4)잠금 해제
        InUse = false;
    }

    // ===== 전시 유틸 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;

        if (item.type == ItemType.Log)
            item.transform.localRotation = Quaternion.Euler(logRotationEuler);
        else
            item.transform.localRotation = Quaternion.Euler(slotLocalEuler);

        item.transform.localScale = Vector3.one * Mathf.Max(0.0001f, placedScale);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
        item.gameObject.SetActive(true);
    }
    private void LateUpdate()
    {
        if (Stored == null) return;

        var item = Stored.GetComponent<Item>();
        if (item == null) return;

        if (item.transform.parent != slot)
            PlaceOnSlot(item);
    }
}
