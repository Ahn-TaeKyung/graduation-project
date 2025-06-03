using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    public static bool isGameOver = false;

    [Header("UI")]
    public TMP_Text healthText; // 텍스트 UI를 인스펙터에서 연결하세요

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();  // 시작 시 UI 업데이트
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
