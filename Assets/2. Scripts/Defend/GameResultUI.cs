// 파일명: GameResultUI.cs (수정본)
using UnityEngine;
using Fusion; // NetworkRunner 참조용
// using UnityEngine.SceneManagement; // 씬 관리자를 사용하지 않으므로 제거

public class GameResultUI : MonoBehaviour
{
    // [제거됨] 씬 이름 필드
    // [SerializeField] private string mapSelectSceneName = "MainScene";

    // "다시 시작" (스테이지 선택 맵으로) 버튼에 연결
    public void OnRestartButton()
    {
        // [핵심 수정] 씬을 변경하는 대신, GameStateManager에게 Ready 상태로 복귀하라고 RPC 요청
        if (GameStateManager.Instance != null)
        {
            Debug.Log("[GameResultUI] Ready 상태(맵 선택)로 복귀를 요청합니다.");
            
            // Host/Client 누구나 이 RPC를 호출할 수 있으며, Host가 상태를 변경할 것입니다.
            GameStateManager.Instance.RPC_ReturnToReady();
            
            // 이 UI 캔버스를 즉시 숨깁니다. (부모 캔버스)
            // GameStateManager가 Ready 상태가 되면 어차피 숨겨주지만,
            // 버튼 클릭 즉시 반응하는 것이 좋습니다.
            // transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameResultUI] GameStateManager.Instance를 찾을 수 없습니다.");
        }
    }

    // "게임 종료" 버튼에 연결 (이 로직은 동일)
    public void OnQuitButton()
    {
        Debug.Log("[GameResultUI] 게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}