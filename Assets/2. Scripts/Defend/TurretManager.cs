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

        var runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("[TurretManager] NetworkRunner not found.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("[TurretManager] prefab is null!");
            return;
        }

        // Spawn
        NetworkObject spawned = runner.Spawn(prefab, worldPos, Quaternion.identity);

        // Spawn 후 owner 할당 (클라이언트 권한)
        if (spawned != null && owner != PlayerRef.None)
        {
            spawned.AssignInputAuthority(owner);
            Debug.Log("[TurretManager] Turret spawned with owner " + owner);
        }
        else if (spawned != null)
        {
            Debug.Log("[TurretManager] Turret spawned at " + cell);
        }
        else
        {
            Debug.LogError("[TurretManager] Spawn failed!");
        }
    }
}
