using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// TurretPlacer: 플레이어 로컬에서 Defend 모드일 때 마우스로 타일 선택, ghost 표시, 설치 요청 전송
/// - 이 스크립트는 PlayerPrefab에 붙여짐
/// </summary>
public class TurretPlacer : NetworkBehaviour
{
    [Header("References")]
    public Camera defendCamera; // 씬의 DefendCamera (씬에 고정)
    public Transform ghostParent;

    [Header("Config")]
    public TurretDefinition[] turretOptions; // 인스펙터에서 등록
    public int selectedIndex = 0; // 선택된 turret 타입

    private GameObject currentGhost;
    private Vector2Int currentCell;
    private bool currentCanPlace = false;

    private void Start()
    {
        // defendCamera는 GameObject.Find로도 찾을 수 있으나, Inspector에 할당하는걸 권장
        if (defendCamera == null)
            defendCamera = GameObject.Find("DefendCamera")?.GetComponent<Camera>();
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        // Defend 모드일때만 동작: 카메라 활성화 여부로 판단하거나 GameState 체크
        if (defendCamera == null) return;
        if (!defendCamera.enabled) // 또는 isDefendView 파라미터로
            return;

        // 마우스 위치 -> cell 계산
        if (!Mouse.current.leftButton.isPressed && Mouse.current.position.IsActuated())
        {
            Vector2 mouse = Mouse.current.position.ReadValue();
            Ray ray = defendCamera.ScreenPointToRay(mouse);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
            {
                Vector3 worldPos = hit.point;
                var cell = GridManager.Instance.WorldToCell(worldPos);
                if (cell != currentCell)
                {
                    currentCell = cell;
                    UpdateGhost();
                }
            }
        }

        // 마우스 클릭 (설치 시도) - 왼클릭
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryRequestPlace(currentCell);
        }

        // Q/E 등으로 turret 선택 변경 (예시)
        if (Keyboard.current.qKey.wasPressedThisFrame)
            SelectPrev();
        if (Keyboard.current.eKey.wasPressedThisFrame)
            SelectNext();
    }

    private void UpdateGhost()
    {
        DestroyCurrentGhost();

        var def = turretOptions[selectedIndex];
        if (def == null) return;

        // world pos center
        Vector3 center = GridManager.Instance.CellToWorldCenter(currentCell);
        currentGhost = Instantiate(def.ghostPrefab, center, Quaternion.identity, ghostParent);
        GhostTurret ghost = currentGhost.GetComponent<GhostTurret>();
        if (ghost != null)
        {
            ghost.SetSize(def.size.x, def.size.y);
            currentCanPlace = GridManager.Instance.IsAreaFree(currentCell, def.size.x, def.size.y);
            ghost.SetValid(currentCanPlace);
        }
    }

    private void DestroyCurrentGhost()
    {
        if (currentGhost != null)
            Destroy(currentGhost);
        currentGhost = null;
    }

    private void TryRequestPlace(Vector2Int cell)
    {
        var def = turretOptions[selectedIndex];
        if (def == null) return;

        if (!GridManager.Instance.IsInsideGrid(cell))
        {
            Debug.Log("[TurretPlacer] cell out of bounds");
            return;
        }

        bool canPlace = GridManager.Instance.IsAreaFree(cell, def.size.x, def.size.y);
        if (!canPlace)
        {
            Debug.Log("[TurretPlacer] 설치 불가");
            return;
        }

        // 네트워크로 설치 요청 전송 (클라이언트 -> Host)
        // 방법: GameStateManager.RequestPlaceTurret(...) 호출 (아래 참고)
        GameStateManager.Instance.RequestPlaceTurretRPC(cell, selectedIndex, Runner.LocalPlayer); // wrapper will handle RPC
    }

    private void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % turretOptions.Length;
        UpdateGhost();
    }

    private void SelectPrev()
    {
        selectedIndex = (selectedIndex - 1 + turretOptions.Length) % turretOptions.Length;
        UpdateGhost();
    }

    private void OnDestroy()
    {
        DestroyCurrentGhost();
    }
}
