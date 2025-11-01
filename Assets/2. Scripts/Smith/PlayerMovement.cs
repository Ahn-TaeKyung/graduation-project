using UnityEngine;
using Fusion;

namespace gameScene {
  [RequireComponent(typeof(NetworkObject))]
  [RequireComponent(typeof(NetworkTransform))]
  [RequireComponent(typeof(CharacterController))]
  public class PlayerMovement : NetworkBehaviour {
    [Header("Move")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 12f;
    [SerializeField] Animator anim;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 12f;     // 대시할 때 속도
    [SerializeField] float dashDuration = 0.25f; // 대시가 유지되는 시간 (초)
    [SerializeField] float dashCooldown = 1.5f;  // 대시 쿨타임 (초)

    CharacterController cc;

    // 네트워크에서 공유해야 하는 값들
    [Networked] private float DashEndTime    { get; set; }
    [Networked] private float NextDashTime   { get; set; }
    [Networked] private Vector3 DashDir      { get; set; }  // 어떤 방향으로 대시하는지

    public override void Spawned() {
      cc = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork() {
      if (!GetInput(out NetworkInputData input))
        return;

      // 이동/대시는 StateAuthority만 한다
      if (!Object.HasStateAuthority)
        return;

      // 현재 시간(시뮬레이션 시간)
      float now = Runner.SimulationTime; // Fusion이 주는 네트워크 시간

      bool isDashing = now < DashEndTime;

      // 현재 입력 방향
      Vector3 moveDir = new Vector3(input.horizontal, 0f, input.vertical);
      bool hasMoveInput = moveDir.sqrMagnitude > 0.0001f;

      // ---- 대시 시작 조건 ----
      // 1) 대시 키가 눌렸고
      // 2) 지금 대시 중이 아니고
      // 3) 쿨타임이 끝났고
      if (input.dash && !isDashing && now >= NextDashTime) {
        // 입력이 없으면 마지막 바라보는 방향으로 대시해도 되고,
        // 여기서는 "입력 방향이 있으면 그쪽, 없으면 현재 forward"로 해보자.
        Vector3 dashDir = hasMoveInput ? moveDir.normalized : transform.forward;
        if (dashDir.sqrMagnitude < 0.0001f) {
          dashDir = transform.forward;
        }

        DashDir = dashDir;
        DashEndTime = now + dashDuration;
        NextDashTime = now + dashCooldown;

        isDashing = true;
      }

      // ---- 실제 이동 ----
      if (isDashing) {
        // 대시 중
        cc.Move(DashDir * dashSpeed * Runner.DeltaTime);

        // 보는 방향도 대시하는 방향으로
        var look = Quaternion.LookRotation(DashDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
          transform.rotation,
          look,
          rotateSpeed * Runner.DeltaTime * 2f
        );

        if (anim) {
          anim.SetBool("isWalking", true);  // 대시 애니 따로 있으면 여기서 Trigger
          anim.speed = 1.5f;                // 살짝 빠르게
        }
      }
      else {
        // 일반 이동
        if (hasMoveInput) {
          moveDir.Normalize();
          cc.Move(moveDir * moveSpeed * Runner.DeltaTime);

          var look = Quaternion.LookRotation(moveDir, Vector3.up);
          transform.rotation = Quaternion.Slerp(
            transform.rotation,
            look,
            rotateSpeed * Runner.DeltaTime
          );
        }

        if (anim) {
          anim.SetBool("isWalking", hasMoveInput);
          anim.speed = 1f;
        }
      }
    }
  }
}
