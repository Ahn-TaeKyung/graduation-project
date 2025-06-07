using UnityEngine;

public class HackerCounter : MonoBehaviour
{
    public static int CompleteCount = 0;
    public static int strikeCount = 0;

    public static int moduleCount => StartManager.moduleCount;

    public static HackerCounter Instance { get; private set; }

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
            Debug.Log("게임 클리어!");
        }
        if (strikeCount >= 3)
        {
            Debug.Log("게임 오버!");
        }
    }
}