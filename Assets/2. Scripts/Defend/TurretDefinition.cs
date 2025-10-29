using UnityEngine;

[CreateAssetMenu(menuName = "TD/TurretDefinition")]
public class TurretDefinition : ScriptableObject
{
    public string turretName;
    public GameObject turretNetworkPrefab; // NetworkObject 가 포함된 prefab
    public GameObject ghostPrefab; // ghost preview prefab
    public Vector2Int size = new Vector2Int(2, 2); // grid 크기 (x,y)
    public int cost = 10;
    public Sprite icon;
}
