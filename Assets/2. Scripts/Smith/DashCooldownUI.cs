using UnityEngine;
using UnityEngine.UI;
using Fusion;
using gameScene; // PlayerMovement 있는 네임스페이스

public class DashCooldownUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement target;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;

    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;   // 비워두면 Camera.main

    [Header("Color")]
    [SerializeField] private Color colorStart = Color.red;    // 쿨 시작 (방금 씀)
    [SerializeField] private Color colorMid   = Color.yellow; // 중간
    [SerializeField] private Color colorEnd   = Color.green;  // 쿨 끝 직전

    void Awake()
    {
        if (target == null)
            target = GetComponentInParent<PlayerMovement>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        // 1) 항상 카메라와 평행하게
        if (faceCamera)
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera != null)
            {
                // 캔버스를 "카메라가 보는 방향"으로 돌려서 옆면이 안 보이게
                var fwd = targetCamera.transform.forward;
                var up  = Vector3.up;
                transform.rotation = Quaternion.LookRotation(fwd, up);
            }
        }

        // 2) 타겟/오브젝트 체크
        if (target == null || target.Object == null)
            return;

        // 내 플레이어 아니면 안 보이게
        if (!target.Object.HasInputAuthority)
        {
            if (canvasGroup) canvasGroup.alpha = 0f;
            return;
        }

        // 3) 쿨타임 값 가져오기 (1 = 막 썼다, 0 = 끝)
        float t = target.DashCooldown01;
        bool onCooldown = t > 0f;

        if (canvasGroup)
            canvasGroup.alpha = onCooldown ? 1f : 0f;

        if (!onCooldown || fillImage == null)
            return;

        // 4) 채우기 (시간 지날수록 차오르게)
        fillImage.fillAmount = 1f - t;

        // 5) 색상 LERP (빨강 -> 노랑 -> 초록)
        // t: 1 → 0.5  : 빨강 → 노랑
        // t: 0.5 → 0  : 노랑 → 초록
        if (t > 0.5f)
        {
            // t=1일 때 lerp=0, t=0.5일 때 lerp=1
            float k = (t - 0.5f) / 0.5f;
            k = Mathf.Clamp01(k);
            fillImage.color = Color.Lerp(colorMid, colorStart, k); // 빨강 쪽으로
        }
        else
        {
            // t=0.5일 때 lerp=1, t=0일 때 lerp=0
            float k = t / 0.5f;
            k = Mathf.Clamp01(k);
            fillImage.color = Color.Lerp(colorEnd, colorMid, k); // 초록 쪽으로
        }
    }
}
