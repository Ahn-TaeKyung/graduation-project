using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraSwitchController : NetworkBehaviour, IGameReadyListener, IGameEndListener
{
    private Camera buildCamera;
    private Camera defendCamera;

    private bool isDefendView = false;
    private bool isCameraControlEnabled = false;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        // 씬 내 카메라를 찾아서 참조
        buildCamera = GameObject.Find("BuildCamera")?.GetComponent<Camera>();
        defendCamera = GameObject.Find("DefendCamera")?.GetComponent<Camera>();

        if (buildCamera == null || defendCamera == null)
        {
            if (defendCamera == null)
            {
                Debug.LogError("[CameraSwitch] DefendCamera를 찾을 수 없습니다. 이름 확인 필요!");
            }
            if (buildCamera == null)
            {
                Debug.LogError("[CameraSwitch] BuildCamera를 찾을 수 없습니다. 이름 확인 필요!");
            }
            return;
        }

        // 초기 카메라 상태
        buildCamera.enabled = true;
        defendCamera.enabled = false;

        // GameStateManager 리스너 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener((IGameReadyListener)this);
            GameStateManager.Instance.RegisterListener((IGameEndListener)this);
            Debug.Log("[CameraSwitch] GameStateManager에 리스너 등록 완료");
            if (GameStateManager.Instance.CurrentState == GameState.Ready)
            {
                Debug.Log("[CameraSwitch] Spawn 후 이미 Ready 상태임, OnGameReady 직접 호출");
                OnGameReady();
            }
        }
        else
        {
            Debug.LogWarning("[CameraSwitch] GameStateManager.Instance가 아직 없음");
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterListener((IGameReadyListener)this);
            GameStateManager.Instance.UnregisterListener((IGameEndListener)this);
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
        defendCamera.enabled = defend;

        Debug.Log($"[CameraSwitch] {(defend ? "Defend Mode" : "Build Mode")} 활성화됨");
    }

    public void OnGameReady()
    {
        if (!Object.HasInputAuthority)
            return;

        isCameraControlEnabled = true;
        isDefendView = false;

        if (buildCamera != null && defendCamera != null)
        {
            buildCamera.enabled = true;
            defendCamera.enabled = false;
        }

        Debug.Log("[CameraSwitch] Game Ready - BuildCamera 활성화됨");
    }

    public void OnGameEnd()
    {
        if (!Object.HasInputAuthority)
            return;

        isCameraControlEnabled = false;
        Debug.Log("[CameraSwitch] Game End - 카메라 전환 비활성화");
    }
}
