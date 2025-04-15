using UnityEngine;
using System.Collections.Generic;

public class ModuleSpawner : MonoBehaviour
{
    [Header("������ ������")]
    public GameObject[] prefabOptions;

    [Header("���� ����")]
    public int spawnCount = 5;

    [Header("��� ��ǥ ���")]
    public Vector3[] relativePos;

    [Header("��ǥ���� �����Ǵ� ȸ����")]
    public Vector3[] rotationOffsets;

    [Header("�ߺ� �� ��ü ��ġ ����")]
    public float searchRadius = 3f;
    public int maxAttemptsPerDuplicate = 10;

    [Header("��� ������Ʈ ũ�� ����")]
    public Vector3 ModuleScale = Vector3.one;



    void Start()
    {
        SpawnModules();
    }

    /// <summary>
    /// ��� ��ġ �迭�� �������� ��� ������Ʈ�� �����Ͽ� �θ��� ���� ������Ʈ�� �����Ѵ�.
    /// </summary>
    void SpawnModules()
    {
        if (prefabOptions == null || prefabOptions.Length == 0)
        {
            Debug.LogWarning("������ �迭�� ��� �ֽ��ϴ�.");
            return;
        }

        if (relativePos == null || relativePos.Length == 0)
        {
            Debug.LogWarning("��� ��ǥ �迭�� ��� �ֽ��ϴ�.");
            return;
        }

        HashSet<Vector3> usedPos = new();
        List<Vector3> availablePos = new(relativePos);

        for (int i = 0; i < spawnCount; i++)
        {
            // �⺻ ��ġ ����
            Vector3 selectedPos;

            // �������� ��� ��ǥ ����
            if (availablePos.Count > 0)
            {
                int index = Random.Range(0, availablePos.Count);
                selectedPos = availablePos[index];
                availablePos.RemoveAt(index);
            }
            else
            {
                // �ĺ��� �� �̻� ������ �⺻������ �� ��ǥ ���� �õ�
                selectedPos = Vector3.zero;
            }
            Vector3 spawnPos = selectedPos;

            // �ߺ��� ��ġ�� �ֺ����� ��ü ��ġ ã��
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
                    Debug.LogWarning($"��ü ��ġ�� ã�� �� �����ϴ�: {spawnPos}");
                    continue;
                }
            }
            // ȸ���� ����
            Vector3 rotationEuler = Vector3.zero;
            if (i < rotationOffsets.Length)
            {
                rotationEuler = rotationOffsets[i];
            }
            usedPos.Add(spawnPos);

            Quaternion rotation = Quaternion.Euler(rotationEuler);

            // �������� ������ ����
            GameObject selectedPrefab = prefabOptions[Random.Range(0, prefabOptions.Length)];

            // �������� �θ��� ���� ������Ʈ(this.transform) �������� ����
            GameObject Module = Instantiate(selectedPrefab, transform);

            // ���� ��ǥ �� ȸ�� ����
            Module.transform.SetLocalPositionAndRotation(spawnPos, rotation);

            // ������ �ʱ�ȭ
            Module.transform.localScale = ModuleScale;
        }
    }
}
