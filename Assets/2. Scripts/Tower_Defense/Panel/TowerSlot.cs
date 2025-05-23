using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSlot : MonoBehaviour
{
    public Image iconImage;      // 자식 아이콘 Image
    public Button button;        // 슬롯 버튼
    private TowerData towerData;
    private TowerInventory inventory;

    public void SetData(TowerData tower, TowerInventory inv)
    {
        towerData = tower;
        inventory = inv;
        iconImage.sprite = tower.icon;

        button.onClick.AddListener(() =>
        {
            inventory.SelectTower(towerData);
        });
    }
}