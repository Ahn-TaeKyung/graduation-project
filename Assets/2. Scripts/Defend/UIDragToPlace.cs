// 파일명: UIDragToPlace.cs (수정됨)
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToPlace : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("이 슬롯이 배치할 터렛의 ID (TurretDefinition의 ID와 일치)")]
    [SerializeField] private string turretID = "BowTurret";
    
    private TurretDefinition _turretDef;
    private TurretPlacer _localPlacer;

    private void Start()
    {
        _turretDef = TurretDatabase.Instance.GetTurretByID(turretID);
        if (_turretDef == null)
            Debug.LogError($"[UIDragToPlace] {turretID} ID를 가진 터렛을 Database에서 찾을 수 없음");
    }

    private TurretPlacer GetLocalPlacer()
    {
        if (_localPlacer == null)
        {
            _localPlacer = TurretPlacer.LocalInstance;
        }
        return _localPlacer;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_turretDef == null) return;
        
        // [핵심 수정] 드래그 시작 전 재고 확인
        if (SharedWeaponInventory.Instance != null)
        {
            int stock = SharedWeaponInventory.Instance.GetWeaponCount(turretID);
            if (stock <= 0)
            {
                Debug.Log($"[TurretPlacer] {turretID} 재고 없음! (UI에서 차단)");
                // TODO: 여기에 "재고가 없습니다" UI 안내 문구 표시
                // ShowStockMessage("재고가 없습니다!");
                return; // 재고가 없으면 드래그를 시작하지 않음
            }
        }
        else
        {
            Debug.LogError("SharedWeaponInventory.Instance를 찾을 수 없습니다.");
            return;
        }
        
        // 재고가 있으면 설치 시작
        GetLocalPlacer()?.StartPlacing(_turretDef);
    }

    public void OnDrag(PointerEventData eventData) { } // 로직 없음

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // 드래그 종료 알림
        GetLocalPlacer()?.EndPlacing();
    }
}