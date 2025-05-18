using UnityEngine;

public class ModuleZoom : MonoBehaviour
{
    public CameraMover cameraMover;
    public CubeGroupFaceController cubeGroupController;
    public Vector3 zoomOffset = new Vector3(0, 0, 2);
    public float zoomDuration = 2f;
    public static bool IsZoomed = false;

    public void OnModuleClickTrigger()
    {
        if (cameraMover == null)
            cameraMover = FindFirstObjectByType<CameraMover>();
        if (cubeGroupController == null)
            cubeGroupController = FindFirstObjectByType<CubeGroupFaceController>();

        if (cubeGroupController == null || !cubeGroupController.cameraIsFixed)
            return;
        if (CubeDragSnap.IsDragging)
            return;
        if (cameraMover != null && cameraMover.IsMoving)
            return;
        if (IsZoomed)
            return;

        Vector3 worldForward = transform.TransformDirection(Vector3.forward);
        Vector3 zoomTarget = transform.position + worldForward * zoomOffset.z;
        Quaternion zoomRot = Quaternion.LookRotation(transform.position - zoomTarget, transform.TransformDirection(Vector3.up));
        cameraMover.MoveTo(zoomTarget, zoomRot, zoomDuration);
        IsZoomed = true;
    }

    void Update()
    {
        // (1) 이동 중이라도 우클릭 복귀는 반드시 허용
        if (IsZoomed && Input.GetMouseButtonDown(1))
        {
            if (cubeGroupController != null)
                cubeGroupController.MoveCameraBackToFace();
            IsZoomed = false;
        }
    }
}
