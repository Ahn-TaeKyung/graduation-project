using System.Collections.Generic;
using UnityEngine;

public class TowerInventoryUI : MonoBehaviour
{
    public TowerInventory inventory;
    public GameObject slotPrefab;
    public Transform slotParent;

    private List<TowerSlot> slots = new();
    private TowerSlot selectedSlot;

    void Start()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
        else
        {
            Debug.LogError("TowerInventoryUI: inventory is not assigned!");
        }
    }

    void RefreshUI()
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);
        slots.Clear();
        selectedSlot = null;

        foreach (TowerData tower in inventory.GetAllTowers())
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            TowerSlot slot = obj.GetComponent<TowerSlot>();
            slot.SetData(tower, () => OnSlotClicked(slot));
            slots.Add(slot);
        }
    }

    void OnSlotClicked(TowerSlot clickedSlot)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = clickedSlot;
        selectedSlot.SetSelected(true);

        inventory.selectedTower = clickedSlot.GetTowerData();
    }
}
