using UnityEngine;
using Fusion;
using UnityEngine.InputSystem; // 새 Input System 네임스페이스

namespace gameScene
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody rb;

        public override void Spawned()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            // 자신의 캐릭터일 때만 카메라 따라오게 설정
            // if (Object.HasInputAuthority)
            // {
            //     Camera.main.transform.SetParent(transform);
            //     Camera.main.transform.localPosition = new Vector3(0, 10, -8);
            //     Camera.main.transform.localEulerAngles = new Vector3(45, 0, 0);
            // }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                Vector3 move = new Vector3(inputData.horizontal, 0f, inputData.vertical);

                // 정규화된 입력값으로 이동
                if (move.sqrMagnitude > 0.01f)
                {
                    Vector3 target = rb.position + move.normalized * moveSpeed * Runner.DeltaTime;
                    rb.MovePosition(target);
                }
            }
        }
    }
}
