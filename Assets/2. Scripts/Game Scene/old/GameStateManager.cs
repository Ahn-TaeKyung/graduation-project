using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("References")]
    public MonsterSpawner spawner;
    public GameObject m_end_canvas;

    [Networked] 
    public GameState CurrentState { get; private set; }

    private readonly List<IGameReadyListener> _gameReadyListeners = new();
    private readonly List<IGameStartListener> _gameStartListeners = new();
    private readonly List<IGameEndListener> _gameEndListeners = new();

    private GameState _lastSyncedState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        if (Object.HasStateAuthority)
        {
            // 호스트(방장)만 상태 초기화 가능
            ChangeState(GameState.Loading);
        }

        _lastSyncedState = CurrentState;
    }

    public override void FixedUpdateNetwork()
    {
        // Fusion 2.x에서는 직접 비교를 통해 상태 변경 감지
        if (_lastSyncedState != CurrentState)
        {
            Debug.Log($"[Networked] GameState changed: {_lastSyncedState} → {CurrentState}");
            HandleStateChange(CurrentState);
            _lastSyncedState = CurrentState;
        }
    }

    #region Listener Registration

    public void RegisterListener(IGameReadyListener listener)
    {
        if (!_gameReadyListeners.Contains(listener))
            _gameReadyListeners.Add(listener);
    }

    public void RegisterListener(IGameStartListener listener)
    {
        if (!_gameStartListeners.Contains(listener))
            _gameStartListeners.Add(listener);
    }

    public void RegisterListener(IGameEndListener listener)
    {
        if (!_gameEndListeners.Contains(listener))
            _gameEndListeners.Add(listener);
    }

    public void UnregisterListener(IGameReadyListener listener)
    {
        _gameReadyListeners.Remove(listener);
    }

    public void UnregisterListener(IGameStartListener listener)
    {
        _gameStartListeners.Remove(listener);
    }

    public void UnregisterListener(IGameEndListener listener)
    {
        _gameEndListeners.Remove(listener);
    }

    #endregion

    #region State Change Logic

    public void ChangeState(GameState newState)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("[GameStateManager] Only the host can change the game state!");
            return;
        }

        if (CurrentState == newState) return;

        Debug.Log($"[GameStateManager] Changing state: {CurrentState} → {newState}");
        CurrentState = newState;

        HandleStateChange(newState);
    }

    private void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Loading:
                break;

            case GameState.Role:
                if (GameSceneManager.Instance != null)
                    GameSceneManager.Instance.SetupById();

                ChangeState(GameState.Ready);
                break;

            case GameState.Ready:
                foreach (var listener in _gameReadyListeners)
                    listener.OnGameReady();

                Debug.Log("[GameStateManager] 모든 IGameReadyListener 초기화 완료");
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners)
                    listener.OnGameStart();

                Debug.Log("[GameStateManager] 모든 IGameStartListener 초기화 완료");
                break;

            case GameState.Play:
                // Play 상태 진입 시 적 스폰 시작 등
                if (spawner != null)
                    spawner.StartWave();

                Debug.Log("[GameStateManager] Play 상태로 전환됨 - 적 웨이브 시작");
                break;

            case GameState.End:
                foreach (var listener in _gameEndListeners)
                    listener.OnGameEnd();

                if (spawner != null)
                    spawner.StopWave();

                if (GameManager.Instance != null)
                    GameManager.Instance.PauseGameTime();

                if (m_end_canvas != null)
                    m_end_canvas.SetActive(true);

                Debug.Log("[GameStateManager] End 상태 진입 - 게임 종료 처리 완료");
                break;
        }
    }

    #endregion
    // 클라이언트가 설치 요청 시 호출 (RPC 형태)
    // 구현 방식: Fusion RPC 또는 Networked 이벤트 사용 가능. 여기선 간단 wrapper로 구현.
    // 클라이언트가 호출하면 Host가 실제 Spawn 수행
    public void RequestPlaceTurretRPC(Vector2Int cell, int turretIndex, PlayerRef requester)
    {
        // 만약 이 인스턴스가 Host(StateAuthority)라면 바로 Spawn 처리
        if (Object.HasStateAuthority)
        {
            HandlePlaceRequestOnHost(cell, turretIndex, requester);
        }
        else
        {
            // Host에게 RPC로 전송해야 함
            // Fusion에서 RPC 사용법을 프로젝트에 맞게 사용하세요.
            // 아래는 의사코드 — 실제 RPC 호출은 Runner.RPC(...) 등으로 구현해야 합니다.
            var runner = FindObjectOfType<NetworkRunner>();
            if (runner == null) { Debug.LogError("No runner for RPC"); return; }
            // runner.RPC(...); // 프로젝트의 RPC API 사용
            Debug.Log("[GameStateManager] Non-host requested place turret — send RPC to host (implement RPC)");
        }
    }

    // Host에서 최종 검증·스폰
    private void HandlePlaceRequestOnHost(Vector2Int cell, int turretIndex, PlayerRef requester)
    {
        // turretIndex => 해당 TurretDefinition (인스펙터에 저장된 리스트에서 찾음)
        var tm = FindObjectOfType<TurretManager>();
        if (tm == null) { Debug.LogError("TurretManager missing"); return; }

        // find turret definition from a centralized store — 예시: TurretDatabase singleton (간단화)
        var defs = FindObjectOfType<TurretDatabase>();
        if (defs == null) { Debug.LogError("TurretDatabase missing"); return; }

        if (turretIndex < 0 || turretIndex >= defs.definitions.Length) { Debug.LogError("Invalid turretIndex"); return; }
        var def = defs.definitions[turretIndex];
        if (def == null || def.turretNetworkPrefab == null) { Debug.LogError("TurretDef or prefab missing"); return; }

        // 검증
        if (!GridManager.Instance.IsAreaFree(cell, def.size.x, def.size.y))
        {
            Debug.Log("[GameStateManager] Host: 설치 검증 실패");
            return;
        }

        // spawn
        var networkObj = def.turretNetworkPrefab.GetComponent<NetworkObject>();
        if (networkObj == null) { Debug.LogError("Prefab has no NetworkObject"); return; }

        tm.SpawnTurretOnHost(networkObj, cell, def.size, requester);
    }
}
