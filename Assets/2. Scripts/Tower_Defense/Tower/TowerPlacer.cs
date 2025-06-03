using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    public Camera mainCamera;
    public TowerInventory inventory;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TowerPlacementZone zone = hit.collider.GetComponent<TowerPlacementZone>();
                if (zone != null && !zone.isOccupied)
                {
                    TowerData tower = inventory.selectedTower;
                    if (tower != null)
                    {
                        Instantiate(tower.prefab, zone.transform.position, Quaternion.identity);
                        zone.isOccupied = true;
                        inventory.RemoveTower(tower);
                        inventory.selectedTower = null;
                    }
                }
            }
        }
    }
}
