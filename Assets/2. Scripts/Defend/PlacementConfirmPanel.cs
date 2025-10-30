// 파일명: PlacementConfirmPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class PlacementConfirmPanel : MonoBehaviour
{
    // [중요] 씬이 로드될 때 무조건 등록되어야 하므로 싱글톤 패턴 유지
    public static PlacementConfirmPanel Instance { get; private set; }

    [Header("Scene UI References")]
    [SerializeField] private Button placeButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 이미 인스턴스가 있으면 파괴 (안전 장치)
            Destroy(gameObject); 
            return;
        }

        // 버튼 클릭 시 로컬 플레이어의 TurretPlacer 함수를 호출하도록 연결
        if (placeButton != null) placeButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);

        // [핵심 해결] Awake가 끝난 후, 패널을 숨김으로써 평소에는 비활성화 상태를 유지합니다.
        // TurretPlacer가 StartPlacing()을 호출할 때 이미 Instance는 등록되어 있으므로 안전합니다.
        HidePanel(); 
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // "설치" 버튼 클릭 시
    private void OnConfirm()
    {
        if (TurretPlacer.LocalInstance != null)
        {
            TurretPlacer.LocalInstance.ConfirmPlacement();
        }
        HidePanel();
    }

    // "취소" 버튼 클릭 시
    public void OnCancel() // public으로 변경 (다른 곳에서 직접 호출 가능하도록)
    {
        if (TurretPlacer.LocalInstance != null)
        {
            TurretPlacer.LocalInstance.CancelPlacement();
        }
        HidePanel();
    }
}