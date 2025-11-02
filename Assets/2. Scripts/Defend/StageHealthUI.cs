// 파일명: StageHealthUI.cs
using UnityEngine;
using TMPro;

public class StageHealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    private int _lastDisplayedHealth = -1;

    void Update()
    {
        if (GameStateManager.Instance == null || healthText == null) return;
        if (GameStateManager.Instance.Object == null)
        {
            return;
        }
        if (!GameStateManager.Instance.Object.IsValid) return;

        // 1. 네트워크 인스턴스에서 현재 체력을 가져옵니다.
        int currentHealth = GameStateManager.Instance.CurrentStageHealth;

        // 2. 체력이 변경되었을 때만 UI 텍스트를 업데이트합니다.
        if (currentHealth == _lastDisplayedHealth) return;
        
        _lastDisplayedHealth = currentHealth;
        healthText.text = $"남은 체력: {currentHealth}";

        if (currentHealth <= 5)
            healthText.color = Color.red;
        else
            healthText.color = Color.white;
    }
}