// 파일명: TurretManager.cs
using Fusion;
using UnityEngine;

public class TurretManager : NetworkBehaviour
{
    public static TurretManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // [RpcSources.All] = 모든 클라이언트가 호출 가능
    // [RpcTargets.StateAuthority] = Host (서버) 에서만 실행됨
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestPlaceTurret(string turretID, Vector2Int gridPos, PlayerRef placer)
    {
        // Host가 아니면 즉시 리턴 (안전장치)
        if (!Object.HasStateAuthority) return;

        TurretDefinition def = TurretDatabase.Instance.GetTurretByID(turretID);
        if (def == null)
        {
            Debug.LogError($"[TurretManager] Host: {turretID} ID를 가진 터렛을 찾을 수 없음");
            return;
        }

        // Host에서 2차 검증 (유효성, 경로, 비용 등)
        if (GridManager.Instance.IsAreaFree(gridPos, def.Size))
        {
            // 1. 그리드 점유 (Host의 GridManager만 업데이트)
            GridManager.Instance.OccupyArea(gridPos, def.Size, true);

            // 2. 월드 좌표 계산
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            worldPos.y = 31;
            // 3. Host가 네트워크 오브젝트 스폰
            // (주의: 터렛은 Host가 소유합니다. Placer는 InputAuthority를 갖지 않습니다.)
            NetworkObject spawnedTurret = Runner.Spawn(def.NetworkPrefab, worldPos, Quaternion.identity);
            if (spawnedTurret != null)
            {
                spawnedTurret.AssignInputAuthority(placer);
                Debug.Log($"[TurretManager] SUCCESS: Turret {turretID} spawned with ID {spawnedTurret.Id}");
            }
            else
            {
                Debug.LogError($"[TurretManager] FAILED: Runner.Spawn({turretID}) failed! Prefab not registered?");
            }
            Debug.Log($"[TurretManager] Host: {placer}가 {turretID}를 {gridPos}에 스폰 성공");
        }
        else
        {
            Debug.LogWarning($"[TurretManager] Host: {placer}의 {gridPos} 스폰 요청 거부 (점유됨)");
            // TODO: 클라이언트에게 스폰 실패 피드백 (예: RPC 응답)
        }
    }
}