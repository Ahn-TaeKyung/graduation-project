// 파일명: GameStateManager.cs (UI 제어 포함 최종본)
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
    [Tooltip("Ready 상태에서 켤 스테이지 선택 캔버스")]
    public GameObject m_stage_select_canvas; // [필수] 씬의 StageSelectMap_Panel을 연결
    [Tooltip("End 상태(게임오버)에서 켤 캔버스")]
    public GameObject m_end_canvas;
    [Tooltip("Clear 상태(게임클리어)에서 켤 캔버스")]
    public GameObject m_clear_canvas;

    // --- Networked State ---
    [Networked]
    public GameState CurrentState { get; private set; }
    
    [Networked]
    public int SelectedStageIndex { get; private set; } = -1;

    [Networked]
    [Tooltip("현재 맵 아이콘의 위치 인덱스 (모든 클라이언트가 동기화)")]
    public int CurrentIconIndex { get; set; } = 0; // [수정] Host가 쓸 수 있도록 public set
    
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

        // 캔버스 초기화 (모두 끄기)
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
            HandleStateChange(CurrentState); // 상태 변경 시 캔버스 켜고 끄기 실행
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
        if (!Object.HasStateAuthority)
        {
            Debug.LogError("Host가 아닌 플레이어가 스테이지 선택을 시도했습니다.");
            return;
        }

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
        
        // Host는 즉시 HandleStateChange를 로컬에서 호출
        HandleStateChange(newState); 
    }

    // Host와 Client 모두 FixedUpdateNetwork를 통해 이 함수를 호출 (상태 동기화)
    private void HandleStateChange(GameState state)
    {
        // [핵심] 모든 UI를 끈 상태에서 시작 (중복 표시 방지)
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
                
                // [핵심] 맵 선택 UI 켜기 (모든 클라이언트)
                if (m_stage_select_canvas != null) m_stage_select_canvas.SetActive(true);
                
                foreach (var listener in _gameReadyListeners) listener.OnGameReady();
                Debug.Log("[GameStateManager] 'Ready' 상태 진입. 맵 선택 UI 활성화.");
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners) listener.OnGameStart();
                Debug.Log($"[GameStateManager] 'Start' 상태 진입 (스테이지 {SelectedStageIndex}). 30초 카운트다운 시작.");
                
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_START_TO_PLAY);
                    SharedGameTimer = DURATION_START_TO_PLAY;
                }
                break;

            case GameState.Play:
                if (spawner != null) spawner.StartWave();
                Debug.Log("[GameStateManager] Play 상태로 전환됨 - 적 웨이브 시작");
                
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_PLAY_TO_END);
                    SharedGameTimer = DURATION_PLAY_TO_END;
                    
                    if (spawner != null && spawner.currentStageData != null)
                        CurrentStageHealth = spawner.currentStageData.StageHealth;
                    else
                        CurrentStageHealth = 1; 
                }
                break;

            case GameState.End: // 게임 오버
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_end_canvas != null)
                    m_end_canvas.SetActive(true); // 게임 오버 UI 켜기
                
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                }
                break;
                
            case GameState.Clear: // 게임 클리어
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_clear_canvas != null)
                    m_clear_canvas.SetActive(true); // 게임 클리어 UI 켜기
                
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                }
                break;
        }
    }
    
    // --- 인터페이스 구현 (비워두기) ---
    public void OnGameReady() { }
    public void OnGameStart() { }
    public void OnGameEnd() { }

    #endregion
}