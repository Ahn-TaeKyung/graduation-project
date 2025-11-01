// 파일명: WaveSpawnGroup.cs
using UnityEngine;

// 이 스크립트는 파일로 만들지만, MonoBehaviour가 아니므로 씬에 붙이지 않습니다.
// WaveData가 이 데이터를 사용합니다.

[System.Serializable]
public class WaveSpawnGroup
{
    [Tooltip("이 그룹에서 스폰할 적의 EnemyDefinition 에셋")]
    public EnemyDefinition enemyDef;
    
    [Tooltip("이 적을 몇 마리 스폰할지")]
    public int count = 10;

    [Tooltip("이 그룹의 적을 한 마리씩 스폰할 때의 시간 간격")]
    public float timeBetweenSpawns = 0.5f;

    [Tooltip("이 그룹이 시작되기 전의 대기 시간 (앞 그룹이 끝난 후)")]
    public float delayBeforeGroup = 1.0f;
}