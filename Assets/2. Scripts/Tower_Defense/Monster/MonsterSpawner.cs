using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public List<Transform> waypoints;
    public float timeBetweenMonsters = 1f;
    public float waveInterval = 3f;

    private int waveCount = 0;
    private bool spawningWave = false;

    void Update()
    {
        if (PlayerHealth.isGameOver) return;

        if (!spawningWave)
            StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        spawningWave = true;
        waveCount++;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateWaveUI(waveCount);

        int monstersPerWave = Mathf.Min(10, waveCount + 1);
        int monsterHealth = 3 + (waveCount - 1) / 5;

        for (int i = 0; i < monstersPerWave; i++)
        {
            if (PlayerHealth.isGameOver) break;

            SpawnMonster(monsterHealth);
            yield return new WaitForSeconds(timeBetweenMonsters);
        }

        yield return new WaitForSeconds(waveInterval);
        spawningWave = false;
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
