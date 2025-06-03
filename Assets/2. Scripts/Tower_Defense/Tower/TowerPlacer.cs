using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    public Camera mainCamera;
    public TowerInventory inventory;
    public LayerMask BuildZone; // 인스펙터에서 BuildZone만 포함되도록 설정

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Raycast에 LayerMask 적용
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, BuildZone))
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
