using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour, IGameReadyListener
{
    public float lookSensitivity = 2f;
    public float minX = -80f, maxX = 80f;
    float xRotation = 0f;
    public Camera hackerCamera; // 플레이어 카메라
    public PlayerMove playermove;
    public Transform cubeTransform; // 클릭 대상 큐브
    public CameraMover cameraMover;
    public CubeGroupFaceController faceController;
    public float activateDistance = 100f; // 레이캐스트 거리
    public float cameraMoveDuration = 0.5f; // 카메라 큐브 고정 시간

    private bool isCubeMode = false;
    private Vector3 beforePosition;
    private Quaternion beforeRotation;

    private void Start()
    {
        // GameStateManager에 등록해서 Ready 상태가 되면 OnGameReady 호출되도록 함
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[PlayerLook] GameStateManager 인스턴스가 없습니다.");
            // 기본 초기화 (테스트용)
            InitializePlayerLook();
        }

        // 기존 Start 초기화 중 씬에 있는 오브젝트 참조만 미리 찾아둠 (OnGameReady에서 활성화 관련 초기화)
        if (hackerCamera == null)
        {
            GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
            if (hackerCameraObject != null)
            {
                hackerCamera = hackerCameraObject.GetComponent<Camera>();
            }
        }
        if (playermove == null) playermove = GetComponentInParent<PlayerMove>();
        if (cameraMover == null) cameraMover = FindFirstObjectByType<CameraMover>();
        if (cubeTransform == null)
        {
            var cubeObj = GameObject.Find("base");
            if (cubeObj != null)
                cubeTransform = cubeObj.transform;
            else
                Debug.LogWarning("[PlayerLook] Cube라는 이름의 오브젝트를 찾을 수 없습니다.");
        }
        if (faceController == null) faceController = FindFirstObjectByType<CubeGroupFaceController>();
    }

    // IGameReadyListener 인터페이스 구현
    public void OnGameReady()
    {
        // 게임 준비 완료 시점에 수행할 초기화
        InitializePlayerLook();
    }

    private void InitializePlayerLook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCubeMode = false;
        xRotation = 0f;
    }

    void Update()
    {
        if (PlayerMove.inputEnabled)
        {
            float mouseY = Mouse.current.delta.y.ReadValue() * lookSensitivity * 0.1f;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minX, maxX);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

        if (!isCubeMode)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = hackerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, activateDistance))
                {
                    if (hit.transform == cubeTransform)
                    {
                        PlayerMove.inputEnabled = false;
                        // 커서 보이기
                        beforePosition = hackerCamera.transform.position;
                        beforeRotation = hackerCamera.transform.rotation;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        // 카메라 이동
                        if (faceController != null)
                        {
                            faceController.MoveCameraToFace(() =>
                            {
                                isCubeMode = true;
                            });
                        }
                    }
                }
            }
        }
        else
        {
            if (Mouse.current.rightButton.wasPressedThisFrame && !ModuleZoom.IsZoomed)
            {
                PlayerMove.inputEnabled = false;

                if (faceController != null)
                {
                    cameraMover.MoveTo(beforePosition, beforeRotation, cameraMoveDuration, () =>
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        PlayerMove.inputEnabled = true;
                        isCubeMode = false;
                        faceController.SetCameraFixed(false);
                    });
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 씬 종료 시 반드시 등록 해제
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterListener(this);
        }
    }
}
