using UnityEngine;

public class HackerCounter : MonoBehaviour, IGameEndListener
{
    public static int CompleteCount = 0;
    public static int strikeCount = 0;

    public static int moduleCount => StartManager.moduleCount;

    public static HackerCounter Instance { get; private set; }
    private void Start()
    {
        // GameStateManager에 자신을 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[MonsterSpawner] GameStateManager 인스턴스가 없습니다.");
        }
    }
    public void OnGameEnd()
    {
        CheckGameState();
    }
    void Awake()
    {
        Instance = this;
    }

    // 모듈 클리어/실패 시마다 이 함수를 반드시 호출하게 한다
    public void AddComplete()
    {
        CompleteCount++;
        CheckGameState();
    }

    public void AddStrike()
    {
        strikeCount++;
        CheckGameState();
    }

    private void CheckGameState()
    {
        if (CompleteCount >= moduleCount)
        {
            GameStateManager.Instance.ChangeState(GameState.End);
            Debug.Log("게임 클리어!");
        }
        else if (strikeCount >= 3)
        {
            GameStateManager.Instance.ChangeState(GameState.End);
            Debug.Log("해킹 실패!");
        }
        else
        {
            Debug.Log("팀원이 버티질 못했습니다...");
        }
    }
}