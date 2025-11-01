// 파일명: GameTimerUI.cs (Polling 방식 최종본)
using UnityEngine;
using TMPro; // TextMeshPro 사용 시
using System; // TimeSpan 사용 시

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    // 이 UI가 마지막으로 표시한 시간을 기억 (불필요한 업데이트 방지)
    private float _lastDisplayedTime = -1f;

    // 매 프레임(Render Thread)마다 실행
    private void Update()
    {
        // [핵심 수정] GameStateManager가 완전히 스폰되었는지 3단계로 확인합니다.
        
        // 1. GameStateManager.Instance (MonoBehaviour)가 Awake()를 호출했는지 확인
        if (GameStateManager.Instance == null || timerText == null)
        {
            return;
        }

        // 2. GameStateManager.Object (NetworkObject)가 Spawned()에 의해 할당되었는지 확인
        //    (이것이 NullReferenceException의 원인이었습니다)
        if (GameStateManager.Instance.Object == null)
        {
            return;
        }

        // 3. NetworkObject가 Fusion 시뮬레이션에서 유효한지(Valid) 확인
        //    (이것이 InvalidOperationException의 원인이었습니다)
        if (!GameStateManager.Instance.Object.IsValid)
        {
            return;
        }

        // --- 이 라인 아래는 GameStateManager의 [Networked] 속성에 안전하게 접근할 수 있습니다 ---

        // 1. 네트워크 인스턴스에서 현재 타이머 값을 직접 가져옵니다.
        float remainingTime = GameStateManager.Instance.SharedGameTimer;

        // 2. 시간이 변경되었을 때만 UI 텍스트를 업데이트합니다.
        if (Mathf.Approximately(remainingTime, _lastDisplayedTime))
        {
            return; // 변경 사항 없음
        }

        // 3. 시간이 변경되었으므로 UI 업데이트
        _lastDisplayedTime = remainingTime;
        
        if (remainingTime < 0) remainingTime = 0;
        
        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
        timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        // (선택 사항) 시간이 10초 미만일 때 색상 변경
        if (remainingTime <= 10.0f && remainingTime > 0)
        {
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }
    }
}