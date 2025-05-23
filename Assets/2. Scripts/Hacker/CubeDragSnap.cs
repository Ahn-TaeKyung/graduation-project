using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // ✅ 새 Input System 사용

public class CubeDragSnap : MonoBehaviour
{
    public static bool IsDragging = false;

    private bool dragging = false;
    private Vector2 prevMousePos;
    private Vector2 mouseDownPos;
    private float mouseDownTime;
    private const float dragThreshold = 5f;
    private const float clickDelay = 0.07f;
    int cubeGroupLayerMask;

    private Camera hackerCamera;

    void Awake()
    {
        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");

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

    void Update()
    {
        if (ModuleZoom.IsZoomed || hackerCamera == null) return;

        // 마우스 누르기
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseDownTime = Time.time;
            mouseDownPos = Mouse.current.position.ReadValue();
            prevMousePos = mouseDownPos;

            Ray ray = hackerCamera.ScreenPointToRay(mouseDownPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeGroupLayerMask))
            {
                if (hit.transform == this.transform)
                {
                    dragging = false;
                    IsDragging = false;
                }
                else
                {
                    dragging = false;
                    IsDragging = false;
                    mouseDownTime = -1000f;
                }
            }
            else
            {
                dragging = false;
                IsDragging = false;
                mouseDownTime = -1000f;
            }
        }

        // 드래그 중
        if (Mouse.current.leftButton.isPressed && mouseDownTime > 0)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentPos - prevMousePos;
            prevMousePos = currentPos;

            if (!dragging && (currentPos - mouseDownPos).magnitude > dragThreshold)
            {
                dragging = true;
                IsDragging = true;
            }

            if (dragging)
            {
                float rotSpeed = 0.1f;
                transform.Rotate(hackerCamera.transform.up, -mouseDelta.x * rotSpeed, Space.World);
                transform.Rotate(hackerCamera.transform.right, mouseDelta.y * rotSpeed, Space.World);
            }
        }

        // 마우스 떼기
        if (Mouse.current.leftButton.wasReleasedThisFrame && mouseDownTime > 0)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            float heldTime = Time.time - mouseDownTime;
            float totalMove = (currentPos - mouseDownPos).magnitude;

            if (!dragging && heldTime < clickDelay && totalMove < dragThreshold)
            {
                Ray ray = hackerCamera.ScreenPointToRay(currentPos);
                int moduleLayerMask = LayerMask.GetMask("Module");
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, moduleLayerMask))
                {
                    ModuleZoom mz = hit.transform.GetComponent<ModuleZoom>();
                    if (mz != null)
                        mz.OnModuleClickTrigger();
                }
            }

            dragging = false;
            IsDragging = false;
            mouseDownTime = -1000f;

            SnapToClosestFaceSmooth();
        }
    }

    void SnapToClosestFaceSmooth()
    {
        if (hackerCamera == null) return;

        Vector3 toCamera = (hackerCamera.transform.position - transform.position).normalized;
        Vector3[] normals = {
            transform.TransformDirection(Vector3.right),
            transform.TransformDirection(-Vector3.right),
            transform.TransformDirection(Vector3.up),
            transform.TransformDirection(-Vector3.up),
            transform.TransformDirection(Vector3.forward),
            transform.TransformDirection(-Vector3.forward)
        };

        float maxDot = -1f;
        int bestIdx = 0;
        for (int i = 0; i < normals.Length; i++)
        {
            float dot = Vector3.Dot(normals[i], toCamera);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIdx = i;
            }
        }

        Vector3 targetLocalNormal = Vector3.right;
        switch (bestIdx)
        {
            case 0: targetLocalNormal = Vector3.right; break;
            case 1: targetLocalNormal = -Vector3.right; break;
            case 2: targetLocalNormal = Vector3.up; break;
            case 3: targetLocalNormal = -Vector3.up; break;
            case 4: targetLocalNormal = Vector3.forward; break;
            case 5: targetLocalNormal = -Vector3.forward; break;
        }

        Quaternion targetRot = Quaternion.FromToRotation(
            transform.TransformDirection(targetLocalNormal), toCamera
        ) * transform.rotation;

        Vector3 euler = targetRot.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        Quaternion snappedRot = Quaternion.Euler(euler);

        StartCoroutine(SmoothSnapRotation(snappedRot, 0.5f));
    }

    IEnumerator SmoothSnapRotation(Quaternion target, float duration)
    {
        Quaternion startRot = transform.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, target, Mathf.Clamp01(t));
            yield return null;
        }
        transform.rotation = target;
    }
}
