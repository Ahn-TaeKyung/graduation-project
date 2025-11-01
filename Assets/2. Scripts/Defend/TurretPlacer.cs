// 파일명: TurretPlacer.cs (수정)
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretPlacer : NetworkBehaviour
{
    [Header("Setup (Prefab)")]
    [SerializeField] private Transform ghostParent;
    [SerializeField] private LayerMask groundLayer;
    
    private Camera _defendCamera;
    private GameObject _currentGhost;
    private Renderer _ghostRenderer;
    private TurretDefinition _currentTurretDef;
    private Vector2Int _currentGridPos;
    private bool _canPlace;
    private bool _isPlacing;

    // [신규] 현재 고스트의 공격 범위 비주얼
    private Transform _currentGhostRangeVisual;

    public static TurretPlacer LocalInstance { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            LocalInstance = this;
            _defendCamera = GameObject.Find("DefendCamera")?.GetComponent<Camera>();
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
        if (_defendCamera == null || !_defendCamera.enabled) return;

        // [신규] 그리드 표시
        GridVisualizer.Instance.ShowGrid();

        _isPlacing = true;
        _currentTurretDef = def;
        _currentGhost = Instantiate(def.GhostPrefab, ghostParent); 
        _ghostRenderer = _currentGhost.GetComponentInChildren<Renderer>(true);
        if (_ghostRenderer == null)
            Debug.LogError($"[TurretPlacer] GhostPrefab에 Renderer 컴포넌트가 없습니다! {def.DisplayName}");

        // [신규] 공격 범위 비주얼 찾기 및 설정
        _currentGhostRangeVisual = _currentGhost.transform.Find("RangeVisual");
        if (_currentGhostRangeVisual != null)
        {
            // AttackRange가 반지름(radius)이므로 스케일(지름)은 * 2
            float diameter = def.AttackRange * 2.0f;
            // Y 스케일은 프리팹에 설정된 얇은 값을 유지
            _currentGhostRangeVisual.localScale = new Vector3(diameter, _currentGhostRangeVisual.localScale.y, diameter);
        }
        else
        {
            Debug.LogWarning($"[TurretPlacer] {def.GhostPrefab.name}에 'RangeVisual' 자식이 없습니다.");
        }

        _currentGhost.SetActive(false); // 처음엔 숨김
        PlacementConfirmPanel.Instance.HidePanel();
    }

    // 2. (Update에서 호출) 드래그 중
    public void UpdatePlacement()
    {
        if (_currentGhost == null || _defendCamera == null) return;

        Ray ray = _defendCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            // 고스트와 범위 표시 켜기
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
            // 맵 밖으로 나가면 고스트와 범위 표시 끄기
            if (_currentGhost.activeSelf) _currentGhost.SetActive(false);
            _canPlace = false;
        }
    }

    // 3. (UIDragToPlace가 호출) 드래그 끝 (드롭)
    public void EndPlacing()
    {
        if (!Object.HasInputAuthority || !_isPlacing) return;

        // [신규] 그리드 숨기기 (드롭 시)
        GridVisualizer.Instance.HideGrid();

        if (_canPlace)
        {
            PlacementConfirmPanel.Instance.ShowPanel();
        }
        else
        {
            CancelPlacement(); // CancelPlacement가 고스트를 파괴함
        }
        _isPlacing = false;
    }

    // 4. (PlacementConfirmPanel이 호출) 설치 확정
    public void ConfirmPlacement()
    {
        if (_currentGhost == null || !_canPlace) return;

        // [신규] 그리드 숨기기 (확정 시)
        GridVisualizer.Instance.HideGrid();

        // ... (재고 확인 및 RPC 호출 로직) ...
        int stock = SharedWeaponInventory.Instance.GetWeaponCount(_currentTurretDef.ID);
        if (stock <= 0)
        {
            CancelPlacement();
            return;
        }
        SharedWeaponInventory.Instance.RPC_UseWeapon(_currentTurretDef.ID);
        TurretManager.Instance.RPC_RequestPlaceTurret(
            _currentTurretDef.ID,
            _currentGridPos,
            Runner.LocalPlayer
        );

        Destroy(_currentGhost); // 고스트와 RangeVisual 자식이 함께 파괴됨
        _currentGhost = null;
        _currentGhostRangeVisual = null; // 참조 초기화
    }

    // 5. (PlacementConfirmPanel이 호출) 설치 취소
    public void CancelPlacement()
    {
        // [신규] 그리드 숨기기 (취소 시)
        GridVisualizer.Instance.HideGrid();

        if (_currentGhost != null) Destroy(_currentGhost); // 고스트와 RangeVisual 자식이 함께 파괴됨
        _currentGhost = null;
        _currentGhostRangeVisual = null; // 참조 초기화
        
        PlacementConfirmPanel.Instance.HidePanel(); // (이중 확인)
        _isPlacing = false;
    }
}