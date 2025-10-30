// 파일명: Bullet.cs
using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float despawnTime = 3f; // 최대 생존 시간

    [Networked] private Vector3 _direction { get; set; }
    [Networked] private TickTimer _lifeTimer { get; set; }
    
    // TurretNetwork.cs에서 호출하여 총알의 방향을 초기화합니다.
    public void Init(Vector3 direction)
    {
        // 이 함수는 Host에서만 호출됩니다.
        _direction = direction;
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, despawnTime);
    }

    public override void FixedUpdateNetwork()
    {
        // 1. 이동 (Host의 Logic으로 계산 후 클라이언트에 전파)
        transform.position += _direction * speed * Runner.DeltaTime;

        // 2. 수명 체크 (Host만 제거 권한 가짐)
        if (Object.HasStateAuthority && _lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        // 3. 충돌 체크 (Host만 충돌 처리를 통해 데미지를 적용)
        if (Object.HasStateAuthority)
        {
            // Physics.OverlapSphere 대신 SphereCast를 사용해 충돌 감지
            if (Runner.GetPhysicsScene().SphereCast(
                transform.position - _direction * speed * Runner.DeltaTime, // 이전 위치
                0.2f, // 총알 크기 (반지름)
                _direction,
                out var hit,
                speed * Runner.DeltaTime,
                LayerMask.GetMask("Enemy"))) 
            {
                // 적에게 데미지 적용
                if (hit.collider.TryGetComponent<EnemyNetwork>(out var enemy))
                {
                    // [핵심 수정] 
                    // TakeDamage를 호출하기 전에 적이 유효한지(스폰 완료 && 살아있는지) 확인합니다.
                    if (enemy != null && enemy.Object != null && enemy.Object.Id.IsValid && !enemy.IsDead)
                    {
                        enemy.TakeDamage(damage);
                    }
                    
                    // 총알은 적의 유효성(살았든 죽었든)과 관계없이 적중했으므로 제거합니다.
                    Runner.Despawn(Object); 
                    return; // 이 틱에서는 더 이상 처리할 필요가 없습니다.
                }
            }
        }
    }
    
    // 시각적 충돌 처리를 위한 Local Physics 구현 (옵션)
    private void OnCollisionEnter(Collision collision)
    {
        // Total Authority에서는 클라이언트에서 충돌이 감지되더라도 
        // 데미지 로직은 Host만 처리해야 합니다.
        // 여기서는 시각적 효과만 넣고, 데미지 로직은 FixedUpdateNetwork에서 처리합니다.
        
        // if (!Object.HasStateAuthority)
        // {
        //     // 폭발 효과, 사운드 등 로컬 효과만 실행
        // }
    }
}