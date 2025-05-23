using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    public float moveDuration = 1.0f;

    private bool moving = false;
    private Coroutine moveCoroutine;

    public bool IsMoving => moving;

    private Camera hackerCamera;

    void Awake()
    {
        GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
        if (hackerCameraObject != null)
        {
            hackerCamera = hackerCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("태그가 'hacker'인 카메라를 찾을 수 없습니다.");
        }
    }

    public void MoveTo(Vector3 pos, Quaternion rot, float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(pos, rot, duration));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        if (hackerCamera == null)
        {
            Debug.LogError("hackerCamera가 설정되지 않았습니다.");
            yield break;
        }

        moving = true;
        Transform cam = hackerCamera.transform;
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
}
