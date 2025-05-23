using UnityEngine;

public class Goal : MonoBehaviour
{
    private PlayerHealth playerHealth; // PlayerHealth 스크립트를 연결할 변수

    void Start()
    {
        // "Player" 태그를 가진 오브젝트에서 PlayerHealth 스크립트를 찾음
        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster")) // 몬스터가 목표에 도달했을 때
        {
            playerHealth.TakeDamage(1); // 플레이어 체력 감소
            Destroy(other.gameObject); // 몬스터 제거
        }
    }
}
