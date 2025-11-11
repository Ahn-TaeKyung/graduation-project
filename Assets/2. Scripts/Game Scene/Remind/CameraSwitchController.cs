// 파일명: CameraSwitchController.cs (Spawned 함수 수정)
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitchController : NetworkBehaviour, IGameReadyListener, IGameEndListener, IGameStartListener
{
    private Camera buildCamera;
    private Camera defendCamera;
    private Canvas towerBox; // TowerCanvas 참조

    private bool isDefendView = false;
    private bool isCameraControlEnabled = true;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        // 씬 내 카메라를 찾아서 참조
        buildCamera = GameObject.Find("SmithCamera")?.GetComponent<Camera>();
        defendCamera = GameObject.Find("DefendCamera")?.GetComponent<Camera>();
        towerBox = GameObject.Find("TowerCanvas")?.GetComponent<Canvas>();
        
        if (buildCamera == null || defendCamera == null)
        {
            Debug.LogError("[CameraSwitch] BuildCamera 또는 DefendCamera를 찾을 수 없습니다.");
            return;
        }

        // GameStateManager 리스너 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener((IGameReadyListener)this);
            GameStateManager.Instance.RegisterListener((IGameEndListener)this);
            GameStateManager.Instance.RegisterListener((IGameStartListener)this);
            Debug.Log("[CameraSwitch] GameStateManager에 리스너 등록 완료");
        }
        else
        {
            Debug.LogWarning("[CameraSwitch] GameStateManager.Instance가 아직 없음");
        }
        
        // [핵심 수정] "이미 Ready 상태라면 직접 호출" 로직 제거
        // GameStateManager의 FixedUpdateNetwork가 이 역할을 대신합니다.
        
        // [수정] 초기 카메라는 GameStateManager가 Ready 상태가 될 때 OnGameReady에서 설정
        // buildCamera.enabled = true;
        // defendCamera.enabled = false;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null && Object.HasInputAuthority) // HasInputAuthority 체크 추가
        {
            GameStateManager.Instance.UnregisterListener((IGameReadyListener)this);
            GameStateManager.Instance.UnregisterListener((IGameEndListener)this);
            GameStateManager.Instance.UnregisterListener((IGameStartListener)this);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log($"[CameraSwitch] Tab 입력 감지됨 - 권한:{Object.HasInputAuthority}, 카메라활성:{isCameraControlEnabled}");
        }

        if (!Object.HasInputAuthority || !isCameraControlEnabled)
            return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("[CameraSwitch] Tab 눌림 감지됨");
            isDefendView = !isDefendView;
            SwitchCamera(isDefendView);
        }
    }

    private void SwitchCamera(bool defend)
    {
        if (buildCamera == null || defendCamera == null)
            return;

        buildCamera.enabled = !defend;
        if (towerBox != null) towerBox.enabled = defend; // towerBox null 체크
        defendCamera.enabled = defend;

        Debug.Log($"[CameraSwitch] {(defend ? "Defend Mode" : "Build Mode")} 활성화됨");
    }

    // Ready 상태 (맵 선택)
    public void OnGameReady()
    {
        if (!Object.HasInputAuthority)
            return;

        // [핵심 수정] Ready 상태에서 탭 전환을 허용합니다. (당신의 의도)
        isCameraControlEnabled = true; 
        
        SwitchCamera(false); // 기본 카메라인 BuildCamera(SmithCamera)로 설정
        Debug.Log("[CameraSwitch] Game Ready - BuildCamera 활성화됨, 탭 전환 허용");
    }
    
    // 'Start' 상태 (30초 준비)
    public void OnGameStart()
    {
        if (!Object.HasInputAuthority) return;
        
        isCameraControlEnabled = true; // 이미 true이지만, 확실하게
        
        // Start 상태가 되면 DefendCamera로 자동 전환
        SwitchCamera(true); 
    }
    
    // 'End' 또는 'Clear' 상태
    public void OnGameEnd()
    {
        if (!Object.HasInputAuthority)
            return;

        isCameraControlEnabled = false; // 탭 전환 비활성화
        Debug.Log("[CameraSwitch] Game End - 카메라 전환 비활성화");
    }
}