using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float fastSpeed = 12f;
    public float jumpForce = 7f;
    public float lookSensitivity = 2f;

    Rigidbody rb;
    public static bool inputEnabled;
    bool isGrounded = false;
    float yRotation = 0f;

    void Start()
    {
        inputEnabled = true;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (inputEnabled)
        {


            // 마우스 좌우로 Player(캡슐)만 회전
            if (Mouse.current != null)
            {
                float mouseX = Mouse.current.delta.x.ReadValue() * lookSensitivity * 0.1f;
                yRotation += mouseX;
                transform.rotation = Quaternion.Euler(0, yRotation, 0);
            }

            // WASD 이동
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