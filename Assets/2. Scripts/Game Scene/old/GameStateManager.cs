// 파일명: GameStateManager.cs (체력 초기화 시점 수정)
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System; 


public class GameStateManager : NetworkBehaviour, IGameReadyListener, IGameStartListener, IGameEndListener
{
    public static GameStateManager Instance { get; private set; }

    [Header("Core References")]
    public MonsterSpawner spawner;

    [Header("UI Canvases")]
    public GameObject m_stage_select_canvas; 
    public GameObject m_end_canvas;
    public GameObject m_clear_canvas;

    // --- Networked State ---
    [Networked]
    public GameState CurrentState { get; private set; }
    [Networked]
    public int SelectedStageIndex { get; private set; } = -1;
    [Networked]
    public int CurrentIconIndex { get; set; } = 0;
    [Networked]
    public int CurrentStageHealth { get; private set; }
    [Networked]
    public float SharedGameTimer { get; private set; }

    // --- Timer Config ---
    private const float DURATION_START_TO_PLAY = 30.0f; 
    private const float DURATION_PLAY_TO_END = 300.0f;
    [Networked] private TickTimer _stateTransitionTimer { get; set; }

    private readonly List<IGameReadyListener> _gameReadyListeners = new();
    private readonly List<IGameStartListener> _gameStartListeners = new();
    private readonly List<IGameEndListener> _gameEndListeners = new();
    private GameState _lastSyncedState;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void Spawned()
    {
        base.Spawned();
        if (Object.HasStateAuthority)
        {
            ChangeState(GameState.Loading);
        }
        _lastSyncedState = CurrentState;

        if (m_stage_select_canvas != null) m_stage_select_canvas.SetActive(false);
        if (m_end_canvas != null) m_end_canvas.SetActive(false);
        if (m_clear_canvas != null) m_clear_canvas.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        // 1. (클라이언트용) Host가 변경한 상태를 감지
        if (_lastSyncedState != CurrentState)
        {
            Debug.Log($"[Networked] GameState changed: {_lastSyncedState} → {CurrentState}");
            HandleStateChange(CurrentState);
            _lastSyncedState = CurrentState;
        }

        // 2. (Host 전용) 타이머 업데이트 및 상태 자동 전환
        if (Object.HasStateAuthority)
        {
            if (_stateTransitionTimer.IsRunning)
            {
                SharedGameTimer = _stateTransitionTimer.RemainingTime(Runner) ?? 0f;
                if (_stateTransitionTimer.Expired(Runner))
                {
                    _stateTransitionTimer = TickTimer.None; 
                    if (CurrentState == GameState.Start)
                    {
                        ChangeState(GameState.Play);
                    }
                    else if (CurrentState == GameState.Play)
                    {
                        ChangeState(GameState.Clear);
                    }
                }
            }
        }
    }

    #region Public RPCs (UI -> Host)

    // StageSelectUI(Host)가 "예" 버튼 클릭 시 호출
    public void HostSelectStage(int stageIndex, int iconButtonIndex)
    {
        if (!Object.HasStateAuthority) return;

        this.SelectedStageIndex = stageIndex;
        this.CurrentIconIndex = iconButtonIndex; 
        ChangeState(GameState.Start);
    }

    // GameResultUI(Host/Client)가 "다시시작" 버튼 클릭 시 호출
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReturnToReady()
    {
        if (!Object.HasStateAuthority) return;
        CurrentIconIndex = 0; 
        SelectedStageIndex = -1;
        ChangeState(GameState.Ready);
    }

    // EnemyNetwork(Host)가 호출
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerTakeDamage(int damage)
    {
        if (!Object.HasStateAuthority || CurrentState != GameState.Play) return;

        CurrentStageHealth -= damage;
        Debug.Log($"[GameStateManager] 스테이지 체력 감소. 남은 체력: {CurrentStageHealth}");

        if (CurrentStageHealth <= 0)
        {
            CurrentStageHealth = 0;
            ChangeState(GameState.End);
        }
    }

    #endregion

    #region Listener Registration
    public void RegisterListener(IGameReadyListener listener) { if (listener != null && !_gameReadyListeners.Contains(listener)) _gameReadyListeners.Add(listener); }
    public void RegisterListener(IGameStartListener listener) { if (listener != null && !_gameStartListeners.Contains(listener)) _gameStartListeners.Add(listener); }
    public void RegisterListener(IGameEndListener listener) { if (listener != null && !_gameEndListeners.Contains(listener)) _gameEndListeners.Add(listener); }
    public void UnregisterListener(IGameReadyListener listener) { if (listener != null) _gameReadyListeners.Remove(listener); }
    public void UnregisterListener(IGameStartListener listener) { if (listener != null) _gameStartListeners.Remove(listener); }
    public void UnregisterListener(IGameEndListener listener) { if (listener != null) _gameEndListeners.Remove(listener); }
    #endregion

    #region State Machine

    // Host만 이 함수를 호출해야 함
    public void ChangeState(GameState newState)
    {
        if (!Object.HasStateAuthority) return;
        
        if (CurrentState == newState) return;
        if ((CurrentState == GameState.End || CurrentState == GameState.Clear) && newState != GameState.Ready)
        {
            return;
        }

        Debug.Log($"[GameStateManager] Host가 상태 변경: {CurrentState} → {newState}");
        CurrentState = newState;
        
        HandleStateChange(newState); 
    }

    // Host와 Client 모두 FixedUpdateNetwork를 통해 이 함수를 호출 (상태 동기화)
    private void HandleStateChange(GameState state)
    {
        if (m_stage_select_canvas != null) m_stage_select_canvas.SetActive(false);
        if (m_end_canvas != null) m_end_canvas.SetActive(false);
        if (m_clear_canvas != null) m_clear_canvas.SetActive(false);

        switch (state)
        {
            case GameState.Loading: break;
            case GameState.Role:
                if (Object.HasStateAuthority) ChangeState(GameState.Ready);
                break;

            case GameState.Ready:
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                }

                if (m_stage_select_canvas != null) m_stage_select_canvas.SetActive(true);

                foreach (var listener in _gameReadyListeners) listener.OnGameReady();
                Debug.Log("[GameStateManager] 'Ready' 상태 진입. 맵 선택 UI 활성화.");
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners) listener.OnGameStart();
                Debug.Log($"[GameStateManager] 'Start' 상태 진입 (스테이지 {SelectedStageIndex}). 30초 카운트다운 시작.");

                if (Object.HasStateAuthority)
                {
                    // 1. 30초 타이머 시작
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_START_TO_PLAY);
                    SharedGameTimer = DURATION_START_TO_PLAY;

                    // 2. [핵심 수정] 스테이지 체력을 'Start' 상태에서 즉시 초기화
                    if (spawner != null && spawner.currentStageData != null)
                    {
                        CurrentStageHealth = spawner.currentStageData.StageHealth;
                        Debug.Log($"[GameStateManager] 스테이지 체력 설정: {CurrentStageHealth}");
                    }
                    else
                    {
                        CurrentStageHealth = 1; // 비상용 체력
                        Debug.LogError("[GameStateManager] MonsterSpawner 또는 currentStageData가 null입니다. 비상 체력 1로 시작.");
                    }
                }
                break;

            case GameState.Play:
                if (spawner != null) spawner.StartWave();
                Debug.Log("[GameStateManager] Play 상태로 전환됨 - 적 웨이브 시작");

                if (Object.HasStateAuthority)
                {
                    // 5분 타이머 시작
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_PLAY_TO_END);
                    SharedGameTimer = DURATION_PLAY_TO_END;

                    // [핵심 수정] 체력 설정 로직을 'Start'로 이동했으므로 여기서는 제거
                }
                break;

            case GameState.End: // 게임 오버
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_end_canvas != null)
                    m_end_canvas.SetActive(true);

                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                    CleanupNetworkObjects();
                }
                break;

            case GameState.Clear: // 게임 클리어
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_clear_canvas != null)
                    m_clear_canvas.SetActive(true);

                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                    CleanupNetworkObjects();
                }
                break;
        }
    }
    private void CleanupNetworkObjects()
    {
        // 이 로직은 Host에서만 실행되어야 합니다. (HandleStateChange의 if문이 보장)
        if (!Object.HasStateAuthority) return;

        Debug.Log("[GameStateManager] 게임 종료. 모든 몬스터, 타워, 총알을 정리합니다...");

        // 1. 모든 적 제거
        DespawnAllNetworkObjects<EnemyNetwork>();

        // 2. 모든 총알 제거
        DespawnAllNetworkObjects<Bullet>();

        // 3. 모든 타워 제거 (BowTurret, SwordTurret)
        DespawnAllNetworkObjects<TurretNetwork>(); // BowTurret 스크립트
        DespawnAllNetworkObjects<SwordTurretNetwork>(); // SwordTurret 스크립트
        
        // 4. (선택 사항) AoE 이펙트가 남아있다면 제거
        DespawnAllNetworkObjects<NetworkedVFXAutoDespawn>();
    }

    // [신규] 특정 타입의 모든 NetworkBehaviour를 찾아 Despawn하는 제네릭 함수
    private void DespawnAllNetworkObjects<T>() where T : NetworkBehaviour
    {
        // 씬에 있는 모든 T 타입의 NetworkBehaviour를 찾습니다.
        T[] objectsToDespawn = FindObjectsByType<T>(FindObjectsSortMode.None);
        
        Debug.Log($"[GameStateManager] ... {objectsToDespawn.Length}개의 {typeof(T).Name} 오브젝트 제거 중");

        foreach (T obj in objectsToDespawn)
        {
            // NetworkObject가 유효하고, 아직 Despawn되지 않았다면
            if (obj != null && obj.Object != null && obj.Object.IsValid)
            {
                Runner.Despawn(obj.Object);
            }
        }
    }
    // --- 인터페이스 구현 (비워두기) ---
    public void OnGameReady() { /* 다른 스크립트가 구현 */ }
    public void OnGameStart() { /* 다른 스크립트가 구현 */ }
    public void OnGameEnd() { /* 다른 스크립트가 구현 */ }

    #endregion
}