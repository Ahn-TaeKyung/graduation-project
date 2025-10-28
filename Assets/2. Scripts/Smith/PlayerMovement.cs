using UnityEngine;
using Fusion;

namespace gameScene {
  [RequireComponent(typeof(NetworkObject))]
  [RequireComponent(typeof(NetworkTransform))]
  [RequireComponent(typeof(CharacterController))]
  public class PlayerMovement : NetworkBehaviour {
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 12f;
    [SerializeField] Animator anim;

    CharacterController cc;

    public override void Spawned() {
      cc = GetComponent<CharacterController>();
      // 중력 없이 탑뷰: cc.stepOffset = 0.3f; 정도만
    }

    public override void FixedUpdateNetwork() {
      if (!GetInput(out NetworkInputData input)) return;

      // ★ 역시 StateAuthority에서만 실제 이동 (지터 방지)
      if (!Object.HasStateAuthority) return;

      Vector3 dir = new Vector3(input.horizontal, 0f, input.vertical);
      bool isMoving = dir.sqrMagnitude > 0.0001f;

      if (isMoving) {
        dir.Normalize();
        cc.Move(dir * moveSpeed * Runner.DeltaTime); // ★ transform 대신 CC.Move
        var look = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateSpeed * Runner.DeltaTime);
      }
      if (anim) anim.SetBool("isWalking", isMoving);
    }
  }
}
