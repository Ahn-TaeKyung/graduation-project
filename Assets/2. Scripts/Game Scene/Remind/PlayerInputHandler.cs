using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public static Vector2 MoveInput { get; private set; }
    public static bool DashPressed { get; private set; }

    void Update()
    {
        // 구 InputManager 사용
        float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
        MoveInput = new Vector2(h, v);

        // Shift 눌렀을 때 대시 입력
        DashPressed = Input.GetKeyDown(KeyCode.LeftShift);
    }
}
