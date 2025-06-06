using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    // 통합 변수
    [Header("생성 크기 설정")]
    public int nx = 3;
    public int ny = 3;
    public int nz = 3;

    // 각 컴포넌트 참조
    public CubeSpawner cubeSpawner;
    public AutoBoxColliderForGroup autoGroupCollider;
    public ModuleSpawner moduleSpawner;
    public List<GameObject> modulePrefabs = new List<GameObject>();
    [Header("모듈 배치 수")]
    public int spawnCount = 6;
    public static int moduleCount;

    void Reset() // 오브젝트에 붙일 때 자동 할당
    {
        cubeSpawner = GetComponent<CubeSpawner>();
        autoGroupCollider = GetComponent<AutoBoxColliderForGroup>();
        moduleSpawner = GetComponent<ModuleSpawner>();
    }
    private void Start()
    {
        StartManager.moduleCount = spawnCount;
        SpawnAll();
    }

    [ContextMenu("생성/충돌/모듈 순차 실행")]
    public void SpawnAll()
    {
        // 동기화: BaseManager의 nx/ny/nz → 각 스크립트에 적용
        if (cubeSpawner == null) cubeSpawner = GetComponent<CubeSpawner>();
        if (moduleSpawner == null) moduleSpawner = GetComponent<ModuleSpawner>();
        if (autoGroupCollider == null) autoGroupCollider = GetComponent<AutoBoxColliderForGroup>();

        cubeSpawner.nx = nx;
        cubeSpawner.ny = ny;
        cubeSpawner.nz = nz;

        moduleSpawner.nx = nx;
        moduleSpawner.ny = ny;
        moduleSpawner.nz = nz;
        moduleSpawner.spawnCount = spawnCount;
        moduleSpawner.prefabOptions = modulePrefabs;

        // 1. 큐브 생성
        cubeSpawner.GenerateCubeGroup();

        // 2. 충돌값 생성 (BoxCollider 크기 재설정)
        autoGroupCollider.FitColliderToChildren();

        // 3. 모듈 생성 (랜덤 배치)
        moduleSpawner.SpawnRandomOnBoxSurfaces();

        Debug.Log("BaseManager: nx=" + nx + ", ny=" + ny + ", nz=" + nz + ", moduleCount=" + spawnCount);
    }
}