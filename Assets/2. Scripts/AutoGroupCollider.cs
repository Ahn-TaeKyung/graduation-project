using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AutoBoxColliderForGroup : MonoBehaviour
{
    public bool includeInactive = false;
    [Range(0f, 1f)]
    public float marginRatio = 0.1f; // 10% 마진 (기본값 0.1)

    [ContextMenu("Fit BoxCollider To Children")]
    public void FitColliderToChildren()
    {
        // 자식 중 MeshRenderer가 붙은 모든 오브젝트의 bounds로 계산
        var renderers = GetComponentsInChildren<MeshRenderer>(includeInactive);

        if (renderers.Length == 0)
        {
            Debug.LogWarning("자식에 MeshRenderer가 없습니다!");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        // 마진 추가 (각 방향별로)
        Vector3 sizeWithMargin = bounds.size * (1f + marginRatio);

        // 부모의 로컬 좌표계로 변환
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = bounds.size;

        BoxCollider col = GetComponent<BoxCollider>();
        col.center = localCenter;
        col.size = sizeWithMargin;
    }
}
