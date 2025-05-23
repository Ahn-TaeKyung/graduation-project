using UnityEngine;

public class CubeGroupFaceController : MonoBehaviour
{
    public CameraMover cameraMover;
    public float viewDistance = 5f;
    public float cameraMoveDuration = 1.0f;

    [HideInInspector]
    public bool cameraIsFixed = false;

    private Vector3 fixedDirection;
    private Vector3 fixedCameraPos;
    private Quaternion fixedCameraRot;
    int cubeGroupLayerMask;

    void Awake()
    {
        if (cameraMover == null)
            cameraMover = FindFirstObjectByType<CameraMover>();
        fixedDirection = transform.forward;
        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");
    }

    void Update()
    {
        if (cameraMover != null && cameraMover.IsMoving)
            return;
        if (!cameraIsFixed && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeGroupLayerMask))
            {
                if (hit.transform == this.transform)
                {
                    MoveCameraToFrontFace();
                }
            }
        }
    }

    public void MoveCameraToFrontFace()
    {
        fixedCameraPos = transform.position + fixedDirection * viewDistance;
        fixedCameraRot = Quaternion.LookRotation(-fixedDirection, Vector3.up);

        cameraMover.MoveTo(fixedCameraPos, fixedCameraRot, cameraMoveDuration);
        cameraIsFixed = true;
    }

    public void MoveCameraBackToFace()
    {
        if (cameraIsFixed)
            cameraMover.MoveTo(fixedCameraPos, fixedCameraRot, cameraMoveDuration);
    }
}
