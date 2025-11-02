using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public static Vector2 MoveInput { get; private set; }
    public static bool DashPressed { get; private set; }

    // 대시 입력을 잠깐 들고 있는 버퍼
    static bool _dashBuffer;
    static float _dashBufferTimer;
    const float DashBufferTime = 0.1f;
    void Update()
    {
        // 구 InputManager 사용
        float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
        MoveInput = new Vector2(h, v);

        // 대시 키 눌리면 버퍼 ON
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _dashBuffer = true;
            _dashBufferTimer = DashBufferTime;
        }

        // 버퍼 시간 깎기
        if (_dashBuffer)
        {
            _dashBufferTimer -= Time.deltaTime;
            if (_dashBufferTimer <= 0f)
                _dashBuffer = false;
        }

        // 네트워크로 넘길 값
        DashPressed = _dashBuffer;
    }

    // (원하면) 네트워크에서 소비한 뒤에 불러주는 메서드
    public static void ConsumeDash()
    {
        _dashBuffer = false;
        DashPressed = false;
    }
}
