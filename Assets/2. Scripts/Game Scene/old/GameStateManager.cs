// 파일명: GameStateManager.cs (Start -> Play 타이머 수정본)
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System; 

public class GameStateManager : NetworkBehaviour, IGameReadyListener, IGameStartListener, IGameEndListener
{
    public static GameStateManager Instance { get; private set; }

    [Header("References")]
    public MonsterSpawner spawner;
    public GameObject m_end_canvas;
    [Tooltip("게임 클리어 시 활성화할 캔버스")]
    public GameObject m_clear_canvas; // [신규] 게임 클리어 캔버스

    [Networked]
    public GameState CurrentState { get; private set; }

    // [신규] 현재 선택된 스테이지 인덱스 (모든 클라이언트가 알아야 함)
    [Networked]
    public int SelectedStageIndex { get; private set; } = -1; // -1 = 선택 안됨
    // --- 타이머 시스템 ---
    // [수정] Ready 타이머 상수를 Start 타이머 상수로 변경
    private const float DURATION_START_TO_PLAY = 30.0f; 
    private const float DURATION_PLAY_TO_END = 300.0f; // 5분 = 300초

    [Networked] private TickTimer _stateTransitionTimer { get; set; }

    // UI가 Polling(수동 확인)할 네트워크 변수
    [Networked]
    public float SharedGameTimer { get; private set; }
    
    // (OnChangedRender 및 UI 이벤트 관련 코드 모두 제거됨)
    // --- [신규] 스테이지 체력 시스템 ---
    [Networked]
    [Tooltip("현재 스테이지의 남은 체력 (UI Polling용)")]
    public int CurrentStageHealth { get; private set; }
    
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
        // 캔버스 초기화
        if (m_end_canvas != null) m_end_canvas.SetActive(false);
        if (m_clear_canvas != null) m_clear_canvas.SetActive(false);
    }
    // [신규] Host의 StageSelectUI가 이 함수를 호출합니다.
    // 이 함수는 RPC를 호출하여 모든 클라이언트에게 상태 변경을 전파합니다.
    public void HostSelectStage(int stageIndex)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogError("Host가 아닌 플레이어가 스테이지 선택을 시도했습니다.");
            return;
        }

        // Host가 RPC를 호출하여 SelectedStageIndex를 설정하고 상태를 Start로 변경
        RPC_SelectAndStartStage(stageIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SelectAndStartStage(int stageIndex)
    {
        // 모든 클라이언트가 선택된 스테이지 인덱스를 알게 됨
        this.SelectedStageIndex = stageIndex;

        // Host만 다음 상태로 변경 (이 변경은 어차피 CurrentState [Networked] 변수를 통해 전파됨)
        if (Object.HasStateAuthority)
        {
            ChangeState(GameState.Start);
        }
    }
    // [신규] 적이 목표에 도달했을 때 호출될 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerTakeDamage(int damage)
    {
        // Host만, 그리고 Play 상태일 때만 체력을 감소시킴
        if (!Object.HasStateAuthority || CurrentState != GameState.Play) return;

        CurrentStageHealth -= damage;
        Debug.Log($"[GameStateManager] 스테이지 체력 감소. 남은 체력: {CurrentStageHealth}");

        if (CurrentStageHealth <= 0)
        {
            CurrentStageHealth = 0;
            // 체력이 0이 되면 'End' (게임 오버) 상태로 전환
            Debug.Log("[GameStateManager] 스테이지 체력 0. Game Over!");
            ChangeState(GameState.End);
        }
    }
    public override void FixedUpdateNetwork()
    {
        // 1. 클라이언트에서 상태 변경 감지
        if (_lastSyncedState != CurrentState)
        {
            Debug.Log($"[Networked] GameState changed: {_lastSyncedState} → {CurrentState}");
            HandleStateChange(CurrentState);
            _lastSyncedState = CurrentState;
        }

        // 2. Host에서만 타이머를 업데이트하고 상태를 전환
        if (Object.HasStateAuthority)
        {
            if (_stateTransitionTimer.IsRunning)
            {
                // UI에 표시될 남은 시간을 업데이트
                SharedGameTimer = _stateTransitionTimer.RemainingTime(Runner) ?? 0f;

                if (_stateTransitionTimer.Expired(Runner))
                {
                    _stateTransitionTimer = TickTimer.None; 

                    // [수정] Ready 상태 확인 제거
                    if (CurrentState == GameState.Start) // Start -> Play
                    {
                        Debug.Log("[GameStateManager] Start 타이머 만료. Play 상태로 전환.");
                        ChangeState(GameState.Play);
                    }
                    else if (CurrentState == GameState.Play) // Play -> End
                    {
                        Debug.Log("[GameStateManager] Play 타이머 만료. End 상태로 전환.");
                        ChangeState(GameState.End);
                    }
                }
            }
        }
    }

    #region Listener Registration
    // ... (리스너 등록/해제 코드는 변경 없음) ...
    public void RegisterListener(IGameReadyListener listener) { if (!_gameReadyListeners.Contains(listener)) _gameReadyListeners.Add(listener); }
    public void RegisterListener(IGameStartListener listener) { if (!_gameStartListeners.Contains(listener)) _gameStartListeners.Add(listener); }
    public void RegisterListener(IGameEndListener listener) { if (!_gameEndListeners.Contains(listener)) _gameEndListeners.Add(listener); }
    public void UnregisterListener(IGameReadyListener listener) { _gameReadyListeners.Remove(listener); }
    public void UnregisterListener(IGameStartListener listener) { _gameStartListeners.Remove(listener); }
    public void UnregisterListener(IGameEndListener listener) { _gameEndListeners.Remove(listener); }
    #endregion

    #region State Change Logic
    
    public void ChangeState(GameState newState)
    {
        if (!Object.HasStateAuthority) return;
        
        // [핵심 수정] End/Clear 상태에서도 Ready 상태로 복귀는 허용
        if (CurrentState == newState) return;
        if ((CurrentState == GameState.End || CurrentState == GameState.Clear) && newState != GameState.Ready)
        {
            Debug.LogWarning($"[GameStateManager] 게임이 종료되어 '{newState}'로 변경할 수 없습니다. 'Ready'로만 복귀 가능합니다.");
            return;
        }

        Debug.Log($"[GameStateManager] Changing state: {CurrentState} → {newState}");
        CurrentState = newState;
        HandleStateChange(newState); 
    }
    // [신규] GameResultUI의 "다시시작" 버튼이 호출할 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReturnToReady()
    {
        // Host만 상태를 Ready로 되돌림
        if (!Object.HasStateAuthority) return;

        Debug.Log("[GameStateManager] Host가 Ready 상태(스테이지 선택)로 복귀를 요청받았습니다.");
        ChangeState(GameState.Ready);
    }
    private void HandleStateChange(GameState state)
    {
        // [신규] 상태가 변경될 때마다 캔버스를 먼저 숨겨서 중복 표시 방지
        if (m_end_canvas != null) m_end_canvas.SetActive(false);
        if (m_clear_canvas != null) m_clear_canvas.SetActive(false);
        // (StageSelectUI는 OnGameReady에서 스스로 켬)
        switch (state)
        {
            case GameState.Loading:
                break;
            case GameState.Role:
                if (Object.HasStateAuthority)
                    ChangeState(GameState.Ready);
                break;

            case GameState.Ready:
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                }
                foreach (var listener in _gameReadyListeners)
                    listener.OnGameReady();
                Debug.Log("[GameStateManager] 'Ready' 상태 진입. 호스트의 스테이지 선택 대기 중.");
                
                // [수정] Ready 상태에서 타이머 시작 로직 제거
                // 이제 Host의 선택을 무기한 기다립니다.
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners)
                    listener.OnGameStart();
                Debug.Log($"[GameStateManager] 'Start' 상태 진입 (스테이지 {SelectedStageIndex}). 30초 카운트다운 시작.");

                // Host가 30초 타이머 시작 (이전과 동일)
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_START_TO_PLAY);
                    SharedGameTimer = DURATION_START_TO_PLAY;
                }
                break;

            case GameState.Play:
                if (spawner != null)
                    spawner.StartWave();
                Debug.Log("[GameStateManager] Play 상태로 전환됨 - 적 웨이브 시작");

                // (기존 로직) Host가 5분 타이머 시작
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_PLAY_TO_END);
                    SharedGameTimer = DURATION_PLAY_TO_END;

                    // [신규] 몬스터 스포너의 StageData에서 스테이지 체력 초기화
                    if (spawner != null && spawner.currentStageData != null)
                    {
                        CurrentStageHealth = spawner.currentStageData.StageHealth;
                    }
                    else
                    {
                        CurrentStageHealth = 1; // 비상용 체력
                        Debug.LogError("[GameStateManager] MonsterSpawner 또는 currentStageData가 null입니다. 비상 체력 1로 시작.");
                    }
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
                
            case GameState.Clear: // [신규] 게임 클리어
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

    public void OnGameReady()
    {
        throw new NotImplementedException();
    }

    public void OnGameStart()
    {
        throw new NotImplementedException();
    }

    public void OnGameEnd()
    {
        throw new NotImplementedException();
    }




    #endregion
}