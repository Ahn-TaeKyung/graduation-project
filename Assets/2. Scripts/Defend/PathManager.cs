// 파일명: PathManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    [Header("Waypoints")]
    [Tooltip("적들이 이동할 웨이포인트 목록")]
    public Transform[] Waypoints;

    // [추가] 경로를 점유하는 그리드 셀 목록 (Host에서 한번만 계산)
    private readonly HashSet<Vector2Int> _pathOccupiedCells = new();
    
    // [설정] 경로의 너비 (그리드 셀 단위)
    [Tooltip("적이 지나갈 경로의 너비 (그리드 셀 개수)")]
    [SerializeField] private int pathWidthInCells = 2; // 예: 2x2 셀 너비

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Hierarchy의 "PathWaypoints" 오브젝트의 자식들을 자동으로 등록
        if (Waypoints == null || Waypoints.Length == 0)
        {
            var waypointParent = GameObject.Find("PathWaypoints")?.transform;
            if (waypointParent != null)
            {
                Waypoints = new Transform[waypointParent.childCount];
                for (int i = 0; i < waypointParent.childCount; i++)
                {
                    Waypoints[i] = waypointParent.GetChild(i);
                }
            }
        }
    }

    private void Start()
    {
        // Start 시점에 GridManager가 초기화되었다고 가정하고 경로 셀을 계산
        CalculatePathOccupiedCells();
    }

    // [추가된 기능] 웨이포인트를 기반으로 경로가 점유하는 그리드 셀을 계산합니다.
    private void CalculatePathOccupiedCells()
    {
        if (GridManager.Instance == null || Waypoints == null || Waypoints.Length < 2)
        {
            Debug.LogWarning("[PathManager] GridManager 또는 Waypoints 설정이 불완전합니다.");
            return;
        }

        _pathOccupiedCells.Clear();

        // 모든 웨이포인트 쌍 사이의 경로를 순회합니다.
        for (int i = 0; i < Waypoints.Length - 1; i++)
        {
            Vector3 startPos = Waypoints[i].position;
            Vector3 endPos = Waypoints[i + 1].position;
            
            // 웨이포인트 경로 사이를 촘촘하게 검사하기 위해 Raycast 또는 Linecast 사용을 시뮬레이션
            // 여기서는 단순화하여 웨이포인트 셀들 사이의 모든 셀을 포함합니다.

            if (GridManager.Instance.WorldToGrid(startPos, out Vector2Int startGridPos) &&
                GridManager.Instance.WorldToGrid(endPos, out Vector2Int endGridPos))
            {
                // 경로 상의 모든 셀을 계산 (간단한 Bresenham's line algorithm 대신 선형 보간 사용)
                for (float t = 0; t <= 1.0f; t += 0.05f) // 5% 간격으로 검사
                {
                    Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
                    if (GridManager.Instance.WorldToGrid(currentPos, out Vector2Int pathCell))
                    {
                        // 경로 셀 주변 (pathWidthInCells) 만큼을 점유 영역으로 간주
                        for (int x = -pathWidthInCells / 2; x <= pathWidthInCells / 2; x++)
                        {
                            for (int y = -pathWidthInCells / 2; y <= pathWidthInCells / 2; y++)
                            {
                                Vector2Int occupiedCell = new Vector2Int(pathCell.x + x, pathCell.y + y);
                                _pathOccupiedCells.Add(occupiedCell);
                            }
                        }
                    }
                }
            }
        }
        Debug.Log($"[PathManager] 경로에 의해 점유된 그리드 셀: {_pathOccupiedCells.Count}개");
    }

    // [외부 호출용] 특정 그리드 좌표가 경로에 포함되는지 확인
    public bool IsGridCellOnPath(Vector2Int gridPos)
    {
        return _pathOccupiedCells.Contains(gridPos);
    }
}