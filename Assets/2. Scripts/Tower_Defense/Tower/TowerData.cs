using UnityEngine;

public enum TowerRace { Machine, Goblin, Human, Animal }

[CreateAssetMenu(fileName = "New TowerData", menuName = "Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public GameObject prefab;
    public int cost = 10;
    public Sprite icon;

    public TowerRace race;  // 추가: 종족 정보
    public int level = 1;   // 추가: 타워 레벨 (기본 1)
}
