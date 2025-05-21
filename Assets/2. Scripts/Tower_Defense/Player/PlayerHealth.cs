using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    public static bool isGameOver = false;  // 게임 오버 상태 플래그

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);   // 최소 0으로 클램프

        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0 && !isGameOver)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버!");
        DestroyAllMonstersAndTowers();
        // 여기서 UI 표시나 BGM 정지 등 추가 작업 가능
    }

    // 게임 오버 시 씬에 있는 모든 몬스터와 타워 삭제
    void DestroyAllMonstersAndTowers()
    {
        // 모든 Monster 오브젝트 삭제
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);    
        }
    }

}
