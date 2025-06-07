using UnityEngine;
using UnityEngine.InputSystem;

public class ClickDragHandler : MonoBehaviour
{
    public System.Action<Vector2> OnDrag;        // 드래그 이동량
    public System.Action OnLeftPress;            // 왼클릭 눌렀을 때
    public System.Action OnLeftRelease;          // 왼클릭 땠을 때
    public System.Action OnLeftClick;            // 짧은 클릭
    public System.Action OnRightClick;           // 우클릭
    public System.Action OnDragEnd;              // 드래그 끝
    public System.Action OnLeftHoldPress;        // 길게 누를때
    public System.Action<float> OnLeftHoldRelease; // 누르고 있다가 뗄 때 hold시간 전달

    public Vector2 LastClickPos { get; private set; } // 클릭/드래그 발생 위치

    private bool isDragging = false;
    private bool isHolding = false;
    private Vector2 mouseDownPos;
    private Vector2 lastMousePos;
    private float mouseDownTime;
    private float holdStartTime;

    private const float dragThreshold = 3f;
    private const float clickDelay = 0.1f;


    void Update()
    {

        // 좌클릭 시작
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseDownTime = Time.time;
            holdStartTime = Time.time;
            isHolding = true;
            LastClickPos = Mouse.current.position.ReadValue();
            mouseDownPos = Mouse.current.position.ReadValue();
            lastMousePos = mouseDownPos;
            isDragging = false;
            OnLeftPress?.Invoke();
        }

        // 드래그
        if (Mouse.current.leftButton.isPressed && mouseDownTime > 0)
        {
            Vector2 currPos = Mouse.current.position.ReadValue();
            Vector2 delta = currPos - lastMousePos;

            if (!isDragging && (currPos - mouseDownPos).magnitude > dragThreshold)
                isDragging = true;

            if (isDragging)
                OnDrag?.Invoke(delta);

            lastMousePos = currPos;
        }

        // 좌클릭 해제
        if (Mouse.current.leftButton.wasReleasedThisFrame && mouseDownTime > 0)
        {
            float heldTime = Time.time - mouseDownTime;
            float totalMove = (Mouse.current.position.ReadValue() - mouseDownPos).magnitude;

            if (!isDragging && heldTime < clickDelay && totalMove < dragThreshold)
            {
                LastClickPos = Mouse.current.position.ReadValue();
                OnLeftClick?.Invoke();
                OnLeftRelease?.Invoke();
            }
            else if (isDragging)
                OnDragEnd?.Invoke();
                OnLeftRelease?.Invoke();

            // 홀드(길게누르기) 처리: 무조건 이벤트 발생
            if (isHolding)
            {
                float holdTime = Time.time - holdStartTime;
                OnLeftHoldRelease?.Invoke(holdTime);
                isHolding = false;
            }
            mouseDownTime = -1000f;
            isDragging = false;
        }

        // 우클릭
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            LastClickPos = Mouse.current.position.ReadValue();
            OnRightClick?.Invoke();
        }
    }
}
