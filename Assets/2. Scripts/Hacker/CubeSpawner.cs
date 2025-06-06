using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [Header("생성할 큐브 프리팹")]
    public GameObject cubePrefab;

    [Header("x, y, z 방향 개수")]
    [Range(1, 100)] public int nx = 3;
    [Range(1, 100)] public int ny = 3;
    [Range(1, 100)] public int nz = 3;

    [Header("큐브 간격(gap)")]
    public float gap = 0.1f;

    [ContextMenu("Generate Cube Group")]
    public void GenerateCubeGroup()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("[CubeGroupGenerator] cubePrefab이 비어있음.");
            return;
        }

        // 기존 자식 삭제
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        float cubeSize = 1f; // 기본 큐브 크기 (프리팹이 1,1,1이라면)
        float spacing = cubeSize + gap;

        float offsetX = (nx - 1) * spacing * 0.5f;
        float offsetY = (ny - 1) * spacing * 0.5f;
        float offsetZ = (nz - 1) * spacing * 0.5f;

        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                for (int z = 0; z < nz; z++)
                {
                    // 표면(외벽)에만 생성
                    if (x == 0 || x == nx - 1 ||
                        y == 0 || y == ny - 1 ||
                        z == 0 || z == nz - 1)
                    {
                        Vector3 pos = new Vector3(x * spacing - offsetX, y * spacing - offsetY, z * spacing - offsetZ);
                        GameObject cube = Instantiate(cubePrefab, transform);
                        cube.transform.localPosition = pos;
                        cube.name = $"Cube_{x}_{y}_{z}";
                    }
                }
            }
        }
        Debug.Log($"[CubeGroupGenerator] {nx} x {ny} x {nz} 표면 큐브 생성 완료");
    }
}