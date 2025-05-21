using System.Collections;
using UnityEngine;

public class CubeDragSnap : MonoBehaviour
{
    public static bool IsDragging = false;

    private bool dragging = false;
    private Vector3 prevMousePos;
    private Vector3 mouseDownPos;
    private float mouseDownTime;
    private const float dragThreshold = 5f;
    private const float clickDelay = 0.07f;
    int cubeGroupLayerMask;

    void Awake()
    {
        cubeGroupLayerMask = LayerMask.GetMask("CubeGroup");
    }

    void Update()
    {
        // 확대(줌) 중이면 입력(회전/드래그) 완전 차단!
        if (ModuleZoom.IsZoomed) return;

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownTime = Time.time;
            mouseDownPos = Input.mousePosition;
            prevMousePos = mouseDownPos;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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

        if (Input.GetMouseButton(0) && mouseDownTime > 0)
        {
            Vector3 mouseDelta = Input.mousePosition - prevMousePos;
            prevMousePos = Input.mousePosition;

            if (!dragging && (Input.mousePosition - mouseDownPos).magnitude > dragThreshold)
            {
                dragging = true;
                IsDragging = true;
            }

            if (dragging)
            {
                float rotSpeed = 0.1f;
                transform.Rotate(Camera.main.transform.up, -mouseDelta.x * rotSpeed, Space.World);
                transform.Rotate(Camera.main.transform.right, mouseDelta.y * rotSpeed, Space.World);
            }
        }

        if (Input.GetMouseButtonUp(0) && mouseDownTime > 0)
        {
            float heldTime = Time.time - mouseDownTime;
            float totalMove = (Input.mousePosition - mouseDownPos).magnitude;

            if (!dragging && heldTime < clickDelay && totalMove < dragThreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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

            // ★ 드래그 종료 후 스냅(정렬)
            SnapToClosestFaceSmooth();
        }
    }

    // 스냅(카메라를 바라보는 면을 90도 단위로 정렬)
    void SnapToClosestFaceSmooth()
    {
        Vector3 toCamera = (Camera.main.transform.position - transform.position).normalized;
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
