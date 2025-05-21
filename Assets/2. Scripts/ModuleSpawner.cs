using UnityEngine;
using System.Collections.Generic;

public class ModuleSpawner : MonoBehaviour
{
    [Header("생성할 프리팹")]
    public GameObject[] prefabOptions;

    [Header("생성 설정")]
    public int spawnCount = 5;

    [Header("상대 좌표 목록")]
    public Vector3[] relativePos;

    [Header("좌표마다 대응되는 회전값")]
    public Vector3[] rotationOffsets;

    [Header("중복 시 대체 위치 설정")]
    public float searchRadius = 3f;
    public int maxAttemptsPerDuplicate = 10;

    [Header("모듈 오브젝트 크기 설정")]
    public Vector3 ModuleScale = Vector3.one;



    void Start()
    {
        SpawnModules();
    }

    /// <summary>
    /// 상대 위치 배열을 기준으로 모듈 오브젝트를 생성하여 부모인 현재 오브젝트에 부착한다.
    /// </summary>
    void SpawnModules()
    {
        if (prefabOptions == null || prefabOptions.Length == 0)
        {
            Debug.LogWarning("프리팹 배열이 비어 있습니다.");
            return;
        }

        if (relativePos == null || relativePos.Length == 0)
        {
            Debug.LogWarning("상대 좌표 배열이 비어 있습니다.");
            return;
        }

        HashSet<Vector3> usedPos = new();
        List<Vector3> availablePos = new(relativePos);

        for (int i = 0; i < spawnCount; i++)
        {
            // 기본 위치 설정
            Vector3 selectedPos;

            // 무작위로 상대 좌표 선택
            if (availablePos.Count > 0)
            {
                int index = Random.Range(0, availablePos.Count);
                selectedPos = availablePos[index];
                availablePos.RemoveAt(index);
            }
            else
            {
                // 후보가 더 이상 없으면 기본값에서 새 좌표 생성 시도
                selectedPos = Vector3.zero;
            }
            Vector3 spawnPos = selectedPos;

            // 중복된 위치는 주변에서 대체 위치 찾기
            if (usedPos.Contains(spawnPos))
            {
                bool found = false;
                for (int j = 0; j < maxAttemptsPerDuplicate; j++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-searchRadius, searchRadius),
                        Random.Range(-searchRadius, searchRadius),
                        Random.Range(-searchRadius, searchRadius)
                    );
                    Vector3 candidate = spawnPos + offset;

                    if (!usedPos.Contains(candidate))
                    {
                        spawnPos = candidate;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"대체 위치를 찾을 수 없습니다: {spawnPos}");
                    continue;
                }
            }
            // 회전값 설정
            Vector3 rotationEuler = Vector3.zero;
            if (i < rotationOffsets.Length)
            {
                rotationEuler = rotationOffsets[i];
            }
            usedPos.Add(spawnPos);

            Quaternion rotation = Quaternion.Euler(rotationEuler);

            // 무작위로 프리팹 설정
            GameObject selectedPrefab = prefabOptions[Random.Range(0, prefabOptions.Length)];

            // 프리팹을 부모인 현재 오브젝트(this.transform) 기준으로 생성
            GameObject Module = Instantiate(selectedPrefab, transform);

            // 로컬 좌표 및 회전 설정
            Module.transform.SetLocalPositionAndRotation(spawnPos, rotation);

            // 스케일 초기화
            Module.transform.localScale = ModuleScale;
        }
    }
}
