using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    public float moveDuration = 1.0f;

    private bool moving = false;
    private Coroutine moveCoroutine;
    [HideInInspector]
    public static bool isMoving;

    public Vector3 DefaultCameraPosition;
    public Quaternion DefaultCameraRotation;


    private Camera hackerCamera;

    void Awake()
    {
        CameraMover.isMoving = moving;
        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }
        DefaultCameraPosition = hackerCamera.transform.position;
        DefaultCameraRotation = hackerCamera.transform.rotation;
    }
    public void MoveTo(Vector3 pos, Quaternion rot, float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(pos, rot, duration));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Transform cam = hackerCamera.transform;
        if (hackerCamera == null)
        {
            Debug.LogError("[CameraMover] Camera.main이 null입니다. Tag 설정 또는 카메라 활성화 여부를 확인하세요.");
            moving = false;
            yield break;
        }
        moving = true;
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.position = Vector3.Lerp(startPos, targetPos, t);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        cam.position = targetPos;
        cam.rotation = targetRot;
        moving = false;
    }
    public Coroutine MoveTo(Vector3 pos, Quaternion rot, float duration, System.Action onComplete = null)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(pos, rot, duration, onComplete));
        return moveCoroutine;
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, float duration, System.Action onComplete)
    {
        Transform cam = hackerCamera.transform;
        // ... 기존 코드 ...
        moving = true;
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.position = Vector3.Lerp(startPos, targetPos, t);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        cam.position = targetPos;
        cam.rotation = targetRot;

        moving = false;
        if (onComplete != null) onComplete();
    }
}