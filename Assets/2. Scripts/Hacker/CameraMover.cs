using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    public float moveDuration = 1.0f;

    private bool moving = false;
    private Coroutine moveCoroutine;

    public bool IsMoving => moving;

    public void MoveTo(Vector3 pos, Quaternion rot, float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(pos, rot, duration));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        moving = true;
        Transform cam = Camera.main.transform;
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