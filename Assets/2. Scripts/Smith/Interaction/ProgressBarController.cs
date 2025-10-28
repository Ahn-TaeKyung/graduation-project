using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    float targetTime;
    float currentTime;
    bool isRunning;

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
        if (fillImage) fillImage.fillAmount = 0f;
    }

    public void StopProgress()
    {
        isRunning = false;
        if (fillImage) fillImage.fillAmount = 0f;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isRunning) return;
        currentTime += Time.deltaTime;
        if (fillImage) fillImage.fillAmount = Mathf.Clamp01(currentTime / targetTime);
    }
}
