using UnityEngine;

public class ModuleZoom : MonoBehaviour
{
    public CameraMover cameraMover;
    public CubeGroupFaceController cubeGroupController;
    public CubeDragSnap cubedargsnap;
    public Vector3 zoomOffset = new(0, 0, 2);
    public float zoomDuration = 0.5f;
    public static bool IsZoomed = false;
    public bool c_zoomed; //자식용 static안붙은 변수

    private ClickDragHandler handler;

    void Awake()
    {
        handler = GetComponent<ClickDragHandler>();
        if (handler == null)
            handler = gameObject.AddComponent<ClickDragHandler>();

        // 우클릭 복귀 이벤트 바인딩
        handler.OnRightClick += OnRightClickRestore;
    }

    public void OnModuleClickTrigger()
    {
        if (cameraMover == null)
            cameraMover = FindFirstObjectByType<CameraMover>();
        if (cubeGroupController == null)
            cubeGroupController = FindFirstObjectByType<CubeGroupFaceController>();
        if (cubedargsnap == null)
            cubedargsnap = FindFirstObjectByType<CubeDragSnap>();

        if (cubeGroupController == null || !cubeGroupController.IsCameraFixed)
            return;
        if (CubeDragSnap.IsDragging)
            return;
        if (!CubeDragSnap.IsSnapped)
            return;
        if (cameraMover != null && cameraMover.IsMoving)
            return;

        Vector3 worldForward = transform.TransformDirection(Vector3.forward);
        Vector3 zoomTarget = transform.position + worldForward * zoomOffset.z;
        Quaternion zoomRot = Quaternion.LookRotation(transform.position - zoomTarget, transform.TransformDirection(Vector3.up));
        cameraMover.MoveTo(zoomTarget, zoomRot, zoomDuration);
        IsZoomed = true;
        c_zoomed = true;
    }

    // 우클릭 시 복귀 동작(ClickDragHandler에서만 감지)
    void OnRightClickRestore()
    {
        if (IsZoomed && cubeGroupController != null)
        {
            cubeGroupController.MoveCameraBackToFace();
            IsZoomed = false;
            c_zoomed = false;
        }
    }
}