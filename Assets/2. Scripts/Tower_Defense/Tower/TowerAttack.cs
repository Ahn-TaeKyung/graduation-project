// using System.Collections.Generic;
// using UnityEngine;

// public class TowerAttack : MonoBehaviour
// {
//     public float attackCooldown = 1f;
//     public GameObject bulletPrefab;
//     public Transform firePoint;

//     private List<GameObject> enemiesInRange = new List<GameObject>();
//     private float attackTimer = 0f;

//     void Update()
//     {
//         // 죽은 몬스터(null) 참조 제거
//         enemiesInRange.RemoveAll(e => e == null);

//         attackTimer += Time.deltaTime;
//         if (attackTimer >= attackCooldown && enemiesInRange.Count > 0)
//         {
//             Attack(enemiesInRange[0]);
//             attackTimer = 0f;
//         }
//     }

//     void Attack(GameObject enemy)
//     {
//         GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
//         bullet.GetComponent<Bullet>().SetTarget(enemy.transform);
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Monster"))
//             enemiesInRange.Add(other.gameObject);
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Monster"))
//             enemiesInRange.Remove(other.gameObject);
//     }
// }
