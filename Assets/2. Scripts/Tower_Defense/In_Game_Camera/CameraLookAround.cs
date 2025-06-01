using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Update()
    {
        Vector2 lookDirection = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) lookDirection.y += 1;
        if (Keyboard.current.sKey.isPressed) lookDirection.y -= 1;
        if (Keyboard.current.aKey.isPressed) lookDirection.x -= 1;
        if (Keyboard.current.dKey.isPressed) lookDirection.x += 1;

        // 현재 각도 얻기 (EulerAngles는 0~360)
        Vector3 currentEuler = transform.eulerAngles;

        // X축 (상하) 각도 보정: 0~360 -> -180~180 범위로 변환
        float pitch = currentEuler.x;
        if (pitch > 180f) pitch -= 360f;

        // Y축 (좌우) 각도 보정
        float yaw = currentEuler.y;

        // 회전량 계산
        float pitchDelta = -lookDirection.y * rotationSpeed * Time.deltaTime;
        float yawDelta = lookDirection.x * rotationSpeed * Time.deltaTime;

        // 변경된 각도에 회전량 더하기
        pitch += pitchDelta;
        yaw += yawDelta;

        // 제한 범위 적용 (Clamp)
        pitch = Mathf.Clamp(pitch, 11f, 30f);

        if (yaw < 130f) yaw = 130f;
        else if (yaw > 220f) yaw = 220f;

        // 수정한 각도 적용
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
