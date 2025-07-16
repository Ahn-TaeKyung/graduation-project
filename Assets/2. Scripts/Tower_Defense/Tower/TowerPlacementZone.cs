using UnityEngine;

public class TowerPlacementZone : MonoBehaviour
{
    public bool isOccupied = false;

    public void TryPlaceTower()
    {
        if (isOccupied) return;

        TowerData towerToPlace = GameManager.Instance.inventory.selectedTower;
        if (towerToPlace != null)
        {
            Instantiate(towerToPlace.prefab, transform.position, Quaternion.identity);
            isOccupied = true;

            GameManager.Instance.inventory.RemoveTower(towerToPlace);
            GameManager.Instance.inventory.selectedTower = null;
        }
    }
}
