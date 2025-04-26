using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(NetworkCharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    private NetworkCharacterController m_character_controller;
    private PlayerInput m_player_input;
    private Vector2 m_move_input;
    [SerializeField] private float m_move_speed = 5f;

    public override void Spawned()
    {
        InitializeComponents();
        if(Object.HasInputAuthority)
        {
            SetupInput();
        }
        else
        {
            Debug.Log("InputAuthority가 아직 할당되지 않았습니다.");
        }
    }
    private void InitializeComponents()
    {
        m_character_controller = GetComponent<NetworkCharacterController>();
        m_player_input = GetComponent<PlayerInput>();
    }
    private void SetupInput()
    {
        Debug.Log("여기까지는 정상이야?");
        if (Object.HasInputAuthority)
        {
            Debug.Log("내가 조종하는 플레이어");

            m_player_input.enabled = true;

            // 1. 현재 사용자가 유효하지 않거나 잘못된 경우 리바인딩
            if (m_player_input.user == null || !m_player_input.user.valid)
            {
                Debug.Log(">> InputUser 재설정 시도 중...");

                // 현재 키보드 장치가 있다면
                if (Keyboard.current != null)
                {
                    // PlayerInput에 직접 디바이스를 세팅
                    var devices = new InputDevice[] { Keyboard.current, Mouse.current };

                    // 현재 연결된 User가 없으면 수동으로 할당
                    InputUser user = InputUser.PerformPairingWithDevice(Keyboard.current);
                    user.AssociateActionsWithUser(m_player_input.actions);

                    // 현재 PlayerInput이 사용하는 ControlScheme을 수동 변경
                    m_player_input.SwitchCurrentControlScheme(devices);

                    // 그리고 사용하려는 ActionMap으로 전환
                    m_player_input.SwitchCurrentActionMap("Player");

                    Debug.Log(">> Input 재설정 완료");
                }
                else
                {
                    Debug.LogWarning("현재 키보드 장치를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.Log(">> 기존 유저로 Input 연결 완료");
                // 기존 유저가 유효하면 그냥 ActionMap만 확인
                if (m_player_input.currentActionMap.name != "Player")
                {
                    m_player_input.SwitchCurrentActionMap("Player");
                }
            }
        }
        else
        {
            // Input 권한 없는 객체는 InputSystem 꺼버리기
            m_player_input.enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            Vector3 move = new Vector3(m_move_input.x, 0, m_move_input.y);

            if (move != Vector3.zero)
                move = move.normalized * m_move_speed;

            m_character_controller.Move(move * Runner.DeltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (Object.HasInputAuthority)
            m_move_input = context.ReadValue<Vector2>();
    }
}
