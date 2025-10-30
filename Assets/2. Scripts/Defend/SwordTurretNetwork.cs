// 파일명: SwordTurretNetwork.cs (Aatrox Q1 스타일)
using Fusion;
using UnityEngine;

public class SwordTurretNetwork : NetworkBehaviour
{
    [Header("Targeting Config")]
    [SerializeField] private float range = 10f; // 타겟을 찾는 사거리
    [SerializeField] private float attackRate = 1.5f; // 초당 공격 횟수
    [SerializeField] private LayerMask enemyLayer; 

    [Header("AoE Config")]
    [Tooltip("피해를 줄 2x2 범위의 중심에서 얼마나 퍼지는지 (반지름)")]
    [SerializeField] private float aoeRadius = 1.0f; // 1x1셀 = 1.0f. 2x2셀이므로 1.0f (반지름)
    [SerializeField] private float damage = 50f;

    [Header("References")]
    [Tooltip("타격 지점에 스폰할 네트워크 이펙트 프리팹")]
    [SerializeField] private NetworkPrefabRef aoeEffectPrefab; 

    [Networked] private TickTimer _attackCooldown { get; set; }
    
    private EnemyNetwork _currentTarget;
    private Collider[] _overlapResultsBuffer = new Collider[10]; // 타겟 탐지용
    private Collider[] _aoeDamageBuffer = new Collider[30];    // AoE 피해용

    public override void FixedUpdateNetwork()
    {
        // Host(StateAuthority)만 공격 로직 실행
        if (!Object.HasStateAuthority) return;

        // 1. 타겟 유효성 검사 (BowTurret과 동일)
        if (_currentTarget == null || 
            !_currentTarget.Object || 
            !_currentTarget.Object.Id.IsValid || // 스폰 완료 확인
            _currentTarget.IsDead || 
            Vector3.Distance(transform.position, _currentTarget.transform.position) > range)
        {
            _currentTarget = FindNearestTarget();
        }

        // 2. 공격 로직
        if (_currentTarget != null && _attackCooldown.ExpiredOrNotRunning(Runner))
        {
            // 타겟을 향해 회전 (옵션)
            RotateTowardsTarget(_currentTarget.transform.position); 

            // 공격 실행
            PerformGroundAoEAttack(_currentTarget.transform.position);

            // 쿨다운 리셋
            _attackCooldown = TickTimer.CreateFromSeconds(Runner, 1f / attackRate);
        }
    }

    // Host에서만 실행: 타겟의 '위치'에 AoE 공격
    private void PerformGroundAoEAttack(Vector3 targetPosition)
    {
        // 1. 공격 위치 계산 (가장 가까운 그리드 셀의 중심)
        Vector3 attackWorldPos = targetPosition; // 기본값
        if (GridManager.Instance.WorldToGrid(targetPosition, out Vector2Int gridPos))
        {
            // 2x2의 중심을 계산하기 위해 0.5f를 더한 셀 위치를 사용
            Vector3 worldPosBL = GridManager.Instance.GridToWorld(gridPos);
            // 2x2 영역의 중심 (가로 1셀, 세로 1셀 이동)
            attackWorldPos = worldPosBL + new Vector3(GridManager.Instance.cellSize * 0.5f, 0, GridManager.Instance.cellSize * 0.5f);
        }

        // 2. VFX 스폰 (Host가 스폰 -> 모든 클라에 보임)
        if (aoeEffectPrefab != null)
        {
            Runner.Spawn(
                aoeEffectPrefab, 
                attackWorldPos, // 그리드 중앙
                Quaternion.identity
            );
        }

        // 3. 2x2 범위 데미지 (Host만 계산)
        // OverlapBox를 사용하여 2x2 영역(cellSize * 2)을 정확히 타격
        Vector3 halfExtents = new Vector3(GridManager.Instance.cellSize, 0.5f, GridManager.Instance.cellSize);
        int hitCount = Runner.GetPhysicsScene().OverlapBox(
            attackWorldPos,
            halfExtents,
            _aoeDamageBuffer,
            Quaternion.identity,
            enemyLayer,
            QueryTriggerInteraction.UseGlobal
        );

        Debug.Log($"[SwordTurret] {gridPos} 위치에 2x2 AoE 공격! {hitCount}마리 타격.");

        for (int i = 0; i < hitCount; i++)
        {
            if (_aoeDamageBuffer[i].TryGetComponent<EnemyNetwork>(out var enemy) &&
                enemy.Object != null &&
                enemy.Object.Id.IsValid &&
                !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    // BowTurret과 동일한 타겟 찾기 로직
    private EnemyNetwork FindNearestTarget()
    {
        int hitCount = Runner.GetPhysicsScene().OverlapSphere(
            transform.position, 
            range, 
            _overlapResultsBuffer, 
            enemyLayer, 
            QueryTriggerInteraction.UseGlobal
        );
        
        EnemyNetwork bestTarget = null;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _overlapResultsBuffer[i];
            if (hit.TryGetComponent<EnemyNetwork>(out var enemy) &&
                enemy.Object != null &&
                enemy.Object.Id.IsValid &&
                !enemy.IsDead)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestTarget = enemy;
                }
            }
        }
        return bestTarget;
    }
    
    // BowTurret과 동일한 회전 로직 (선택 사항)
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; 
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Runner.DeltaTime * 10f);
    }
}