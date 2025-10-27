using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public static Vector2 MoveInput { get; private set; }

    void Update()
    {
        // 테스트 확실히 하려고 구 InputManager로 고정
        float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
        MoveInput = new Vector2(h, v);
        // 임시 로그: 키를 누르면 반드시 찍혀야 함
       // if (MoveInput.sqrMagnitude > 0.01f) Debug.Log($"[MoveInput] {MoveInput}");
    }
}
