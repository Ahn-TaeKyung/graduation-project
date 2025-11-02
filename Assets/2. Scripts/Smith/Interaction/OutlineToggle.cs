using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OutlineToggle : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial;   // 단색 Emission 강한 머티리얼
    [SerializeField] [Range(1.0f, 1.2f)] private float scale = 1.03f;
    [SerializeField] private bool buildAtRuntime = true;
    [SerializeField] private float yOffset = 0.01f;      // 테이블 같은 거 살짝 띄우기

    private readonly List<GameObject> clones = new();
    private bool built;

    private void Awake()
    {
        if (buildAtRuntime) BuildClones();
        SetHighlighted(false);
    }

    public void BuildClones()
    {
        if (built) return;
        built = true;

        if (!outlineMaterial)
        {
            Debug.LogWarning($"{name}: Outline material not assigned.");
            return;
        }

        foreach (var mr in GetComponentsInChildren<MeshRenderer>(true))
            CreateCloneForRenderer(mr, mr.GetComponent<MeshFilter>()?.sharedMesh);

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            CreateCloneForRenderer(smr, smr.sharedMesh);
    }

    private void CreateCloneForRenderer(Renderer sourceRenderer, Mesh mesh)
    {
        if (!mesh) return;

        var srcTf = sourceRenderer.transform;

        var go = new GameObject($"{sourceRenderer.gameObject.name}_Outline");
        go.transform.SetParent(srcTf, false);
        go.transform.localPosition = new Vector3(0f, yOffset * 2f, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * scale;

        // 원본이 머티리얼 몇 개 쓰는지 확인
        var srcMats = sourceRenderer.sharedMaterials;
        int matCount = srcMats != null ? srcMats.Length : 1;

        if (sourceRenderer is SkinnedMeshRenderer smrSrc)
        {
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = smrSrc.sharedMesh;
            smr.rootBone = smrSrc.rootBone;
            smr.bones = smrSrc.bones;
            smr.updateWhenOffscreen = true;

            // 서브메시 개수만큼 전부 아웃라인 머티리얼로 채움
            var mats = new Material[matCount];
            for (int i = 0; i < matCount; i++)
                mats[i] = outlineMaterial;
            smr.sharedMaterials = mats;

            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            smr.receiveShadows = false;
            smr.allowOcclusionWhenDynamic = false;
        }
        else
        {
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();

            // 원본이 2개면 2개 다 채우기
            var mats = new Material[matCount];
            for (int i = 0; i < matCount; i++)
                mats[i] = outlineMaterial;
            mr.sharedMaterials = mats;

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.allowOcclusionWhenDynamic = false;
        }

        // 루트 레이어로 통일
        go.layer = gameObject.layer;
        go.SetActive(false);
        clones.Add(go);
    }

    public void SetHighlighted(bool on)
    {
        if (!built && !buildAtRuntime) BuildClones();

        foreach (var c in clones)
            if (c) c.SetActive(on);
    }
}
