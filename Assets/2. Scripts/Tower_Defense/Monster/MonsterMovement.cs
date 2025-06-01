using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class MonsterMovement : MonoBehaviour
{
    public MonsterData data;
    public List<Transform> waypoints;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private float fixedY = 10.0f; // 몬스터가 땅 위에 떠 있도록 Y축 고정값 설정

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Y축 위치 보정 (spawn 시에도 적용되지만 안전하게 Start에서도 고정)
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

        float distance = (transform.position - targetPos).sqrMagnitude;
        if (distance < 0.01f)
        {
            currentWaypointIndex++;
        }
    }
}
