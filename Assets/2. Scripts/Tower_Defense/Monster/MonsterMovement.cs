using UnityEngine;
using System.Collections.Generic;
using Fusion;
//using System.Numerics;

[RequireComponent(typeof(Rigidbody))]
public class MonsterMovement : MonoBehaviour
{
    public MonsterData data;
    public List<Transform> waypoints;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private float fixedY = 1.5f; // 몬스터가 땅 위에 떠 있도록 Y축 고정값 설정

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

        UnityEngine.Quaternion temp = transform.rotation;
        temp.y = temp.y + 90;
        transform.rotation = temp;

        Vector3 lookDirection = targetPos - transform.position;
        if (lookDirection != Vector3.zero)
        {
            
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Debug.Log($"{targetRotation} / {lookDirection}");
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }

        float distance = (transform.position - targetPos).sqrMagnitude;
        if (distance < 0.01f)
        {
            currentWaypointIndex++;
        }
    }
}
