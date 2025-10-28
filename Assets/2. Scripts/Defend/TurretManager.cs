using Fusion;
using UnityEngine;

/// <summary>
/// TurretManager: Host(StateAuthority)에서 설치요청을 검증하고 실제 NetworkObject 스폰
/// - GameStateManager가 RPC 수신을 통해 RequestPlaceTurret 호출하면 여기서 Spawn 수행
/// </summary>
public class TurretManager : NetworkBehaviour
{
    public static TurretManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Host에서 호출되어야 함
    public void SpawnTurretOnHost(NetworkObject prefab, Vector2Int cell, Vector2Int size, PlayerRef owner)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("[TurretManager] SpawnTurretOnHost should be called on StateAuthority (Host)");
            return;
        }

        if (!GridManager.Instance.IsAreaFree(cell, size.x, size.y))
        {
            Debug.Log("[TurretManager] Host: 설치 불가 (검증 실패)");
            return;
        }

        // Grid 점유 처리
        GridManager.Instance.SetAreaOccupied(cell, size.x, size.y, true);

        // 실제 world pos
        Vector3 worldPos = GridManager.Instance.CellToWorldCenter(cell);

        // Network spawn
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("[TurretManager] NetworkRunner not found.");
            return;
        }

        // prefab 인스펙터에 NetworkObject를 가진 프리팹을 넣어야 함
        if (prefab == null)
        {
            Debug.LogError("[TurretManager] prefab is null!");
            return;
        }

        // Spawn - Runner.Spawn 사용 (환경에 맞춰 시그니처 조정 가능)
        var spawned = runner.Spawn(prefab, worldPos, Quaternion.identity, playerRef: owner);
        if (spawned == null)
        {
            Debug.LogError("[TurretManager] Spawn failed!");
        }
        else
        {
            Debug.Log("[TurretManager] Turret spawned at " + cell);
        }
    }
}
