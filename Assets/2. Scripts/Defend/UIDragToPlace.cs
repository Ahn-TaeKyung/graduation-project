// 파일명: UIDragToPlace.cs
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
        // 데이터베이스에서 터렛 정보 찾아오기
        _turretDef = TurretDatabase.Instance.GetTurretByID(turretID);
        if (_turretDef == null)
            Debug.LogError($"[UIDragToPlace] {turretID} ID를 가진 터렛을 Database에서 찾을 수 없음");
    }

    // 로컬 플레이어의 TurretPlacer를 찾는 안전한 방법
    private TurretPlacer GetLocalPlacer()
    {
        if (_localPlacer == null)
        {
            _localPlacer = TurretPlacer.LocalInstance;
        }
        return _localPlacer;
    }

    // 마우스로 이 UI를 클릭했을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_turretDef == null) return;
        
        // 로컬 플레이어의 TurretPlacer에게 "설치 시작" 알림
        GetLocalPlacer()?.StartPlacing(_turretDef);
    }

    // 드래그 중일 때 (매 프레임 호출)
    public void OnDrag(PointerEventData eventData)
    {
        // TurretPlacer는 Update에서 스스로 위치를 갱신하므로,
        // 여기서 TurretPlacer.UpdatePlacement()를 호출할 필요가 없음
        // (만약 TurretPlacer.Update가 아닌 이벤트 기반으로 하려면 여기서 호출)
    }

    // 마우스를 뗐을 때 (맵이든 UI든 어디서든)
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // 로컬 플레이어의 TurretPlacer에게 "설치 종료(드롭)" 알림
        GetLocalPlacer()?.EndPlacing();
    }
}