using UnityEngine;
using UnityEngine.InputSystem;

// GameReady 시점에 동작하도록 인터페이스 추가
public class PlayerMove : MonoBehaviour, IGameReadyListener
{
    public float moveSpeed = 5f;
    public float fastSpeed = 12f;
    public float jumpForce = 7f;
    public float lookSensitivity = 2f;

    private Rigidbody rb;
    public static bool inputEnabled = false;
    private bool isGrounded = false;
    private float yRotation = 0f;
    private RoleType myRole;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 초기에는 입력 비활성화
        // inputEnabled = false;
        // Cursor.lockState = CursorLockMode.Locked;

        // GameStateManager에 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[PlayerMove] GameStateManager 인스턴스를 찾을 수 없음.");
        }
    }

    public void OnGameReady()
    {
        // 역할 확인 후 Hacker인 경우에만 입력 활성화
        myRole = GameSceneManager.Instance.GetMyRole();
        if (myRole == RoleType.Hacker)
        {
            inputEnabled = true;
            Debug.Log("[PlayerMove] Hacker 역할이므로 입력 활성화됨.");
        }
        else
        {
            inputEnabled = false;
            Debug.Log("[PlayerMove] Hacker가 아니므로 입력 비활성화.");
        }
    }

    void Update()
    {
        if (inputEnabled)
        {
            // 마우스 좌우 회전
            if (Mouse.current != null)
            {
                float mouseX = Mouse.current.delta.x.ReadValue() * lookSensitivity * 0.1f;
                yRotation += mouseX;
                transform.rotation = Quaternion.Euler(0, yRotation, 0);
            }

            // 이동
            Vector3 move = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed) move += transform.right;

            float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;
            Vector3 velocity = rb.linearVelocity;
            velocity.x = move.normalized.x * speed;
            velocity.z = move.normalized.z * speed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

            // 점프
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    void OnCollisionStay(Collision collision) => isGrounded = true;
    void OnCollisionExit(Collision collision) => isGrounded = false;
}
