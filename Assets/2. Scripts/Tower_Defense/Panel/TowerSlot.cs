
using UnityEngine;
using UnityEngine.UI;

public class TowerSlot : MonoBehaviour
{
    public Image iconImage;
    public Image highlightImage;  // 선택 강조용
    public UnityEngine.UI.Button button;

    private TowerData towerData;
    private System.Action onClick;

    public void SetData(TowerData tower, System.Action onClicked)
    {
        towerData = tower;
        iconImage.sprite = tower.icon;
        onClick = onClicked;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetSelected(bool selected)
    {
        if (highlightImage != null)
            highlightImage.enabled = selected;
    }

    public TowerData GetTowerData() => towerData;
}
