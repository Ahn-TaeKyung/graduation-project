// 파일명: NetworkedVFXAutoDespawn.cs
using Fusion;
using UnityEngine;

// 이 스크립트는 네트워크로 스폰된 시각 효과(VFX)가
// 일정 시간 후 스스로를 Despawn하게 합니다.
[RequireComponent(typeof(NetworkObject))]
public class NetworkedVFXAutoDespawn : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 2.0f; // 이펙트의 총 재생 시간

    [Networked] private TickTimer _despawnTimer { get; set; }

    public override void Spawned()
    {
        // Host만 타이머를 설정합니다.
        if (Object.HasStateAuthority)
        {
            _despawnTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Host만 타이머를 확인하고 Despawn을 실행합니다.
        if (Object.HasStateAuthority && _despawnTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}