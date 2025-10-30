// 파일명: EnemyDefinition.cs
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDef_", menuName = "TowerDefense/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string ID = "BasicMonster";
    public string DisplayName = "기본 몬스터";

    [Header("Stats")]
    public float MaxHealth = 100f;
    public float MoveSpeed = 2f;
    public int DamageToGoal = 1;

    [Header("Prefabs")]
    [Tooltip("실제 스폰될 네트워크 프리팹 (NetworkObject 포함)")]
    public NetworkPrefabRef NetworkPrefab; 
}