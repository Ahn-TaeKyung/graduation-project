using UnityEngine;

public class TowerPlacementZone : MonoBehaviour
{
    public bool isOccupied = false;

    private void OnMouseDown()
    {
        if (isOccupied) return;

        TowerData towerToPlace = GameManager.Instance.inventory.selectedTower;
        if (towerToPlace != null)
        {
            // 타워 설치
            Instantiate(towerToPlace.prefab, transform.position, Quaternion.identity);
            isOccupied = true;

            // 인벤토리에서 선택 타워 제거
            GameManager.Instance.inventory.RemoveTower(towerToPlace);
            GameManager.Instance.inventory.selectedTower = null;
        }
    }
}