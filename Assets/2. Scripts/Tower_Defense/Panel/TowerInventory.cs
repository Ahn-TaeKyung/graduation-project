using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerInventory : MonoBehaviour
{
    public List<TowerData> towerSlots = new List<TowerData>();
    public TowerData selectedTower;
    public event Action OnInventoryChanged;

    public void AddTower(TowerData tower)
    {
        towerSlots.Add(tower);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveTower(TowerData tower)
    {
        towerSlots.Remove(tower);
        if (selectedTower == tower)
            selectedTower = null;
        OnInventoryChanged?.Invoke();
    }

    public List<TowerData> GetAllTowers()
    {
        return towerSlots;
    }
    
    
}
