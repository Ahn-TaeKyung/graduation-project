using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDragSnap : MonoBehaviour
{
    public static bool IsDragging = false;

    private ClickDragHandler handler;
    int cubeGroupLayerMask;
    int moduleLayerMask;

    [HideInInspector]
    public CubeGroupFaceController groupController;
    private Camera hackerCamera;
    public static bool IsSnapping { get; private set; } = false;
    public static bool IsSnapped => !IsSnapping; // 읽기 전용, 정렬이 끝났으면 true

    void Awake()
    {
        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");
        moduleLayerMask = LayerMask.GetMask("Module");

        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }

        handler = GetComponent<ClickDragHandler>();
        if (handler == null)
            handler = gameObject.AddComponent<ClickDragHandler>();

        handler.OnDrag += OnDragCube;
        handler.OnLeftClick += OnLeftClickCube;
        handler.OnRightClick += OnRightClickCube;
        handler.OnDragEnd += OnDragEndCube;

        // 반드시 참조 연결(직접 인스펙터 할당 또는 Find 등)
        if (groupController == null)
            groupController = GetComponentInParent<CubeGroupFaceController>();
    }

    void OnDragCube(Vector2 mouseDelta)
    {
        if (!groupController.IsCameraFixed) return;
        if (ModuleZoom.IsZoomed) return;
        IsDragging = true;

        float rotSpeed = 0.15f;
        transform.Rotate(hackerCamera.transform.up, -mouseDelta.x * rotSpeed, Space.World);
        transform.Rotate(hackerCamera.transform.right, mouseDelta.y * rotSpeed, Space.World);
    }

    void OnLeftClickCube()
    {
        if (!groupController.IsCameraFixed) return;
        if (ModuleZoom.IsZoomed) return;

        Vector2 clickScreenPos = handler.LastClickPos;
        Ray ray = hackerCamera.ScreenPointToRay(clickScreenPos);

        // CubeGroup + Module 모두 포함 (layerMask | 연산)
        int combinedMask = cubeGroupLayerMask | moduleLayerMask;
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, combinedMask);

        if (hits.Length == 0)
            return;

        // 거리순으로 정렬 (가까운 것부터 판정)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            int objLayer = hit.transform.gameObject.layer;

            // CubeGroup(벽)에 막히면 즉시 중단(뒤는 안 봄)
            if ((cubeGroupLayerMask & (1 << objLayer)) != 0)
            {
                // Debug.Log("큐브 벽에 막힘, 모듈 클릭 없음");
                break;
            }
            // Module에 처음 맞으면 바로 트리거
            if ((moduleLayerMask & (1 << objLayer)) != 0)
            {
                ModuleZoom mz = hit.transform.GetComponent<ModuleZoom>();
                if (mz != null)
                    SnapModuleFaceToCamera(
                        hit.transform,    // 클릭된 모듈 Transform
                        hackerCamera,     // 카메라 참조
                        0.1f,             // 회전 시간
                        () => mz.OnModuleClickTrigger() // 회전 끝나고 확대 트리거
                    );
                break; // 첫 번째 모듈만 처리
            }
        }
        IsDragging = false;
    }


    void OnRightClickCube()
    {
        if (!groupController.IsCameraFixed) return;
        Debug.Log("우클릭 이벤트 발생!");
    }

    void OnDragEndCube()
    {
        if (!groupController.IsCameraFixed) return;
        IsDragging = false;
        SnapToClosestFaceSmooth();
    }

    // 이하 스냅(정렬) 함수 등 동일
    void SnapToClosestFaceSmooth()
    {
        if (IsSnapping) return;
        IsSnapping = true;
        Vector3 toCamera = (hackerCamera.transform.position - transform.position).normalized;
        Vector3[] normals = {
            transform.TransformDirection(Vector3.right),
            transform.TransformDirection(-Vector3.right),
            transform.TransformDirection(Vector3.up),
            transform.TransformDirection(-Vector3.up),
            transform.TransformDirection(Vector3.forward),
            transform.TransformDirection(-Vector3.forward)
        };

        float maxDot = -1f;
        int bestIdx = 0;
        for (int i = 0; i < normals.Length; i++)
        {
            float dot = Vector3.Dot(normals[i], toCamera);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIdx = i;
            }
        }

        Vector3 targetLocalNormal = Vector3.right;
        switch (bestIdx)
        {
            case 0: targetLocalNormal = Vector3.right; break;
            case 1: targetLocalNormal = -Vector3.right; break;
            case 2: targetLocalNormal = Vector3.up; break;
            case 3: targetLocalNormal = -Vector3.up; break;
            case 4: targetLocalNormal = Vector3.forward; break;
            case 5: targetLocalNormal = -Vector3.forward; break;
        }

        Quaternion targetRot = Quaternion.FromToRotation(
            transform.TransformDirection(targetLocalNormal), toCamera
        ) * transform.rotation;

        Vector3 euler = targetRot.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        Quaternion snappedRot = Quaternion.Euler(euler);

        StartCoroutine(SmoothSnapRotation(snappedRot, 0.5f));
    }

    IEnumerator SmoothSnapRotation(Quaternion target, float duration)
    {
        Quaternion startRot = transform.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, target, Mathf.Clamp01(t));
            yield return null;
        }
        transform.rotation = target;
        IsSnapping = false;
    }
    void SnapModuleFaceToCamera(Transform module, Camera cam, float duration, System.Action onComplete = null)
    {
        // 1. 모듈의 현재 월드 기준 정면/위
        Vector3 moduleForwardWorld = module.transform.forward;
        Vector3 moduleUpWorld = module.transform.up;

        // 2. 카메라의 정면/위
        Vector3 cameraForward = cam.transform.forward.normalized;
        Vector3 cameraUp = cam.transform.up.normalized;

        // 3. 목표 회전: 카메라의 forward 반대(-forward), up도 그대로(또는 -up도 가능, 일반적으로 up은 그대로)
        Quaternion targetRotation = Quaternion.LookRotation(-cameraForward, cameraUp);

        // 4. 현재 모듈의 오리엔테이션(월드기준)
        Quaternion moduleCurrentRot = Quaternion.LookRotation(moduleForwardWorld, moduleUpWorld);

        // 5. 큐브를 targetRotation으로 맞추기 위해 필요한 회전값 (상대변환)
        Quaternion delta = targetRotation * Quaternion.Inverse(moduleCurrentRot);

        // 6. 큐브 전체에 적용
        Quaternion finalRot = delta * transform.rotation;

        StartCoroutine(SmoothSnapRotationWithCallback(finalRot, duration, onComplete));
    }

    IEnumerator SmoothSnapRotationWithCallback(Quaternion target, float duration, System.Action onComplete)
    {
        IsSnapping = true;
        Quaternion startRot = transform.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, target, Mathf.Clamp01(t));
            yield return null;
        }
        transform.rotation = target;
        IsSnapping = false;
        onComplete?.Invoke();
    }

}