using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OutlineToggle : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial;   // 단색 Emission 강한 머티리얼
    [SerializeField] [Range(1.0f, 1.2f)] private float scale = 1.03f;
    [SerializeField] private bool buildAtRuntime = true;

    private readonly List<GameObject> clones = new();
    private bool built;

    private void Awake()
    {
        if (buildAtRuntime) BuildClones();
        SetHighlighted(false);
    }

    /// 프리팹에 미리 만들어두지 않았다면 런타임에 원본 MeshRenderer들을 복제해 '외곽' 전용 렌더러 생성
    public void BuildClones()
    {
        if (built) return;
        built = true;

        if (!outlineMaterial)
        {
            Debug.LogWarning($"{name}: Outline material not assigned.");
            return;
        }

        // 원본의 모든 MeshRenderer/SkinnedMeshRenderer를 찾아 복제
        foreach (var mr in GetComponentsInChildren<MeshRenderer>(true))
            CreateCloneForRenderer(mr, mr.GetComponent<MeshFilter>()?.sharedMesh);

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            CreateCloneForRenderer(smr, (smr as SkinnedMeshRenderer).sharedMesh);
    }

    private void CreateCloneForRenderer(Renderer sourceRenderer, Mesh mesh)
    {
        if (!mesh) return;

        var srcTf = sourceRenderer.transform;

        var go = new GameObject($"{sourceRenderer.gameObject.name}_Outline");
        go.transform.SetParent(srcTf, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * scale;

        // 원본 종류에 따라 적절한 렌더러를 붙인다
        if (sourceRenderer is SkinnedMeshRenderer smrSrc)
        {
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = smrSrc.sharedMesh;
            smr.rootBone = smrSrc.rootBone;
            smr.bones = smrSrc.bones;
            smr.updateWhenOffscreen = true;
            smr.sharedMaterial = outlineMaterial;
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            smr.receiveShadows = false;
            smr.allowOcclusionWhenDynamic = false;
        }
        else
        {
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = outlineMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.allowOcclusionWhenDynamic = false;
        }

        go.layer = sourceRenderer.gameObject.layer; // 레이어 유지
        go.SetActive(false);
        clones.Add(go);
    }

    public void SetHighlighted(bool on)
    {
        // 복제물이 아직 없고, 런타임 생성이 꺼져있으면 시도
        if (!built && !buildAtRuntime) BuildClones();

        foreach (var c in clones)
            if (c) c.SetActive(on);
    }
}
