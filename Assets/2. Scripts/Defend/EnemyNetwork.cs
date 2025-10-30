// 파일명: EnemyNetwork.cs (수정됨)
using Fusion;
using UnityEngine;

public class EnemyNetwork : NetworkBehaviour
{
    [Header("Enemy Data")]
    public EnemyDefinition Definition; // Scriptable Object 참조
    
    [Networked] public float Health { get; set; }
    [Networked] private int _currentWaypointIndex { get; set; }
    [Networked] public NetworkBool IsDead { get; private set; }
    
    // [오류 수정] OnChanged 속성 제거
    [Networked]
    private float _syncedHealthVisual { get; set; }
    
    // [추가] 변경 감지를 위한 로컬 변수
    private float _lastHealthVisual;

    [Header("Visuals")]
    [SerializeField] private GameObject visualsRoot;
    // TODO: 여기에 체력바(HealthBar) UI 컴포넌트(Image 등)를 연결해야 함
    // [SerializeField] private Image healthBar; 

    public override void Spawned()
    {
        if (Definition == null) return;
        
        if (Object.HasStateAuthority)
        {
            Health = Definition.MaxHealth;
            _syncedHealthVisual = Definition.MaxHealth;
            _currentWaypointIndex = 0;
            IsDead = false;
        }

        // [추가] 로컬 변수 초기화
        _lastHealthVisual = _syncedHealthVisual;
        // 초기 체력바 설정
        UpdateHealthVisual(_syncedHealthVisual);
    }

    // Render는 매 프레임 호출되며 시각적 업데이트에 적합합니다.
    public override void Render()
    {
        // [추가] Fusion 2 스타일의 변경 감지
        if (_lastHealthVisual != _syncedHealthVisual)
        {
            UpdateHealthVisual(_syncedHealthVisual);
            _lastHealthVisual = _syncedHealthVisual;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsDead) return;
        
        if (Object.HasStateAuthority)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        if (PathManager.Instance == null || PathManager.Instance.Waypoints.Length == 0) return;

        if (_currentWaypointIndex >= PathManager.Instance.Waypoints.Length)
        {
            ReachGoal();
            return;
        }
        
        Vector3 targetPos = PathManager.Instance.Waypoints[_currentWaypointIndex].position;
        float step = Definition.MoveSpeed * Runner.DeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            _currentWaypointIndex++;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (!Object.HasStateAuthority || IsDead) return;

        Health -= damage;
        _syncedHealthVisual = Health; // 시각적 변수도 Host에서 업데이트

        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!Object.HasStateAuthority) return;
        IsDead = true;
        Runner.Despawn(Object);
    }
    
    private void ReachGoal()
    {
        if (!Object.HasStateAuthority) return;
        Runner.Despawn(Object); 
    }

    // [제거됨] public static void OnHealthChanged(Changed<EnemyNetwork> change)
    // Fusion 2.x에서는 이 콜백을 사용하지 않습니다.

    // 클라이언트에서 시각적 업데이트 처리
    private void UpdateHealthVisual(float currentHealth)
    {
        // TODO: 몬스터 머리 위의 체력바를 업데이트하는 로직
        // if (healthBar != null && Definition != null && Definition.MaxHealth > 0)
        // {
        //     healthBar.fillAmount = currentHealth / Definition.MaxHealth;
        // }
        // Debug.Log($"Enemy Health Updated: {currentHealth}");
    }
}