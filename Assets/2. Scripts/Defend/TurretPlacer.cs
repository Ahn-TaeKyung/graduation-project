// 파일명: TurretPlacer.cs
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.UI; // UI 버튼 직접 참조 제거

public class TurretPlacer : NetworkBehaviour
{
    [Header("Setup (Prefab)")]
    [Tooltip("자신의 자식인 GhostContainer Transform")]
    [SerializeField] private Transform ghostParent;
    [SerializeField] private LayerMask groundLayer;
    
    // [제거됨] 씬 UI 참조 필드
    // [SerializeField] private GameObject placementConfirmPanel;
    // [SerializeField] private Button placeButton;
    // [SerializeField] private Button cancelButton;

    private Camera _defendCamera;
    private GameObject _currentGhost;
    [SerializeField] private GameObject TowerPanel;
    private Renderer _ghostRenderer; // 고스트 색상 변경용
    private TurretDefinition _currentTurretDef;
    private Vector2Int _currentGridPos;
    private bool _canPlace;
    private bool _isPlacing; // 현재 드래그(설치) 중인지 여부

    public static TurretPlacer LocalInstance { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            LocalInstance = this;
            _defendCamera = GameObject.Find("DefendCamera")?.GetComponent<Camera>();
            TowerPanel = GameObject.Find("BuildPanel")?.GetComponent<GameObject>();
            // [제거됨] 버튼 리스너 연결 로직
        }
    }

    private void Update()
    {
        if (!Object.HasInputAuthority || _defendCamera == null || !_defendCamera.enabled)
        {
            if (_isPlacing) CancelPlacement(); // 모드 전환 시 강제 취소
            return;
        }

        if (_isPlacing)
        {
            UpdatePlacement();
        }
    }

    // 1. (UIDragToPlace가 호출) 드래그 시작
    public void StartPlacing(TurretDefinition def)
    {
        if (!Object.HasInputAuthority) return;
        if (_currentGhost != null) Destroy(_currentGhost);
        if (def == null || def.GhostPrefab == null) return;
        
        if (_defendCamera == null || !_defendCamera.enabled)
        {
            Debug.LogWarning("DefendCamera가 활성화되지 않아 설치를 시작할 수 없습니다.");
            return;
        }

        _isPlacing = true;
        _currentTurretDef = def;
        _currentGhost = Instantiate(def.GhostPrefab, ghostParent); 
        _ghostRenderer = _currentGhost.GetComponentInChildren<Renderer>(true);
        if (_ghostRenderer == null)
        {
            Debug.LogError($"[TurretPlacer] GhostPrefab에 Renderer 컴포넌트가 없습니다! {def.DisplayName}");
            // 오류가 났더라도 계속 진행은 가능하게 함 (색상 변경만 포기)
        }
        _currentGhost.SetActive(false);

        // [수정됨] 씬 UI의 싱글톤을 직접 호출
        PlacementConfirmPanel.Instance.HidePanel();
    }

    // 2. (Update에서 호출) 드래그 중
    public void UpdatePlacement()
    {
        if (_currentGhost == null || _defendCamera == null) return;

        Ray ray = _defendCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            if (!_currentGhost.activeSelf) _currentGhost.SetActive(true);

            if (GridManager.Instance.WorldToGrid(hit.point, out _currentGridPos))
            {
                _currentGhost.transform.position = GridManager.Instance.GridToWorld(_currentGridPos);
                _canPlace = GridManager.Instance.IsAreaFree(_currentGridPos, _currentTurretDef.Size);
                if (_ghostRenderer != null)
                    _ghostRenderer.material.color = _canPlace ? Color.green : Color.red;
            }
        }
        else
        {
            if (_currentGhost.activeSelf) _currentGhost.SetActive(false);
            _canPlace = false;
        }
    }

    // 3. (UIDragToPlace가 호출) 드래그 끝 (드롭)
    public void EndPlacing()
    {
        if (!Object.HasInputAuthority || !_isPlacing) return;

        if (_canPlace)
        {
            // [수정됨] 씬 UI의 싱글톤을 직접 호출
            PlacementConfirmPanel.Instance.ShowPanel();
        }
        else
        {
            CancelPlacement();
        }
        _isPlacing = false; // 드래그 종료
    }

    // 4. (PlacementConfirmPanel이 호출) 설치 확정
    public void ConfirmPlacement()
    {
        if (_currentGhost == null || !_canPlace) return;

        TurretManager.Instance.RPC_RequestPlaceTurret(
            _currentTurretDef.ID,
            _currentGridPos,
            Runner.LocalPlayer
        );

        Destroy(_currentGhost);
        _currentGhost = null;
        // 패널 숨기기는 PlacementConfirmPanel이 스스로 처리
    }

    // 5. (PlacementConfirmPanel이 호출) 설치 취소
    public void CancelPlacement()
    {
        if (_currentGhost != null) Destroy(_currentGhost);
        _currentGhost = null;
        
        // [수정됨] 씬 UI의 싱글톤을 직접 호출 (이미 켜져있을 수 있으므로)
        PlacementConfirmPanel.Instance.HidePanel();
        _isPlacing = false;
    }
}