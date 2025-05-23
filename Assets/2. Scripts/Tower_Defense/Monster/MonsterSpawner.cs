using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab; // 몬스터 프리팹
    public Transform spawnPoint; // 몬스터 스폰 위치
    public List<Transform> waypoints; // 몬스터가 지나갈 경로
    public float timeBetweenMonsters = 1f; // 몬스터 간격
    public float waveInterval = 3f; // Wave 간격

    private int waveCount = 0; // 현재 Wave 번호
    private bool spawningWave = false;

    void Update()
    {
        // 게임 오버 상태면 스폰하지 않음
        if (PlayerHealth.isGameOver) return;

        if (!spawningWave)
            StartCoroutine(SpawnWave());
    }

    // Wave마다 몬스터 스폰하는 코루틴
    IEnumerator SpawnWave()
    {
        spawningWave = true;
        waveCount++;
        Debug.Log($"Wave {waveCount} 시작!");

        int monstersPerWave = 0;
        int monsterHealth = 3; // 기본 체력 3으로 시작

        // 1~4 웨이브는 몬스터 수만 증가 (2, 3, 4, 5)
        if (waveCount <= 4)
        {
            monstersPerWave = waveCount + 1; // 1웨이브에 2마리, 2웨이브에 3마리, 3웨이브에 4마리, 4웨이브에 5마리
        }
        else
        {
            // 5, 10, 15, 20, 25 웨이브마다 체력 증가
            int additionalHealth = (waveCount - 1) / 5; // 5, 10, 15, 20, 25 웨이브마다 체력 증가
            monsterHealth = 3 + additionalHealth; // 5, 10, 15, ... 웨이브에서 체력 증가

            // 5번째 이후부터 몬스터 수 증가 (5웨이브는 3마리, 6웨이브는 4마리, ...)
            if (waveCount % 5 == 1)
            {
                monstersPerWave = 4; // 6번째 웨이브에서 4마리
            }
            else if (waveCount % 5 == 2)
            {
                monstersPerWave = 5; // 7번째 웨이브에서 5마리
            }
            else if (waveCount % 5 == 3)
            {
                monstersPerWave = 6; // 8번째 웨이브에서 6마리
            }
            else if (waveCount % 5 == 4)
            {
                monstersPerWave = 7; // 9번째 웨이브에서 7마리
            }
        }

        // 몬스터 생성
        for (int i = 0; i < monstersPerWave; i++)
        {
            if (PlayerHealth.isGameOver) break;  // 게임 오버 상태일 경우 중지

            SpawnMonster(monsterHealth);
            yield return new WaitForSeconds(timeBetweenMonsters);  // 각 몬스터 간격을 기다림
        }

        // Wave 간격 기다리기
        yield return new WaitForSeconds(waveInterval);
        spawningWave = false;
    }

    // 몬스터를 생성하는 함수
    void SpawnMonster(int health)
    {
        Vector3 pos = spawnPoint.position + Vector3.up * 0.3f;  // 살짝 띄워서 생성
        GameObject m = Instantiate(monsterPrefab, pos, Quaternion.identity);

        // 체력 초기화
        MonsterStatus monsterStatus = m.GetComponent<MonsterStatus>();
        if (monsterStatus != null)
        {
            monsterStatus.InitializeHealth(health); // 체력 설정
        }

        // 몬스터 경로 설정
        MonsterMovement mm = m.GetComponent<MonsterMovement>();
        if (mm != null)
        {
            mm.waypoints = waypoints;
        }
    }
}
