// 파일명: SharedWeaponInventory.cs (CS0103, CS0165 모두 해결)
using Fusion;
using System.Collections.Generic; // KeyValuePair
using UnityEngine;

// 이 스크립트는 NetworkObject와 함께 씬의 'Managers' 같은 곳에 있어야 합니다.
public class SharedWeaponInventory : NetworkBehaviour
{
    public static SharedWeaponInventory Instance { get; private set; }

    // [Networked] 속성만 있으면 Host -> Client로 딕셔너리 데이터가 동기화됩니다.
    [Networked]
    public NetworkDictionary<NetworkString<_16>, int> WeaponCounts { get; }

    // 참고: 딕셔너리 변경 감지를 하려면 "버전 플래그" 방식이 필요하지만,
    // 현재는 UI가 매 프레임 Polling하는 방식으로 구현합니다.
    // (이전 대화에서 OnChangedRender를 사용하지 않기를 원하셨으므로)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // [RPC] DeliveryStation에서 호출 (Host에서만 실행됨)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddWeapon(string turretID)
    {
        if (!Object.HasStateAuthority) return;

        NetworkString<_16> key = (NetworkString<_16>)turretID;
        
        WeaponCounts.TryGet(key, out int currentCount);
        WeaponCounts.Set(key, currentCount + 1);

        Debug.Log($"[Inventory] Host: {turretID} 추가. 총 재고: {currentCount + 1}");
    }

    // [RPC] TurretPlacer에서 호출 (Host에서만 실행됨)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseWeapon(string turretID)
    {
        if (!Object.HasStateAuthority) return;

        NetworkString<_16> key = (NetworkString<_16>)turretID;

        if (WeaponCounts.TryGet(key, out int currentCount) && currentCount > 0)
        {
            WeaponCounts.Set(key, currentCount - 1);
            Debug.Log($"[Inventory] Host: {turretID} 사용. 남은 재고: {currentCount - 1}");
        }
    }

    // [Helper] UI나 Placer에서 현재 재고를 즉시 읽기 위한 함수
    public int GetWeaponCount(string turretID)
    {
        NetworkString<_16> key = (NetworkString<_16>)turretID;
        
        // [수정 2] CS0165 해결: 'count'를 미리 선언하고 0으로 초기화
        int count = 0; 
        
        // [수정 1] CS0103 해결: 'IsSpawned' 대신 'this.Object.IsSpawned' 사용
        // 'Object'는 NetworkBehaviour가 상속받은 NetworkObject 프로퍼티입니다.
        // 'this'를 사용하여 UnityEngine.Object와의 모호성을 제거합니다.
        if (this.Object != null && this.Object.IsValid && WeaponCounts.TryGet(key, out count))
        {
            // 키가 존재하면 TryGet이 count를 덮어쓰고 true를 반환
            return count;
        }
        
        // 스폰 전이거나 키가 없으면 0을 반환
        return 0;
    }
}