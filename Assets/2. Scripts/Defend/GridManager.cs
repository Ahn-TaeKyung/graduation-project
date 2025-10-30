// 파일명: GridManager.cs
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Config")]
    [Tooltip("월드 맵으로 사용할 Ground 오브젝트의 Transform")]
    [SerializeField] private Transform groundTransform;
    [Tooltip("Ground 오브젝트의 X축 Scale과 동일해야 함")]
    [SerializeField] private int gridWidth = 30;
    [Tooltip("Ground 오브젝트의 Z축 Scale과 동일해야 함")]
    [SerializeField] private int gridHeight = 20;
    public float cellSize = 1.0f; // 1셀 = 1유닛

    private Vector3 _origin; // 그리드의 (0,0) 월드 좌표 (좌측 하단)
    private bool[,] _occupiedGrid;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _occupiedGrid = new bool[gridWidth, gridHeight];
        
        // Ground 큐브의 중심과 스케일을 기반으로 좌측 하단 (0,0) 원점 계산
        Vector3 center = groundTransform.position;
        // Ground 큐브의 스케일은 1유닛 큐브 기준이므로 cellSize를 곱해 실제 월드 크기를 구함
        Vector3 worldSize = new Vector3(groundTransform.localScale.x * cellSize, 0, groundTransform.localScale.z * cellSize);
        
        _origin = center - new Vector3(worldSize.x / 2f, 0, worldSize.z / 2f);
        _origin.y = groundTransform.position.y; // Ground의 높이
    }

    // 월드 좌표 -> 그리드 좌표
    public bool WorldToGrid(Vector3 worldPos, out Vector2Int gridPos)
    {
        gridPos = Vector2Int.zero;
        Vector3 localPos = worldPos - _origin;

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.z / cellSize);

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            return false; // 그리드 범위 밖
        }

        gridPos = new Vector2Int(x, y);
        return true;
    }

    // 그리드 좌표 (중심) -> 월드 좌표
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x * cellSize) + (cellSize * 0.5f);
        float z = (gridPos.y * cellSize) + (cellSize * 0.5f);
        // Y는 Ground의 Y 레벨 + 0.1 (바닥 뚫림 방지)
        return _origin + new Vector3(x, 0.1f, z); 
    }

    // 특정 구역이 비어있는지 검사
    public bool IsAreaFree(Vector2Int gridPos, Vector2Int size)
    {
        bool checkPath = PathManager.Instance != null;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = gridPos + new Vector2Int(x, y);

                if (cell.x < 0 || cell.x >= gridWidth || cell.y < 0 || cell.y >= gridHeight)
                    return false; // 맵 밖

                if (_occupiedGrid[cell.x, cell.y])
                    return false; // 이미 점유됨
                
                // TODO: 여기에 "적 경로"인지 검사하는 로직 추가 (PathManager 참조)
                if (checkPath && PathManager.Instance.IsGridCellOnPath(cell))
                {
                    Debug.Log($"설치 불가: 셀 {cell}은 적 이동 경로입니다.");
                    return false; 
                }
            }
        }
        return true;
    }

    // 구역 점유 (Host에서만 호출되어야 함)
    public void OccupyArea(Vector2Int gridPos, Vector2Int size, bool occupy)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = gridPos + new Vector2Int(x, y);
                if (cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight)
                {
                    _occupiedGrid[cell.x, cell.y] = occupy;
                }
            }
        }
    }
}