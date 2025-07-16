using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour, IGameEndListener
{
    public int maxHealth = 10;
    private int currentHealth;
    public static bool isGameOver = false;

    [Header("UI")]
    public TMP_Text healthText; // 텍스트 UI를 인스펙터에서 연결하세요

    void Start()
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
        currentHealth = maxHealth;
        UpdateHealthUI();  // 시작 시 UI 업데이트
    }

    public void OnGameEnd()
    {
        GameOver();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0 && !isGameOver)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        GameStateManager.Instance.ChangeState(GameState.End);
        Debug.Log("게임 오버!");
        DestroyAllMonstersAndTowers();
    }

    void DestroyAllMonstersAndTowers()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);    
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"<color=red>HP: {currentHealth}/{maxHealth}</color>";
    }
}
