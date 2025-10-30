using Fusion;
using UnityEngine;

public class Workbench : NetworkBehaviour, IInteractable
{
    [Header("Setup")]
    [SerializeField] private Transform slot;
    [SerializeField] private float craftTime = 2f;
    [SerializeField] private ItemType inputType = ItemType.Plank;
    [SerializeField] private NetworkObject outputPrefab;   // Item → NetworkObject

    [Header("Placed Visual Tuning")]
    [SerializeField] private Vector3 slotLocalOffset;
    [SerializeField] private Vector3 slotLocalEuler;
    [SerializeField] private float placedScale = 1f;

    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;

    // 이 작업대 하나에 대해서만 공유되는 상태
    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private bool InUse { get; set; }

    public InteractionKind Kind => (Stored == null) ? InteractionKind.Tap : InteractionKind.Hold;
    public float HoldDuration => craftTime;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        // 이 작업대만 사용 중이면 못 씀
        if (InUse)
        {
            hint = "";   // "사용중" 안 띄운다 했으니까 빈칸
            return false;
        }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 재료 올려두기" : "";
            return ok;
        }
        else
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E 꾹 - 제작" : "";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;          // 이 작업대만 잠겨있을 수 있음

        if (Stored != null) return;
        if (p.hand.Held == null || p.hand.Held.type != inputType) return;

        // 손에서 빼서 작업대에 올리기
        var item = p.hand.Take();
        var no = item.GetComponent<NetworkObject>();
        Stored = no;

        PlaceOnSlot(item);
    }

    public void OnHoldStart(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;
        if (Stored == null) return;
        if (!p.hand.IsEmpty) return;

        InUse = true;   
        if (progressBar) progressBar.StartProgress(craftTime);
    }

    public void OnHoldCancel(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;

        InUse = false;
        if (progressBar) progressBar.StopProgress();
    }

    public void OnHoldComplete(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (!InUse) return;
        if (Stored == null) { InUse = false; return; }
        if (!p.hand.IsEmpty) { InUse = false; return; }

        if (progressBar) progressBar.StopProgress();

        // 1) 올려둔 재료 제거
        Runner.Despawn(Stored);
        Stored = null;

        // 2) 결과물 생성 (네트워크)
        var spawned = Runner.Spawn(
            outputPrefab,
            slot.position,
            slot.rotation,
            p.NetObj.InputAuthority
        );

        var item = spawned.GetComponent<Item>();
        p.hand.Pick(item);

       
        InUse = false;
    }

    // ===== 유틸 =====
    private void PlaceOnSlot(Item item)
    {
        item.transform.SetParent(slot);
        item.transform.localPosition = slotLocalOffset;
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
