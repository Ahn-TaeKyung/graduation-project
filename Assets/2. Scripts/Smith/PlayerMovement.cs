using UnityEngine;
using Fusion;

namespace gameScene
{
  [RequireComponent(typeof(NetworkObject))]
  [RequireComponent(typeof(NetworkTransform))]
  [RequireComponent(typeof(CharacterController))]
  public class PlayerMovement : NetworkBehaviour
  {
    [Header("Move")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 12f;
    [SerializeField] Animator anim;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 12f;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 1.5f;
    const float DashEpsilon = 0.03f;

    [Header("Animator Params")]
    [SerializeField] string walkParam = "isWalking";
    [SerializeField] string holdParam = "isHolding";

    CharacterController cc;
    PlayerInteractor interactor;

    float baseY;
    Vector3 animLocalOrigin;

    [Networked] private float DashEndTime  { get; set; }
    [Networked] private float NextDashTime { get; set; }
    [Networked] private Vector3 DashDir    { get; set; }

    // 👇 "걷고 있다"를 네트워크로도 보내는 버전
    [Networked] private NetworkBool IsMovingNet { get; set; }

    // 로컬에서도 쓸 수 있게 캐시(호스트는 이게 실데이터, 프록시는 네트워크 값 복사)
    bool _simIsDashing;
    bool _simHasMoveInput;

    // --- 여긴 UI용 프로퍼티들 ---
    public float DashCooldownSeconds => dashCooldown;

    public float DashRemaining {
      get {
        if (Runner == null)
          return 0f;
        float remain = NextDashTime - Runner.SimulationTime;
        return remain < 0f ? 0f : remain;
      }
    }

    public float DashCooldown01 {
      get {
        if (dashCooldown <= 0f)
          return 0f;
        return DashRemaining / dashCooldown;
      }
    }
    // ------------------------------

    public override void Spawned() {
      cc = GetComponent<CharacterController>();
      interactor = GetComponent<PlayerInteractor>();

      baseY = transform.position.y;
      cc.stepOffset = 0.05f;

      if (anim != null) {
        anim.applyRootMotion = false;
        animLocalOrigin = anim.transform.localPosition;
      }
    }

    public override void FixedUpdateNetwork() {
      // 입력 못 받으면 끝
      if (!GetInput(out NetworkInputData input))
        return;

      // 👉 여기까지는 모든 클라이언트가 와도 OK
      // 여기서 isHolding 같은 건 계산해도 되지만
      // 실제 이동/대시는 권한 가진 쪽에서만 한다

      // ─────────────────────────────
      // ❗ 권한 없는 프록시는 여기서 끝
      if (!Object.HasStateAuthority)
        return;
      // ─────────────────────────────

      float now = Runner.SimulationTime;
      bool isDashing = now < DashEndTime;

      Vector3 moveDir = new Vector3(input.horizontal, 0f, input.vertical);
      bool hasMoveInput = moveDir.sqrMagnitude > 0.0001f;

      // ── 대시 시작 ──
      if (input.dash && !isDashing && now + DashEpsilon >= NextDashTime) {
        Vector3 dashDir = hasMoveInput ? moveDir.normalized : transform.forward;
        if (dashDir.sqrMagnitude < 0.0001f)
          dashDir = transform.forward;

        DashDir      = dashDir;
        DashEndTime  = now + dashDuration;
        NextDashTime = now + dashCooldown;
        isDashing    = true;
      }

      // ── 이동 ──
      if (isDashing) {
        cc.Move(DashDir * dashSpeed * Runner.DeltaTime);

        var look = Quaternion.LookRotation(DashDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
          transform.rotation,
          look,
          rotateSpeed * Runner.DeltaTime * 2f
        );
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
      }

      // y 고정
      float yDiff = baseY - transform.position.y;
      if (Mathf.Abs(yDiff) > 0.001f) {
        cc.Move(new Vector3(0f, yDiff, 0f));
      }

      // ✅ 여기서 "내가 지금 움직이고 있냐"를 네트워크에 싣는다
      IsMovingNet    = hasMoveInput || isDashing;

      // ✅ 그리고 로컬 캐시에도 저장 (호스트는 이걸 Render에서 씀)
      _simIsDashing   = isDashing;
      _simHasMoveInput = hasMoveInput;
    }

    public override void Render() {
      // 모델 위치 고정
      if (anim != null) {
        anim.transform.localPosition = animLocalOrigin;
      }

      if (anim) {
        bool isHolding = interactor != null &&
                         interactor.hand != null &&
                         !interactor.hand.IsEmpty;

        // ✅ 권한 가진 쪽은 로컬 시뮬 값 사용
        // ✅ 권한 없는 프록시는 네트워크에서 온 값 사용
        bool isWalkingLike =
          Object.HasStateAuthority
            ? (_simIsDashing || _simHasMoveInput)
            : (IsMovingNet || Runner.SimulationTime < DashEndTime); // 대시 중이면 무조건 걷기 true

        anim.SetBool(walkParam, isWalkingLike);
        anim.SetBool(holdParam, isHolding);
      }
    }
  }
}
