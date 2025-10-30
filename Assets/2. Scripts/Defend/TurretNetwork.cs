// 파일명: TurretNetwork.cs (오류 수정 최종본)
using Fusion;
using UnityEngine;
// using System.Collections.Generic; // List 대신 Array를 사용하므로 제거해도 됨

public class TurretNetwork : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float fireRate = 1f; // 초당 발사 횟수
    [SerializeField] private Transform firePoint;
    [SerializeField] private NetworkPrefabRef bulletPrefab;
    [SerializeField] private LayerMask enemyLayer; // 적 오브젝트가 위치한 레이어

    [Networked] private TickTimer _fireCooldown { get; set; }
    
    private EnemyNetwork _currentTarget;

    // [오류 수정 1] List<Collider> 대신 Collider[] (배열)을 사용합니다.
    // OverlapSphere의 결과를 담을 미리 할당된 버퍼입니다.
    // 10은 동시에 감지할 수 있는 최대 적의 수입니다. (필요시 늘리세요)
    private Collider[] _overlapResultsBuffer = new Collider[10];
    private void Update()
    {
        // 이 로그가 안 찍히면 Turret 오브젝트가 씬에서 비활성화되어 있거나 Destroy된 것입니다.
        // Debug.Log($"[TURRET UNITY HEARTBEAT] {gameObject.name} is updating.");
    }    
    public override void FixedUpdateNetwork()
    {
        // Debug.Log($"[TURRET HEARTBEAT] Turret {Object.Id} is alive. Authority: {Object.HasStateAuthority}");
        // 공격 로직은 Host(StateAuthority)만 실행
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("Turret is running on Client Authority!");
            return;
        }
        // Debug.Log($"Turret FixedUpdateNetwork running. Cooldown: {_fireCooldown.RemainingTime(Runner)}");
        // 1. 타겟 유효성 검사
        if (_currentTarget == null || 
            !_currentTarget.Object || // (안전 체크 1) NetworkObject가 파괴되었는지 확인
            _currentTarget.IsDead || // (기존 로직)
            Vector3.Distance(transform.position, _currentTarget.transform.position) > range)
        {
            _currentTarget = FindNearestTarget();
        }
        // 2. 발사 로직
        if (_currentTarget != null && _fireCooldown.ExpiredOrNotRunning(Runner))
        {
            // Debug.Log($"Firing at target: {_currentTarget.name}");
            RotateTowardsTarget(_currentTarget.transform.position);
            Fire(_currentTarget.transform.position);
            _fireCooldown = TickTimer.CreateFromSeconds(Runner, 1f / fireRate);
        }
        if (_currentTarget == null && _fireCooldown.IsRunning == false)
        {
            _currentTarget = FindNearestTarget();
            if (_currentTarget != null)
            {
                 Debug.Log($"[Turret] Target Acquired: {_currentTarget.name}"); 
            }
        }
    }

    private EnemyNetwork FindNearestTarget()
    {
        // [오류 수정 1] OverlapSphere는 결과를 배열에 채우고, 감지된 개수(int)를 반환합니다.
        int hitCount = Runner.GetPhysicsScene().OverlapSphere(
            transform.position, 
            range, 
            _overlapResultsBuffer, // 결과를 담을 배열 버퍼
            enemyLayer, 
            // [오류 수정 2] HitOptions.None -> QueryTriggerInteraction.UseGlobal
            // "Queries Hit Triggers" 프로젝트 설정을 따르도록 합니다.
            QueryTriggerInteraction.UseGlobal 
        );
        
        EnemyNetwork bestTarget = null;
        float shortestDistance = float.MaxValue;

        // [오류 수정 1] hitCount 만큼만 순회합니다.
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _overlapResultsBuffer[i]; // 버퍼에서 콜라이더 가져오기

            if (hit.TryGetComponent<EnemyNetwork>(out var enemy) &&
                enemy.Object != null && // (안전 체크 1)
                !enemy.IsDead) // (기존 로직)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestTarget = enemy;
                }
            }
        }
        
        // (선택적) 버퍼 클리어 (다음 프레임을 위해)
        // System.Array.Clear(_overlapResultsBuffer, 0, hitCount);

        return bestTarget;
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; 
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Runner.DeltaTime * 10f);
    }

    private void Fire(Vector3 targetPos)
    {
        if (bulletPrefab == null) return;

        Vector3 direction = (targetPos - firePoint.position).normalized;
        
        NetworkObject spawnedBullet = Runner.Spawn(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );
        
        if (spawnedBullet.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Init(direction);
        }
    }
}