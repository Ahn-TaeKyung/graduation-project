using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ButtonInputForwarder : MonoBehaviour
{
    public int buttonIndex; // 0~8 (입력 버튼), 9 (확인 버튼 등)
    public NineButtons targetManager; // Inspector에서 할당

    private ClickDragHandler handler;
    private Collider col;
    private Camera hackerCamera;
    private bool isPressed = false;
    private Collider myCollider;
    void Awake()
    {
        if (handler == null)
            handler = gameObject.AddComponent<ClickDragHandler>();
        col = GetComponent<Collider>();
        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }
        // 콜백 등록
        if (handler != null)
        {
            handler.OnLeftPress = () => OnPress();
            handler.OnLeftRelease = () => OnRelease();
        }
    }
    private void Update()
    {
        col.enabled = ModuleZoom.IsZoomed;
        if (targetManager.isComplete == true)
        {
            col.enabled = false;
        }
    }
    // Raycast로 내 위에 있을 때만 9Buttons에 입력 전달
    void OnPress()
    {
        if (!ModuleZoom.IsZoomed) return;
        var cam = hackerCamera;
        if (cam == null || col == null) return;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!col.Raycast(ray, out RaycastHit hit, 100f)) return;
        isPressed = true;
        if (targetManager == null)
            Debug.LogWarning($"{name}: targetManager(NineButtons)가 할당되지 않았습니다.");

        if (targetManager != null)
        {
            targetManager.OnButtonPress(buttonIndex);

        }
    }
    void OnRelease()
    {
        if (!ModuleZoom.IsZoomed) return;
        if (!isPressed) return;
        isPressed = false;
        if (targetManager == null)
            Debug.LogWarning($"{name}: targetManager(NineButtons)가 할당되지 않았습니다.");
        if (targetManager != null && buttonIndex < 9)
        {
            targetManager.OnButtonRelease(buttonIndex);
        }
        // 확인버튼(9)에서만 OnConfirmClick
        else if (targetManager != null && buttonIndex == 9)
        {
            targetManager.OnConfirmClick();
        }
    }
}
