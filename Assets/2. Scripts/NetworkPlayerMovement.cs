using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    private NetworkCharacterController m_character_controller;
    private Vector2 m_move_input;
    public float m_move_speed = 5f;

    public override void Spawned()
    {
        m_character_controller = GetComponent<NetworkCharacterController>();

        if (Object.HasInputAuthority)
        {
            Debug.Log("내가 조종하는 플레이어");
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
        m_move_input = context.ReadValue<Vector2>();
    }
}
