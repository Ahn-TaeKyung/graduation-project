using System.Collections.Generic;
using UnityEngine;

public class ModuleSpawner : MonoBehaviour
{
    [Header("프리팹 리스트 (랜덤 선택)")]
    public List<GameObject> prefabOptions;

    [Header("큐브 구조 (직육면체)")]
    [Range(1, 100)] public int nx = 3;
    [Range(1, 100)] public int ny = 3;
    [Range(1, 100)] public int nz = 3;
    public float cubeSize = 1.0f;
    public float gap = 0.1f;

    [Header("총 생성 개수")]
    [Range(1, 10000)] public int spawnCount = 10;

    [Header("생성 옵션")]
    public bool clearOnSpawn = true;

    private List<GameObject> spawnedObjects = new();

    [ContextMenu("Spawn Random On Box Surfaces")]
    public void SpawnRandomOnBoxSurfaces()
    {
        if (prefabOptions == null || prefabOptions.Count == 0)
        {
            Debug.LogError("프리팹 리스트가 비어 있습니다.");
            return;
        }

        if (clearOnSpawn)
        {
            foreach (var obj in spawnedObjects)
                if (obj) DestroyImmediate(obj);
            spawnedObjects.Clear();
        }

        float sx = cubeSize + gap;
        float sy = cubeSize + gap;
        float sz = cubeSize + gap;

        float halfX = (nx - 1) / 2f;
        float halfY = (ny - 1) / 2f;
        float halfZ = (nz - 1) / 2f;

        // 1. 모든 외벽 후보 좌표와 방향 수집(중복 X)
        var candidates = new List<(Vector3 pos, Vector3 normal, Vector3 up)>();
        HashSet<string> uniqueChecker = new();

        // +X, -X 면
        for (int y = 0; y < ny; y++)
            for (int z = 0; z < nz; z++)
            {
                // +X
                Vector3 p1 = new Vector3(+halfX * sx + 0.5f * sx, (y - halfY) * sy, (z - halfZ) * sz);
                if (uniqueChecker.Add(p1.ToString()))
                    candidates.Add((p1, Vector3.right, Vector3.up));
                // -X
                Vector3 p2 = new Vector3(-halfX * sx - 0.5f * sx, (y - halfY) * sy, (z - halfZ) * sz);
                if (uniqueChecker.Add(p2.ToString()))
                    candidates.Add((p2, Vector3.left, Vector3.up));
            }
        // +Y, -Y 면
        for (int x = 0; x < nx; x++)
            for (int z = 0; z < nz; z++)
            {
                // +Y
                Vector3 p3 = new Vector3((x - halfX) * sx, +halfY * sy + 0.5f * sy, (z - halfZ) * sz);
                if (uniqueChecker.Add(p3.ToString()))
                    candidates.Add((p3, Vector3.up, Vector3.back));
                // -Y
                Vector3 p4 = new Vector3((x - halfX) * sx, -halfY * sy - 0.5f * sy, (z - halfZ) * sz);
                if (uniqueChecker.Add(p4.ToString()))
                    candidates.Add((p4, Vector3.down, Vector3.forward));
            }
        // +Z, -Z 면
        for (int x = 0; x < nx; x++)
            for (int y = 0; y < ny; y++)
            {
                // +Z
                Vector3 p5 = new Vector3((x - halfX) * sx, (y - halfY) * sy, +halfZ * sz + 0.5f * sz);
                if (uniqueChecker.Add(p5.ToString()))
                    candidates.Add((p5, Vector3.forward, Vector3.up));
                // -Z
                Vector3 p6 = new Vector3((x - halfX) * sx, (y - halfY) * sy, -halfZ * sz - 0.5f * sz);
                if (uniqueChecker.Add(p6.ToString()))
                    candidates.Add((p6, Vector3.back, Vector3.up));
            }

        // 2. 랜덤 셔플
        int totalCandidates = candidates.Count;
        for (int i = 0; i < totalCandidates; i++)
        {
            int j = Random.Range(i, totalCandidates);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // 3. 개수만큼만 생성
        int count = Mathf.Min(spawnCount, candidates.Count);
        Debug.Log($"후보 좌표 개수: {candidates.Count}, 생성 요청: {count}, 프리팹 개수: {prefabOptions.Count}");
        for (int i = 0; i < count; i++)
        {
            var (pos, normal, up) = candidates[i];
            Quaternion rot = Quaternion.LookRotation(normal, up);
            GameObject selectedPrefab = prefabOptions[Random.Range(0, prefabOptions.Count)];
            GameObject go = Instantiate(selectedPrefab, transform.TransformPoint(pos), rot, transform);
            spawnedObjects.Add(go);
        }

        Debug.Log($"전체 바깥면 후보 {candidates.Count}개 중 {count}개 생성 완료");
    }
}