using UnityEngine;

public class MonsterStatus : MonoBehaviour
{
    public MonsterData data;

    private int currentHealth;

    // 기존 Start()에서 체력 초기화 제거
    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name}에 MonsterData가 할당되지 않았습니다!");
        }
    }

    // 웨이브별로 체력 초기화할 때 호출
    public void InitializeHealth(int health)
    {
        currentHealth = health;
        Debug.Log($"{data.monsterName} 체력 초기화: {currentHealth}");
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{data.monsterName} 체력: {currentHealth}/{data.maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (data.deathEffectPrefab != null)
        {
            Instantiate(data.deathEffectPrefab, transform.position, Quaternion.identity);
        }

        GameManager.Instance.AddGold(1); // 예시로 골드 1 추가
        Destroy(gameObject);
    }
}
