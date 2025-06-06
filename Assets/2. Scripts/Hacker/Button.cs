using System.Collections;
using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("버튼 눌림 거리")]
    public float pressDistance = 1;
    [Header("눌림 방향 (예: -z)")]
    public Vector3 pressDirection = Vector3.back;
    private float holdThreshold = 0.3f;

    private Vector3 defaultPosition;
    private bool isPressed = false;
    private float pressTime;

    private ModuleZoom ModuleZoom;
    private ClickDragHandler clickDragHandler;
    private Camera hackerCamera;
    private ButtonPatternManager patternManager;
    private Collider myCollider;   // Inspector에서 할당(혹은 GetComponent로 할당)

    void Awake()
    {
        defaultPosition = transform.localPosition;
        myCollider = GetComponent<Collider>();
        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }
        ModuleZoom = GetComponentInParent<ModuleZoom>();
        if (patternManager == null)
            patternManager = GetComponent<ButtonPatternManager>();
        if (patternManager != null)
        {
            patternManager.patternCode = GenerateRandomCode();
            patternManager.ApplyPatternCode(); // 코드 바뀌었으니 패턴 새로 생성
            Debug.Log($"[Button] 내 코드: {patternManager.patternCode}");
        }
        clickDragHandler = GetComponent<ClickDragHandler>();
        if (clickDragHandler == null)
            clickDragHandler = gameObject.AddComponent<ClickDragHandler>();

        clickDragHandler.OnLeftPress += OnButtonPress;
        clickDragHandler.OnLeftRelease += OnButtonRelease;  // 뗄 때(올라감)

    }
    private void Update()
    {
        myCollider.enabled = ModuleZoom.c_zoomed;
    }

    void OnDestroy()
    {
        // 메모리릭 방지
        if (clickDragHandler != null)
        {
            clickDragHandler.OnLeftPress -= OnButtonPress;
            clickDragHandler.OnLeftRelease -= OnButtonRelease;  // 뗄 때(올라감)

        }
    }


    void OnButtonPress()
    {
        if (!ModuleZoom.c_zoomed) return;
        // Raycast로 자기 자신 위인지 확인
        Ray ray = hackerCamera.ScreenPointToRay(clickDragHandler.LastClickPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.transform == this.transform)
            {
                // 버튼의 중심이 카메라 View 안에 있는지 확인
                Vector3 viewportPos = hackerCamera.WorldToViewportPoint(transform.position);

                bool isVisible =
                    viewportPos.z > 0 && // 카메라 앞에 있어야 함
                    viewportPos.x > 0 && viewportPos.x < 1 &&
                    viewportPos.y > 0 && viewportPos.y < 1;

                if (isVisible)
                {
                    patternManager.OnInputStarted();
                    pressTime = Time.time;
                    ButtonPress();
                }
                // 보이지 않으면 무시
            }
        }
    }


    void OnButtonRelease()
    {
        if (!ModuleZoom.c_zoomed) return;
        if (!isPressed) return;

        float holdTime = Time.time - pressTime;

        if (holdTime <= holdThreshold)
        {
            // 클릭 처리
            patternManager.OnButtonClick();
            Debug.Log($"[Button] Click 판정, 시간:{holdTime:F2}s");
        }
        else
        {
            // 홀드 처리
            patternManager.OnButtonHold(holdTime);
            Debug.Log($"[Button] Hold 판정, 시간:{holdTime:F2}s");
        }

        ButtonRelease();
    }

    void ButtonPress()
    {
        if (!isPressed)
        {
            transform.localPosition = defaultPosition + pressDirection.normalized * pressDistance;
            isPressed = true;
            Debug.Log($"[Button:{gameObject.name}] 눌림");
        }
    }

    void ButtonRelease()
    {
        if (isPressed)
        {
            transform.localPosition = defaultPosition;
            isPressed = false;
            Debug.Log($"[Button:{gameObject.name}] 복귀");
        }
    }

    public static string GenerateRandomCode(int length = 6)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        System.Random rand = new System.Random(System.DateTime.Now.Millisecond + UnityEngine.Random.Range(0, 100000));
        char[] code = new char[length];
        for (int i = 0; i < length; i++)
            code[i] = chars[rand.Next(62)];
        return new string(code);
    }
}
