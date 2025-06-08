using UnityEngine;
using System.Collections.Generic;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class MonsterMovement : MonoBehaviour
{
    public MonsterData data;
    public List<Transform> waypoints;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private float fixedY = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Y축 위치 고정
        Vector3 startPos = transform.position;
        startPos.y = fixedY;
        transform.position = startPos;
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPos = new Vector3(targetWaypoint.position.x, fixedY, targetWaypoint.position.z);

        float step = data.moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPos, step));

        // ✅ 회전 처리 (Y축 고정 회전)
        Vector3 lookDirection = targetPos - transform.position;
        lookDirection.y = 0f; // 고개를 숙이거나 들지 않게
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }

        float distance = (transform.position - targetPos).sqrMagnitude;
        if (distance < 0.01f)
        {
            currentWaypointIndex++;
        }
    }
}
