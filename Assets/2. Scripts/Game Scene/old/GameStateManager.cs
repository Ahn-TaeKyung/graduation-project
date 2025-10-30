using Fusion;
using UnityEngine;
using System.Collections.Generic;
// using Unity.VisualScripting; // 불필요시 제거

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("References")]
    public MonsterSpawner spawner; // MonsterSpawner.cs가 필요합니다.
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
    // [중요] 이 함수는 Host(StateAuthority)만 호출해야 합니다.
    public void ChangeState(GameState newState)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("[GameStateManager] Only the host can change the game state!");
            return;
        }

        if (CurrentState == newState) return;

        Debug.Log($"[GameStateManager] Changing state: {CurrentState} → {newState}");
        CurrentState = newState; // 네트워크 변수 변경 (클라이언트에 전파됨)

        // [중요] Host에서 즉시 HandleStateChange를 호출 (리스너 등록 문제 해결)
        HandleStateChange(newState);
    }

    private void HandleStateChange(GameState state)
    {
        // 이 함수는 FixedUpdateNetwork에 의해 Host와 Client 모두에서 호출됩니다.
        switch (state)
        {
            case GameState.Loading:
                break;
            case GameState.Role:
                // if (GameSceneManager.Instance != null)
                // {
                //     GameSceneManager.Instance.SetupById();
                // }
                ChangeState(GameState.Ready); // Host만 이 코드를 실행
                break;

            case GameState.Ready:
                foreach (var listener in _gameReadyListeners)
                {
                    listener.OnGameReady();
                }
                Debug.Log("[GameStateManager] 모든 IGameReadyListener 초기화 완료");
                break;

            case GameState.Start:
                foreach (var listener in _gameStartListeners)
                {
                    listener.OnGameStart();
                }
                Debug.Log("[GameStateManager] 모든 IGameStartListener 초기화 완료");
                break;

            case GameState.Play:
                if (spawner != null)
                    spawner.StartWave(); // Host/Client 모두에서 호출되므로 Spawner 내부에서 권한 확인 필요
                Debug.Log("[GameStateManager] Play 상태로 전환됨 - 적 웨이브 시작");
                break;

            case GameState.End:
                foreach (var listener in _gameEndListeners)
                {
                    listener.OnGameEnd();
                }
                if (spawner != null)
                    spawner.StopWave();
                // if (GameManager.Instance != null)
                //     GameManager.Instance.PauseGameTime();
                if (m_end_canvas != null)
                    m_end_canvas.SetActive(true);
                Debug.Log("[GameStateManager] End 상태 진입 - 게임 종료 처리 완료");
                break;
        }
    }
    #endregion
}