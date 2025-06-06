using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        if (playermove == null) playermove = GetComponentInParent<PlayerMove>();
        if (cameraMover == null) cameraMover = FindFirstObjectByType<CameraMover>();
        if (cubeTransform == null)
        {
            var cubeObj = GameObject.Find("base");
            if (cubeObj != null)
                cubeTransform = cubeObj.transform;
            else
                Debug.LogWarning("Cube라는 이름의 오브젝트를 찾을 수 없습니다.");
        }
        if (faceController == null) faceController = FindFirstObjectByType<CubeGroupFaceController>();
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
                        // CubeGroupFaceController가 카메라 이동 주도
                        if (faceController != null)
                        {
                            faceController.MoveCameraToFace(() =>
                            {
                                // 카메라 이동 완료 콜백에서 큐브 모드 진입
                                isCubeMode = true;
                            });

                        }
                    }
                }
            }
        }
        else
        {
            // 우클릭/ESC 시 복귀
            if (Mouse.current.rightButton.wasPressedThisFrame && !ModuleZoom.IsZoomed)
            {
                PlayerMove.inputEnabled = false;

                // CubeGroupFaceController가 카메라 복귀 주도
                if (faceController != null)
                {
                    cameraMover.MoveTo(beforePosition, beforeRotation, cameraMoveDuration,()=>
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        PlayerMove.inputEnabled = true;
                        isCubeMode = false;
                        faceController.IsCameraFixed = false;
                    });
                }
            }
        }
    }
}