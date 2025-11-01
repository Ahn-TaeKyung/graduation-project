// 파일명: WeaponStockUI.cs (Polling 방식 최종본)
using TMPro;
using UnityEngine;

public class WeaponStockUI : MonoBehaviour
{
    [Tooltip("이 UI가 추적할 터렛의 ID (TurretDefinition의 ID와 일치)")]
    [SerializeField] private string turretID = "BowTurret";
    [SerializeField] private TMP_Text countText;

    // 이 UI가 현재 표시하고 있는 재고 값
    private int _currentDisplayedStock = -1; // -1로 초기화하여 첫 프레임에 무조건 업데이트되도록 함

    // OnEnable/OnDisable의 이벤트 구독 로직을 모두 제거합니다.

    // 매 프레임(Render Thread)마다 실행됩니다.
    private void Update()
    {
        // SharedWeaponInventory 인스턴스가 준비되었는지 확인합니다.
        if (SharedWeaponInventory.Instance == null)
        {
            return;
        }

        // 1. 네트워크 인벤토리에서 현재 재고를 가져옵니다.
        int newStock = SharedWeaponInventory.Instance.GetWeaponCount(turretID);

        // 2. 이 UI가 기억하는 재고와 다를 경우에만 텍스트를 업데이트합니다.
        if (_currentDisplayedStock != newStock)
        {
            UpdateText(newStock);
            _currentDisplayedStock = newStock; // 현재 재고를 기억합니다.
        }
    }

    private void UpdateText(int count)
    {
        if (countText != null)
        {
            countText.text = count.ToString();
            countText.color = (count <= 0) ? Color.red : Color.black;
        }
    }
}