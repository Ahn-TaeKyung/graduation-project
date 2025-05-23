using UnityEngine;
using UnityEngine.InputSystem; // 추가

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Input System에서 Keyboard.current로 입력 체크
        float h = 0, v = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) v += 1;
            if (Keyboard.current.sKey.isPressed) v -= 1;
            if (Keyboard.current.dKey.isPressed) h += 1;
            if (Keyboard.current.aKey.isPressed) h -= 1;
        }
        moveInput = new Vector2(h, v).normalized;
    }

    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }
}