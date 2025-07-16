using UnityEngine;
using System.Collections;

public class CubeDragSnap : MonoBehaviour, IGameReadyListener
{
    public static bool IsDragging = false;
    public static bool IsSnapping { get; private set; } = false;
    public static bool IsSnapped => !IsSnapping;

    private ClickDragHandler handler;
    private Camera hackerCamera;
    private int cubeGroupLayerMask;
    private int moduleLayerMask;
    public CubeGroupFaceController groupController;
    private GameSceneManager GameSceneManager;

    private bool isInitialized = false;

    void Start()
    {
        handler = GetComponent<ClickDragHandler>() ?? gameObject.AddComponent<ClickDragHandler>();
        handler.OnDrag += OnDragCube;
        handler.OnLeftClick += OnLeftClickCube;
        handler.OnRightClick += OnRightClickCube;
        handler.OnDragEnd += OnDragEndCube;

        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");
        moduleLayerMask = LayerMask.GetMask("Module");

        // GameStateManager가 준비되었을 때 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[CubeDragSnap] GameStateManager 인스턴스가 없음.");
        }
    }

    public void OnGameReady()
    {
        GameSceneManager = FindFirstObjectByType<GameSceneManager>();
        RoleType myRole = GameSceneManager.GetMyRole();
        if (myRole != RoleType.Hacker)
        {
            return; // 내 역할이 아니라면 아무 것도 안 함
        }
        GameObject hackerCam = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCam != null)
        {
            hackerCamera = hackerCam.GetComponent<Camera>();
            isInitialized = true;
        }
        else
        {
            Debug.LogError("[CubeDragSnap] 해커 카메라를 찾을 수 없습니다.");
        }
    }

    // 이하 기존 코드 유지
    void OnDragCube(Vector2 mouseDelta)
    {
        if (!isInitialized || !groupController.IsCameraFixed) return;
        if (ModuleZoom.IsZoomed) return;

        IsDragging = true;
        float rotSpeed = 0.15f;
        transform.Rotate(hackerCamera.transform.up, -mouseDelta.x * rotSpeed, Space.World);
        transform.Rotate(hackerCamera.transform.right, mouseDelta.y * rotSpeed, Space.World);
    }

    void OnLeftClickCube()
    {
        if (!isInitialized || !groupController.IsCameraFixed || ModuleZoom.IsZoomed) return;

        Ray ray = hackerCamera.ScreenPointToRay(handler.LastClickPos);
        int combinedMask = cubeGroupLayerMask | moduleLayerMask;
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, combinedMask);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            int objLayer = hit.transform.gameObject.layer;

            if ((cubeGroupLayerMask & (1 << objLayer)) != 0)
                break;

            if ((moduleLayerMask & (1 << objLayer)) != 0)
            {
                var mz = hit.transform.GetComponent<ModuleZoom>();
                if (mz != null)
                    SnapModuleFaceToCamera(hit.transform, hackerCamera, 0.1f, () => mz.OnModuleClickTrigger());
                break;
            }
        }

        IsDragging = false;
    }

    void OnRightClickCube() => Debug.Log("우클릭");

    void OnDragEndCube()
    {
        if (!isInitialized || !groupController.IsCameraFixed) return;
        IsDragging = false;
        SnapToClosestFaceSmooth();
    }
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
