using UnityEngine;
using UnityEngine.UI;

public class GameExitButton : MonoBehaviour
{
    [SerializeField] private Button m_exitButton;

    private void Start()
    {
        if (m_exitButton != null)
            m_exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnExitClicked()
    {
        Debug.Log("게임 종료 버튼 클릭됨");

#if UNITY_EDITOR
        // 에디터에서 실행 중일 경우, 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드에서는 앱 종료
        Application.Quit();
#endif
    }
}
