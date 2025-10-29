using UnityEngine;

/// <summary>
/// GhostTurret: ghost prefab 제어 (크기 조절, 색 바꾸기)
/// - ghostPrefab은 타워 모델의 복사본이며, Material은 투명/컬러 변경이 가능해야 함
/// </summary>
public class GhostTurret : MonoBehaviour
{
    public Renderer[] renderers;

    public void SetSize(int sx, int sy)
    {
        // 내부 모델 크기 조정. 예: x-> sx * cellSize
        float cellSize = GridManager.Instance.cellSize;
        transform.localScale = new Vector3(sx * cellSize, 1f, sy * cellSize);
    }

    public void SetValid(bool ok)
    {
        Color c = ok ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                if (m.HasProperty("_Color")) m.color = c;
            }
        }
    }
}
