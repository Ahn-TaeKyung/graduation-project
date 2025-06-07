using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Networked] public GameState CurrentState { get; private set; }

    private readonly List<IGameReadyListener> _gameReadyListeners = new();
    private readonly List<IGameStartListener> _gameStartListeners = new();

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

        if (HasStateAuthority)
        {
            ChangeState(GameState.Loading);
        }
    }

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

    public void UnregisterListener(IGameReadyListener listener)
    {
        _gameReadyListeners.Remove(listener);
    }

    public void UnregisterListener(IGameStartListener listener)
    {
        _gameStartListeners.Remove(listener);
    }

    public void ChangeState(GameState newState)
    {
        if (!HasStateAuthority) return;
        if (CurrentState == newState) return;
        Debug.Log($"상태 변경 {CurrentState} => {newState}");

        CurrentState = newState;
        HandleStateChange(newState);
    }

    void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Loading:
                break;

            case GameState.Role:
                if (GameSceneManager.Instance != null)
                {
                    GameSceneManager.Instance.SetupByRole();
                }
                ChangeState(GameState.Ready);
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
                // ...
                break;

            case GameState.End:
                // ...
                break;
        }
    }
}
