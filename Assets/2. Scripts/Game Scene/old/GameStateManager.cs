// 파일명: GameStateManager.cs (Spawned 함수 수정)
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;




public class GameStateManager : NetworkBehaviour, IGameReadyListener, IGameStartListener, IGameEndListener
{
    public static GameStateManager Instance { get; private set; }

    [Header("Core References")]
    public MonsterSpawner spawner;

    [Header("UI Canvases")]
    public GameObject m_stage_select_canvas; 
    public GameObject m_end_canvas;
    public GameObject m_clear_canvas;
    
    // [신규] UI 캔버스의 씬 상의 정확한 이름 (인스펙터 참조 실패 시 대비)
    private const string STAGE_SELECT_CANVAS_NAME = "StageSelectMap_Panel"; // 씬에 있는 UI 캔버스 오브젝트의 이름
    private const string END_CANVAS_NAME = "m_end_canvas"; // 씬에 있는 UI 캔버스 오브젝트의 이름
    private const string CLEAR_CANVAS_NAME = "m_clear_canvas"; // 씬에 있는 UI 캔버스 오브젝트의 이름

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
        Debug.Log($"[GSM] Spawned on {(Object.HasStateAuthority ? "Host" : "Client")}, CurrentState={CurrentState}");

        
        // [핵심 수정 1]
        // _lastSyncedState를 현재 상태(Host로부터 동기화된)와 다른 값으로 초기화합니다.
        // 이렇게 하면 클라이언트의 첫 FixedUpdateNetwork에서 if문이 true가 되어
        // HandleStateChange가 무조건 호출됩니다.
        _lastSyncedState = (GameState)(-1); // 존재하지 않는 값으로 설정

        // [핵심 수정 2] UI 참조가 비어있다면 이름으로 다시 찾습니다. (안전 장치)
        StartCoroutine(EnsureUICanvasReferences());
        HandleStateChange(CurrentState);
    }

    private IEnumerator EnsureUICanvasReferences()
    {
        while (m_stage_select_canvas == null)
        {
            m_stage_select_canvas = GameObject.Find(STAGE_SELECT_CANVAS_NAME);
            if (m_stage_select_canvas == null)
                yield return null;
        }
        while (m_end_canvas == null)
        {
            m_end_canvas = GameObject.Find(END_CANVAS_NAME);
            if (m_end_canvas == null)
                yield return null;
        }
        while (m_clear_canvas == null)
        {
            m_clear_canvas = GameObject.Find(CLEAR_CANVAS_NAME);
            if (m_clear_canvas == null)
                yield return null;
        }
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
                        ChangeState(GameState.Play);
                    else if (CurrentState == GameState.Play)
                        ChangeState(GameState.Clear);
                }
            }
        }
    }

    #region Public RPCs (UI -> Host)
    public void HostSelectStage(int stageIndex, int iconButtonIndex)
    {
        if (!Object.HasStateAuthority) return;
        this.SelectedStageIndex = stageIndex;
        this.CurrentIconIndex = iconButtonIndex; 
        ChangeState(GameState.Start);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReturnToReady()
    {
        if (!Object.HasStateAuthority) return;
        CurrentIconIndex = 0; 
        SelectedStageIndex = -1;
        ChangeState(GameState.Ready);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerTakeDamage(int damage)
    {
        if (!Object.HasStateAuthority || CurrentState != GameState.Play) return;
        CurrentStageHealth -= damage;
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
    private void HandleStateChange(GameState state)
    {
        foreach (var listener in _gameReadyListeners)
            listener.OnGameReady();
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
                if (m_stage_select_canvas != null)
                    m_stage_select_canvas.SetActive(true);
                else
                    Debug.LogError($"[GameStateManager] m_stage_select_canvas 참조가 null이라 켤 수 없습니다!");
                
                foreach (var listener in _gameReadyListeners) listener.OnGameReady();
                Debug.Log("[GameStateManager] 'Ready' 상태 진입. 맵 선택 UI 활성화.");
                break;
            case GameState.Start:
            
                m_stage_select_canvas.SetActive(false);
                foreach (var listener in _gameStartListeners) listener.OnGameStart();
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_START_TO_PLAY);
                    SharedGameTimer = DURATION_START_TO_PLAY;
                    if (spawner != null && spawner.currentStageData != null)
                        CurrentStageHealth = spawner.currentStageData.StageHealth;
                    else
                        CurrentStageHealth = 1; 
                }
                break;
            case GameState.Play:
                m_stage_select_canvas.SetActive(false);
                if (spawner != null) spawner.StartWave();
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.CreateFromSeconds(Runner, DURATION_PLAY_TO_END);
                    SharedGameTimer = DURATION_PLAY_TO_END;
                }
                break;
            case GameState.End: 
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_end_canvas != null) m_end_canvas.SetActive(true); 
                if (Object.HasStateAuthority)
                {
                    _stateTransitionTimer = TickTimer.None;
                    SharedGameTimer = 0;
                    CleanupNetworkObjects();
                }
                break;
            case GameState.Clear: 
                foreach (var listener in _gameEndListeners) listener.OnGameEnd();
                if (spawner != null) spawner.StopWave();
                if (m_clear_canvas != null) m_clear_canvas.SetActive(true); 
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
        if (!Object.HasStateAuthority) return;
        Debug.Log("[GameStateManager] 게임 종료. 모든 몬스터, 타워, 총알을 정리합니다...");
        DespawnAllNetworkObjects<EnemyNetwork>();
        DespawnAllNetworkObjects<Bullet>();
        DespawnAllNetworkObjects<TurretNetwork>(); 
        DespawnAllNetworkObjects<SwordTurretNetwork>();
        DespawnAllNetworkObjects<NetworkedVFXAutoDespawn>();
    }
    private void DespawnAllNetworkObjects<T>() where T : NetworkBehaviour
    {
        T[] objectsToDespawn = FindObjectsByType<T>(FindObjectsSortMode.None);
        Debug.Log($"[GameStateManager] ... {objectsToDespawn.Length}개의 {typeof(T).Name} 오브젝트 제거 중");
        foreach (T obj in objectsToDespawn)
        {
            if (obj != null && obj.Object != null && obj.Object.IsValid)
            {
                Runner.Despawn(obj.Object);
            }
        }
    }
    
    public void OnGameReady() { }
    public void OnGameStart() { }
    public void OnGameEnd() { }
    #endregion
}