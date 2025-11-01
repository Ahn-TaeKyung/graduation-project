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
    [SerializeField] float dashSpeed = 12f;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 1.5f;
    //  아주 약간의 여유 (핑/부동소수 오차 때문에)
    const float DashEpsilon = 0.03f;

    CharacterController cc;

    [Networked] private float DashEndTime  { get; set; }
    [Networked] private float NextDashTime { get; set; }
    [Networked] private Vector3 DashDir    { get; set; }

    public float DashCooldownSeconds => dashCooldown;
    public float DashRemaining {
      get {
        if (Runner == null) return 0f;
        float remain = NextDashTime - Runner.SimulationTime;
        return remain < 0f ? 0f : remain;
      }
    }
    public float DashCooldown01 {
      get {
        if (dashCooldown <= 0f) return 0f;
        return DashRemaining / dashCooldown;
      }
    }

    public override void Spawned() {
      cc = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork() {
      if (!GetInput(out NetworkInputData input))
        return;

      // 이동/대시는 서버(StateAuthority)만
      if (!Object.HasStateAuthority)
        return;

      float now = Runner.SimulationTime;
      bool isDashing = now < DashEndTime;

      Vector3 moveDir = new Vector3(input.horizontal, 0f, input.vertical);
      bool hasMoveInput = moveDir.sqrMagnitude > 0.0001f;

      // 대시 시작 조건에 epsilon 추가
      if (input.dash && !isDashing && now + DashEpsilon >= NextDashTime) {
        Vector3 dashDir = hasMoveInput ? moveDir.normalized : transform.forward;
        if (dashDir.sqrMagnitude < 0.0001f)
          dashDir = transform.forward;

        DashDir      = dashDir;
        DashEndTime  = now + dashDuration;
        NextDashTime = now + dashCooldown;
        isDashing    = true;

        // (선택) 입력 버퍼 소비하고 싶으면
        // PlayerInputHandler.ConsumeDash();
      }

      if (isDashing) {
        cc.Move(DashDir * dashSpeed * Runner.DeltaTime);

        var look = Quaternion.LookRotation(DashDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
          transform.rotation,
          look,
          rotateSpeed * Runner.DeltaTime * 2f
        );

        if (anim) {
          anim.SetBool("isWalking", true);
          anim.speed = 1.5f;
        }
      } else {
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
