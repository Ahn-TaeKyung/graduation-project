// 파일명: MonsterSpawner.cs
using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : NetworkBehaviour, IGameStartListener
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private EnemyDefinition basicEnemyDef; // 기본 적 데이터
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float timeBetweenSpawns = 0.5f;

    private Coroutine _waveCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public override void Spawned()
    {
        // Host든 Client든 리스너 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener((IGameStartListener)this);
        }
        else
        {
             Debug.LogWarning("[Spawner] GameStateManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    // IGameStartListener 구현
    public void OnGameStart()
    {
        // 게임 시작 시, Host만 Play 상태로 전환
        if (Object.HasStateAuthority)
        {
            GameStateManager.Instance.ChangeState(GameState.Play);
        }
    }

    // GameStateManager.HandleStateChange(GameState.Play)에서 호출됨
    public void StartWave()
    {
        if (!Object.HasStateAuthority) return;
        
        // 이전 코루틴이 있다면 정지
        if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
        
        // 웨이브 코루틴 시작
        _waveCoroutine = StartCoroutine(WaveRoutine());
        Debug.Log("[MonsterSpawner] 웨이브 루틴 시작됨.");
    }

    public void StopWave()
    {
        if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
        Debug.Log("[MonsterSpawner] 웨이브 루틴 중지됨.");
    }
    
    private IEnumerator WaveRoutine()
    {
        int waveCount = 1;
        while (GameStateManager.Instance.CurrentState == GameState.Play)
        {
            Debug.Log($"Wave {waveCount} 시작!");
            yield return StartCoroutine(SpawnWave(waveCount * 10)); // 웨이브마다 몬스터 수 증가
            
            // 모든 적이 죽을 때까지 기다려야 하지만, 여기서는 단순화하여 시간만 대기
            Debug.Log($"Wave {waveCount} 종료. 다음 웨이브까지 {timeBetweenWaves}초 대기.");
            yield return new WaitForSeconds(timeBetweenWaves);
            
            waveCount++;
        }
    }

    private IEnumerator SpawnWave(int count)
    {
        if (PathManager.Instance.Waypoints.Length == 0) yield break;
        Vector3 startPos = PathManager.Instance.Waypoints[0].position;
        
        for (int i = 0; i < count; i++)
        {
            // Host 권한으로 적 스폰
            Runner.Spawn(basicEnemyDef.NetworkPrefab, startPos, Quaternion.identity);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}