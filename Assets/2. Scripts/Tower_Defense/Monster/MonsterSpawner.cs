using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour, IGameStartListener
{
    [Header("Spawner Settings")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public List<Transform> waypoints;
    public float timeBetweenMonsters = 1f;
    public float waveInterval = 3f;

    private int waveCount = 0;
    private bool isSpawning = false;
    private Coroutine waveCoroutine;

    private void Start()
    {
        // GameStateManager에 자신을 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[MonsterSpawner] GameStateManager 인스턴스가 없습니다.");
        }
    }

    public void OnGameStart()
    {
        StartWave();
    }

    public void StartWave()
    {
        if (isSpawning) return;

        isSpawning = true;
        waveCoroutine = StartCoroutine(LoopWaves());
    }

    public void StopWave()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
            isSpawning = false;
            Debug.Log("[MonsterSpawner] 웨이브가 수동으로 중단되었습니다.");
        }
    }

    IEnumerator LoopWaves()
    {
        while (!PlayerHealth.isGameOver)
        {
            yield return StartCoroutine(SpawnWave());
            yield return new WaitForSeconds(waveInterval);
        }

        isSpawning = false; // 게임오버로 루프 종료
    }

    IEnumerator SpawnWave()
    {
        waveCount++;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateWaveUI(waveCount);

        int monstersPerWave = Mathf.Min(10, waveCount + 1);
        int monsterHealth = 3 + (waveCount - 1) / 2;

        for (int i = 0; i < monstersPerWave; i++)
        {
            if (PlayerHealth.isGameOver) yield break;

            SpawnMonster(monsterHealth);
            yield return new WaitForSeconds(timeBetweenMonsters);
        }
    }

    void SpawnMonster(int health)
    {
        Vector3 pos = spawnPoint.position + Vector3.up * 0.3f;
        GameObject m = Instantiate(monsterPrefab, pos, Quaternion.identity);

        MonsterStatus monsterStatus = m.GetComponent<MonsterStatus>();
        if (monsterStatus != null)
            monsterStatus.InitializeHealth(health);

        MonsterMovement mm = m.GetComponent<MonsterMovement>();
        if (mm != null)
            mm.waypoints = waypoints;
    }
}
