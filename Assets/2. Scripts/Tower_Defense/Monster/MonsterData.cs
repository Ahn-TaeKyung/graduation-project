using UnityEngine;


[CreateAssetMenu(fileName = "MonsterData", menuName = "TowerDefense/MonsterData")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int maxHealth;
    public float moveSpeed;
    public GameObject deathEffectPrefab;
    // 필요시 공격력, 방어력, 보상 골드 등 추가 가능
}
