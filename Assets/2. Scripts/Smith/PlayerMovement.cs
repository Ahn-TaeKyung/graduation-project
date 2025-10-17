using UnityEngine;
using UnityEngine.InputSystem; // 새 Input System 네임스페이스

namespace gameScene
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;

        private Rigidbody rb;
        private Vector2 moveInput; // 새 Input System은 Vector2를 기본으로 사용
        private Vector3 moveDirection;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        void Update()
        {
            // 키보드 입력 감지
            if (Keyboard.current == null) return;

            float h = 0f;
            float v = 0f;

            if (Keyboard.current.aKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed) h += 1f;
            if (Keyboard.current.sKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed) v += 1f;

            moveInput = new Vector2(h, v).normalized;
        }

        void FixedUpdate()
        {
            moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
