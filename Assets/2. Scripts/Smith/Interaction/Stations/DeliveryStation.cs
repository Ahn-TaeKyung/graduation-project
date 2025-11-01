// 파일명: DeliveryStation.cs (수정됨)
using Fusion;
using UnityEngine;

public class DeliveryStation : NetworkBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "OpenTrigger";

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (!p.hand.IsEmpty && p.hand.Held != null)
        {
            var t = p.hand.Held.type;
            if (t == ItemType.Sword || t == ItemType.Bow)
            {
                hint = "E - 무기 납품";
                return true;
            }
        }
        hint = "";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        // 클라이언트면 RPC로 서버에 요청
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestTap(p.NetObj.InputAuthority);
            return;
        }
        HandleTap(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestTap(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleTap(p);
    }

    // Host에서 실행
    void HandleTap(PlayerInteractor p)
    {
        if (p.hand.IsEmpty || p.hand.Held == null) return;
        var item = p.hand.Held;
        
        // [수정] ItemType을 TurretID 문자열로 변환
        string turretID = GetTurretIDFromItemType(item.type);
        if (turretID == null)
        {
            Debug.LogWarning($"제출할 수 없는 아이템 타입: {item.type}");
            return;
        }

        // 손에서 빼고 제거
        var taken = p.hand.Take();
        var no = taken.GetComponent<NetworkObject>();
        if (no != null)
            Runner.Despawn(no);
        else
            Destroy(taken.gameObject);

        // [핵심 수정] JSON 저장 대신 네트워크 인벤토리에 RPC 호출
        if (SharedWeaponInventory.Instance != null)
        {
            SharedWeaponInventory.Instance.RPC_AddWeapon(turretID);
        }

        // 애니메이션 실행
        RPC_PlayOpen();
    }
    
    // [신규 헬퍼] ItemType을 TurretDefinition의 ID와 일치시킵니다.
    // (이 부분은 당신의 TurretDefinition ID 설정에 맞게 수정해야 합니다)
    private string GetTurretIDFromItemType(ItemType type)
    {
        if (type == ItemType.Sword)
            return "SwordTurret"; // TurretDef_Sword의 ID
        if (type == ItemType.Bow)
            return "BowTurret"; // TurretDef_Bow의 ID
        
        return null; // 그 외 아이템은 제출 불가
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayOpen()
    {
        if (!animator) return;
        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(openTrigger);
    }

    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
    public void OnHoldComplete(PlayerInteractor p) { }

    private PlayerInteractor FindPlayerByRef(PlayerRef who)
    {
        var all = UnityEngine.Object.FindObjectsByType<PlayerInteractor>(UnityEngine.FindObjectsSortMode.None);
        foreach (var pi in all)
        {
            if (pi.Object != null && pi.Object.InputAuthority == who)
                return pi;
        }
        return null;
    }
}