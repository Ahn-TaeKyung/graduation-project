using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    // ▶ 진행 관련
    float targetTime;
    float currentTime;
    bool isRunning;

    // ▶ 빌보드(카메라 바라보기) 관련
    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;   // 비워두면 Camera.main

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartProgress(float duration)
    {
        targetTime = Mathf.Max(0.0001f, duration);
        currentTime = 0f;
        isRunning = true;
        gameObject.SetActive(true);

        if (fillImage)
            fillImage.fillAmount = 0f;
    }

    public void StopProgress()
    {
        isRunning = false;
        if (fillImage)
            fillImage.fillAmount = 0f;

        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // 1) 카메라 쪽으로 돌리기 (DashCooldownUI에서 하던 거랑 같음)
        if (faceCamera)
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera != null)
            {
                // 카메라가 보는 방향과 평행하게 만들어서 옆면이 안 보이게
                Vector3 fwd = targetCamera.transform.forward;
                Vector3 up  = Vector3.up;
                transform.rotation = Quaternion.LookRotation(fwd, up);
            }
        }

        // 2) 진행 업데이트
        if (!isRunning) return;

        currentTime += Time.deltaTime;

        if (fillImage)
            fillImage.fillAmount = Mathf.Clamp01(currentTime / targetTime);
    }
}
