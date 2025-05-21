using UnityEngine;

public class MonsterStatus : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    // 체력 초기화 함수 추가
    public void InitializeHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} 체력 초기화: {currentHealth}/{maxHealth}");
    }


    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 죽음!");
        GameManager.Instance.AddGold(1); // 골드 획득
        Destroy(gameObject);
    }

}
