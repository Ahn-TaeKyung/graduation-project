using UnityEngine;

/// <summary>
/// GridManager: world <-> cell 변환, cell 점유/해제, 범위 검사 제공
/// 사용: GridManager.Instance.IsAreaFree(cellOrigin, sizeX,sizeY)
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public Vector2 originWorld = Vector2.zero; // world 좌표 원점
    public int width = 50;
    public int height = 30;
    public float cellSize = 1f; // 한 칸의 world 단위 크기

    private bool[,] occupied;
    private bool[,] pathMask; // 적의 길(설치 금지 영역)

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        occupied = new bool[width, height];
        pathMask = new bool[width, height];
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        var local = new Vector2(worldPos.x - originWorld.x, worldPos.z - originWorld.y);
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = originWorld.x + (cell.x + 0.5f) * cellSize;
        float z = originWorld.y + (cell.y + 0.5f) * cellSize;
        return new Vector3(x, 0f, z);
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    // 체크: 주어진 origin cell, sizeX x sizeY 공간이 모두 비어있고 path에 포함되지 않았는가
    public bool IsAreaFree(Vector2Int originCell, int sizeX, int sizeY)
    {
        for (int cx = 0; cx < sizeX; cx++)
            for (int cy = 0; cy < sizeY; cy++)
            {
                var c = new Vector2Int(originCell.x + cx, originCell.y + cy);
                if (!IsInsideGrid(c)) return false;
                if (occupied[c.x, c.y]) return false;
                if (pathMask[c.x, c.y]) return false;
            }
        return true;
    }

    // 마킹: 설치 시점에 점유 처리
    public void SetAreaOccupied(Vector2Int originCell, int sizeX, int sizeY, bool occupy)
    {
        for (int cx = 0; cx < sizeX; cx++)
            for (int cy = 0; cy < sizeY; cy++)
            {
                var c = new Vector2Int(originCell.x + cx, originCell.y + cy);
                if (IsInsideGrid(c))
                    occupied[c.x, c.y] = occupy;
            }
    }

    // 외부에서 Path(적의 길)를 세팅하는 함수 (PathGrid 스크립트가 호출)
    public void SetPathCells(System.Collections.Generic.IEnumerable<Vector2Int> pathCells)
    {
        // 모두 false 초기화 후 적용 (단순 처리)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                pathMask[x, y] = false;

        foreach (var c in pathCells)
            if (IsInsideGrid(c))
                pathMask[c.x, c.y] = true;
    }
}
