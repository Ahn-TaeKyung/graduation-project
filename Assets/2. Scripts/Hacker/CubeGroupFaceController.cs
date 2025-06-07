using UnityEngine;

public class CubeGroupFaceController : MonoBehaviour, IGameReadyListener
{
    public CameraMover cameraMover;
    public float viewDistance = 7f;
    public float cameraMoveDuration = 0.5f;

    public bool IsCameraFixed { get; private set; } = false;

    private Camera hackerCamera;
    private ClickDragHandler inputHandler;
    private int cubeGroupLayerMask;
    private GameSceneManager GameSceneManager;

    private void Awake()
    {
        if (cameraMover == null)
            cameraMover = FindFirstObjectByType<CameraMover>();

        inputHandler = GetComponent<ClickDragHandler>() ?? gameObject.AddComponent<ClickDragHandler>();
        inputHandler.OnLeftClick += OnLeftClickHandler;

        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");
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
        }
        else
        {
            Debug.LogError("[CubeGroupFaceController] 해커 카메라를 찾을 수 없습니다.");
        }
    }

    void OnLeftClickHandler()
    {
        if (CameraMover.isMoving || IsCameraFixed || hackerCamera == null)
            return;

        Ray ray = hackerCamera.ScreenPointToRay(inputHandler.LastClickPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeGroupLayerMask))
        {
            if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
                MoveCameraToFace();
        }
    }
    public void SetCameraFixed(bool value)
    {
        IsCameraFixed = value;
    }

    public void MoveCameraToFace(System.Action onComplete = null)
    {
        Vector3 dir = Vector3.back;
        Vector3 pos = transform.position + dir * viewDistance;
        Quaternion rot = Quaternion.LookRotation(transform.position - pos, Vector3.up);

        cameraMover.MoveTo(pos, rot, cameraMoveDuration, () =>
        {
            SetCameraFixed(true); // 여기서도 직접 할당 대신 메서드 사용 가능
            onComplete?.Invoke();
        });
    }

    public void MoveCameraBackToFace(System.Action onComplete = null)
    {
        cameraMover.MoveTo(cameraMover.DefaultCameraPosition, cameraMover.DefaultCameraRotation, cameraMoveDuration, onComplete);
    }
}
