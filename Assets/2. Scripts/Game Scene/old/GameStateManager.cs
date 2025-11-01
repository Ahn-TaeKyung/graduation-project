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

    [Networked]
    public GameState CurrentState { get; private set; }

    // --- 타이머 시스템 ---
    // [수정] Ready 타이머 상수를 Start 타이머 상수로 변경
    private const float DURATION_START_TO_PLAY = 30.0f; 
    private const float DURATION_PLAY_TO_END = 300.0f; // 5분 = 300초

    [Networked] private TickTimer _stateTransitionTimer { get; set; }

    // UI가 Polling(수동 확인)할 네트워크 변수
    [Networked]
    public float SharedGameTimer { get; private set; }
    
    // (OnChangedRender 및 UI 이벤트 관련 코드 모두 제거됨)
    
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
                if (Object.HasStateAuthority)
                    ChangeState(GameState.Ready);
                break;

            case GameState.Ready:
                foreach (var listener in _gameReadyListeners)
                    listener.OnGameReady();
                Debug.Log("[GameStateManager] 모든 IGameReadyListener 초기화 완료");
                
                // [수정] Ready 상태에서 타이머 시작 로직 제거
                // (Ready -> Start는 당신이 별도 로직으로 처리)
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners)
                    listener.OnGameStart();
                Debug.Log("[GameStateManager] 모든 IGameStartListener 초기화 완료");

                // [신규] Host가 'Start' 상태에서 30초 타이머 시작
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_START_TO_PLAY);
                    SharedGameTimer = DURATION_START_TO_PLAY; // UI 즉시 업데이트
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
                }
                break;

            case GameState.End:
                foreach (var listener in _gameEndListeners)
                    listener.OnGameEnd();
                if (spawner != null)
                    spawner.StopWave();
                if (m_end_canvas != null)
                    m_end_canvas.SetActive(true);
                Debug.Log("[GameStateManager] End 상태 진입 - 게임 종료 처리 완료");

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