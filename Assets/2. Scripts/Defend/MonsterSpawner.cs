// 파일명: MonsterSpawner.cs (데이터 기반 최종본)
using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : NetworkBehaviour, IGameStartListener
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Stage Configuration")]
    [Tooltip("현재 스테이지에서 사용할 StageData 에셋")]
    [SerializeField] private StageData currentStageData;

    // [제거됨] basicEnemyDef, AromorEnemyDef (이제 StageData가 관리)
    // [제거됨] timeBetweenWaves, timeBetweenSpawns (이제 WaveData가 관리)

    private Coroutine _waveCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public override void Spawned()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener((IGameStartListener)this);
        }
    }

    // "Start" 상태 진입 시 호출 (GameStateManager가 30초 대기 시작)
    public void OnGameStart()
    {
        Debug.Log("[MonsterSpawner] GameState.Start 진입. 30초 대기 시작.");
    }

    // "Play" 상태 진입 시 호출 (GameStateManager가 호출)
    public void StartWave()
    {
        if (!Object.HasStateAuthority) return;
        
        if (currentStageData == null)
        {
            Debug.LogError("[MonsterSpawner] currentStageData가 할당되지 않았습니다! 스포너를 시작할 수 없습니다.");
            return;
        }

        if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
        _waveCoroutine = StartCoroutine(StageRoutine());
        Debug.Log("[MonsterSpawner] 스테이지 루틴 시작됨.");
    }

    public void StopWave()
    {
        if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
        Debug.Log("[MonsterSpawner] 웨이브 루틴 중지됨.");
    }
    
    // [수정] StageData를 기반으로 모든 웨이브를 순차적으로 실행
    private IEnumerator StageRoutine()
    {
        int waveCount = 1;

        // StageData에 정의된 모든 웨이브를 순회합니다.
        foreach (WaveData wave in currentStageData.waves)
        {
            // Host가 아니거나 Play 상태가 아니면 즉시 중지
            if (!Object.HasStateAuthority || GameStateManager.Instance.CurrentState != GameState.Play)
                yield break;

            Debug.Log($"Wave {waveCount} ({wave.name}) 시작!");
            
            // 현재 웨이브 스폰 코루틴을 실행하고 끝날 때까지 대기
            yield return StartCoroutine(SpawnWaveRoutine(wave, waveCount)); 
            
            Debug.Log($"Wave {waveCount} ({wave.name}) 종료. 다음 웨이브까지 {wave.timeAfterWave}초 대기.");
            yield return new WaitForSeconds(wave.timeAfterWave);
            
            waveCount++;
        }

        // 모든 웨이브가 끝났습니다.
        Debug.Log("[MonsterSpawner] 스테이지의 모든 웨이브가 종료되었습니다.");
        // (타이머가 0이 되거나, Host가 수동으로 End 상태를 호출할 때까지 대기)
    }

    // [수정] WaveData를 기반으로 한 그룹의 적들을 스폰
    private IEnumerator SpawnWaveRoutine(WaveData waveData, int waveCount)
    {
        if (PathManager.Instance == null || PathManager.Instance.Waypoints.Length == 0)
        {
             Debug.LogError("[MonsterSpawner] PathManager의 Waypoints가 없습니다!");
             yield break;
        }
        Vector3 startPos = PathManager.Instance.Waypoints[0].position;

        // 이 웨이브에 속한 모든 '적 그룹'을 순회합니다.
        foreach (WaveSpawnGroup group in waveData.spawnGroups)
        {
            // 그룹 시작 전 딜레이
            yield return new WaitForSeconds(group.delayBeforeGroup);

            // [안전 코드]
            if (group.enemyDef == null)
            {
                Debug.LogError($"[MonsterSpawner] {waveData.name}에 Enemy Def가 할당되지 않은 그룹이 있습니다!");
                continue;
            }
            if (group.enemyDef.NetworkPrefab == null)
            {
                Debug.LogError($"[MonsterSpawner] {group.enemyDef.name} 에 Network Prefab이 없습니다!");
                continue;
            }
            if (waveCount > 6 && waveCount < 9)
            {
                group.enemyDef.MoveSpeed = group.enemyDef.MoveSpeed * 1.2f;
            }

            // 이 그룹의 적을 'count'만큼 스폰합니다.
            for (int i = 0; i < group.count; i++)
            {
                // Host가 아니거나 Play 상태가 아니면 즉시 중지
                if (!Object.HasStateAuthority || GameStateManager.Instance.CurrentState != GameState.Play)
                    yield break;
                    
                Runner.Spawn(group.enemyDef.NetworkPrefab, startPos, Quaternion.identity);
                
                // 이 그룹에 정의된 스폰 간격만큼 대기
                yield return new WaitForSeconds(group.timeBetweenSpawns);
            }
        }
    }
}