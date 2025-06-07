using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour, IGameReadyListener
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
    private GameSceneManager GameSceneManager;

    [Header("모듈 배치 수")]
    public int spawnCount = 6;
    public static int moduleCount;

    void Reset() // 오브젝트에 붙을 때 자동 할당
    {
        cubeSpawner = GetComponent<CubeSpawner>();
        autoGroupCollider = GetComponent<AutoBoxColliderForGroup>();
        moduleSpawner = GetComponent<ModuleSpawner>();
    }

    private void Start()
    {
        // GameStateManager가 준비되었을 때 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[StartManager] GameStateManager 인스턴스가 없음.");
        }
    }

    public void OnGameReady()
    {
        GameSceneManager = FindFirstObjectByType<GameSceneManager>();
        RoleType myRole = GameSceneManager.GetMyRole();
        if (myRole != RoleType.Hacker)
        {
            return; // 내 역할이 아니라면 아무 것도 안 함
        }
        StartManager.moduleCount = spawnCount;
        SpawnAll();
    }

    [ContextMenu("생성/충돌/모듈 순차 실행")]
    public void SpawnAll()
    {
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

        Debug.Log("StartManager: nx=" + nx + ", ny=" + ny + ", nz=" + nz + ", moduleCount=" + spawnCount);
    }
}
