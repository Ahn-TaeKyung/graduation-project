using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// PathGrid: 에디터에서 적의 이동 경로(waypoints)를 입력하거나 런타임에 빌드하면서 GridManager에 path cells 등록
/// </summary>
public class PathGrid : MonoBehaviour
{
    public List<Transform> waypoints = new(); // 적이 지나는 지점들 (순서대로)
    public int samplesPerSegment = 10;

    private void Start()
    {
        RegeneratePath();
    }

    public void RegeneratePath()
    {
        var pathCells = new HashSet<Vector2Int>();
        if (waypoints.Count < 2) { GridManager.Instance.SetPathCells(pathCells); return; }

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[i + 1].position;
            for (int s = 0; s <= samplesPerSegment; s++)
            {
                float t = (float)s / samplesPerSegment;
                Vector3 pos = Vector3.Lerp(a, b, t);
                var cell = GridManager.Instance.WorldToCell(pos);
                pathCells.Add(cell);
            }
        }

        GridManager.Instance.SetPathCells(pathCells);
#if UNITY_EDITOR
        Debug.Log($"[PathGrid] Path cells set: {pathCells.Count}");
#endif
    }
}
