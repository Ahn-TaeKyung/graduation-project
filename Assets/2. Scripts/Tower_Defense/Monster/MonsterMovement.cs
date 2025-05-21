using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterMovement : MonoBehaviour
{
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    public float moveSpeed = 3f;

    private Rigidbody rb;
    private float fixedY; // 고정 Y 값

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fixedY = transform.position.y;
    }

    void FixedUpdate() // Rigidbody는 FixedUpdate에서 움직여야 함!!
    {
        if (currentWaypointIndex < waypoints.Count)
        {
            MoveToNextWaypoint();
        }
        // else
        // {
        //     //Destroy(gameObject);
        // }
    }

    void MoveToNextWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        Vector3 targetPosition = new Vector3(
            targetWaypoint.position.x,
            fixedY,
            targetWaypoint.position.z
        );

        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 moveVector = direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + moveVector);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }
}
