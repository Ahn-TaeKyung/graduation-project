using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerInventory : MonoBehaviour
{
    public List<TowerData> towerSlots = new List<TowerData>();

    public Transform slotParent;  // InventoryPanel
    public GameObject slotPrefab; // TowerSlot prefab

    public TowerData selectedTower = null;

    private List<GameObject> currentSlots = new List<GameObject>();

    public void AddTower(TowerData tower)
    {
        towerSlots.Add(tower);
        Debug.Log($"{tower.towerName} 인벤토리에 추가됨");
        RefreshUI();
    }

    public void RemoveTower(TowerData tower)
    {
        if (towerSlots.Contains(tower))
        {
            towerSlots.Remove(tower);
            if (selectedTower == tower)
                selectedTower = null;
            RefreshUI();
        }
    }

    public void SelectTower(TowerData tower)
    {
        if (towerSlots.Contains(tower))
        {
            selectedTower = tower;
            Debug.Log($"{tower.towerName} 선택됨");
            // TODO: 선택 UI 표시
        }
    }

    // UI 슬롯 다시 생성
    public void RefreshUI()
    {
        // 기존 슬롯 제거
        foreach (var slotObj in currentSlots)
        {
            Destroy(slotObj);
        }
        currentSlots.Clear();

        // 슬롯 생성
        foreach (var tower in towerSlots)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            TowerSlot slot = slotObj.GetComponent<TowerSlot>();
            slot.SetData(tower, this);
            currentSlots.Add(slotObj);
        }
    }
}
