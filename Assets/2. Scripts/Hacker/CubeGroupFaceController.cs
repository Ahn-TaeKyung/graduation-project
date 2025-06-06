using UnityEngine;

public class CubeGroupFaceController : MonoBehaviour
{
    public CameraMover cameraMover;
    public float viewDistance = 7f;
    public float cameraMoveDuration = 0.5f;

    [HideInInspector]
    public bool IsCameraFixed = false;

    private Vector3 fixedDirection;
    private Vector3 fixedCameraPos;
    private Quaternion fixedCameraRot;
    int cubeGroupLayerMask;

    private Camera hackerCamera;
    private ClickDragHandler inputHandler;

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

        // 입력 핸들러 붙이기(없으면 자동 생성)
        inputHandler = GetComponent<ClickDragHandler>();
        if (inputHandler == null)
            inputHandler = gameObject.AddComponent<ClickDragHandler>();

        inputHandler.OnLeftClick += OnLeftClickHandler;
        Debug.Log("[CubeGroupFaceController] ClickHandler 바인딩 완료");
    }

    // 좌클릭 이벤트 처리
    void OnLeftClickHandler()
    {
        if (cameraMover == null)
        {
            Debug.LogWarning("[CubeGroupFaceController] cameraMover가 할당되지 않았습니다.");
            return;
        }

        if (CameraMover.isMoving)
        {
            Debug.Log("[CubeGroupFaceController] 카메라 이동 중 - 클릭 무시");
            return;
        }

        if (!IsCameraFixed)
        {
            Vector2 clickScreenPos = inputHandler.LastClickPos;
            Ray ray = hackerCamera.ScreenPointToRay(clickScreenPos);
            Debug.Log($"[CubeGroupFaceController] Ray 생성: {clickScreenPos}");

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeGroupLayerMask))
            {
                Debug.Log($"[CubeGroupFaceController] Raycast hit: {hit.transform.name}");
                if (hit.transform == this.transform)
                    if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
                    {
                        Debug.Log("[CubeGroupFaceController] CameraMove 호출");
                        MoveCameraToFace();
                    }
            }
        }
    }

    public void MoveCameraToFace()
    {
        fixedDirection = Vector3.back; // 뒤쪽 방향
        fixedCameraPos = transform.position + fixedDirection * viewDistance;
        fixedCameraRot = Quaternion.LookRotation(transform.position - fixedCameraPos, Vector3.up);

        cameraMover.MoveTo(fixedCameraPos, fixedCameraRot, cameraMoveDuration, () =>
        {
            IsCameraFixed = true;
        });
    }

    public void MoveCameraBackToFace()
    {
        if (IsCameraFixed)
            cameraMover.MoveTo(fixedCameraPos, fixedCameraRot, cameraMoveDuration);

    }
    public void MoveCameraToFace(System.Action onComplete = null)
    {
        fixedDirection = Vector3.back; // 뒤쪽 방향
        fixedCameraPos = transform.position + fixedDirection * viewDistance;
        fixedCameraRot = Quaternion.LookRotation(transform.position - fixedCameraPos, Vector3.up);

        cameraMover.MoveTo(fixedCameraPos, fixedCameraRot, cameraMoveDuration, () =>
        {
            IsCameraFixed = true;
        });
    }

    public void MoveCameraBackToFace(System.Action onComplete = null)
    {
        // 원위치/회전으로 부드러운 복귀
        cameraMover.MoveTo(cameraMover.DefaultCameraPosition, cameraMover.DefaultCameraRotation, cameraMoveDuration, onComplete);
    }

}