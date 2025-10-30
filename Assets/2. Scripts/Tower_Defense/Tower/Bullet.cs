// using UnityEngine;

// public class Bullet : MonoBehaviour
// {
//     public float speed = 10f;
//     public float lifeTime = 3f;
//     private Transform target;

//     public void SetTarget(Transform targetTransform)
//     {
//         target = targetTransform;
//     }

//     void Start()
//     {
//         Destroy(gameObject, lifeTime);
//     }

//     void Update()
//     {
//         if (target == null)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         // 총알이 목표를 바라보도록 회전
//         Vector3 direction = (target.position - transform.position).normalized;
//         transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
//         // 총알이 목표를 향해 이동
//         transform.position += direction * speed * Time.deltaTime;
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Monster"))
//         {
//             MonsterStatus monster = other.GetComponent<MonsterStatus>();
//             if (monster != null)
//             {
//                 monster.TakeDamage(1);
//             }
//             // 몬스터 맞으면 총알 파괴
//             Destroy(gameObject);
//         }
//     }
// }
