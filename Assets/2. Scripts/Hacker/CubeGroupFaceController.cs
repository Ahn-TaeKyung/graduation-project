using UnityEngine;
using UnityEngine.InputSystem; // ✅ 새 Input System 사용

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

    private Camera hackerCamera;

    void Awake()
    {
        if (cameraMover == null)
            cameraMover = FindFirstObjectByType<CameraMover>();

        fixedDirection = transform.forward;
        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");

        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (cameraMover != null && cameraMover.IsMoving)
            return;

        if (!cameraIsFixed && Mouse.current.leftButton.wasPressedThisFrame && hackerCamera != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = hackerCamera.ScreenPointToRay(mousePos);

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
