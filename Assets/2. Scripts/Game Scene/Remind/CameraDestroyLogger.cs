// CameraDestroyLogger.cs (DefendCamera에 붙일 스크립트)
using UnityEngine;

public class CameraDestroyLogger : MonoBehaviour
{
    void OnDestroy()
    {
        // 이 로그가 찍히는 순간, buildCamera도 TurretPlacer에게는 null이 됩니다.
        Debug.LogWarning($"!!!!!!!!!!! 'DefendCamera'가 파괴되었습니다. 현재 시간: {Time.time}", this);
        // 이 로그가 EndDrag()의 [RUNTIME DESTROY] 로그와 시간 차이가 얼마나 나는지 비교해보세요.
    }
}