// 파일명: TurretDefinition.cs
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretDef_", menuName = "TowerDefense/Turret Definition")]
public class TurretDefinition : ScriptableObject
{
    public string ID = "BasicTurret";
    public string DisplayName = "기본 포탑";
    public int Cost = 100;
    public Vector2Int Size = new Vector2Int(1, 1); // 그리드 크기 (1x1)

    [Header("Prefabs")]
    [Tooltip("설치 전 로컬 미리보기 프리팹 (네트워크 없음)")]
    public GameObject GhostPrefab;
    [Tooltip("실제 스폰될 네트워크 프리팹 (NetworkObject 포함)")]
    public NetworkPrefabRef NetworkPrefab; // Fusion 2.x는 NetworkPrefabRef 사용
}