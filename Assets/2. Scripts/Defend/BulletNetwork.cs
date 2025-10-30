using Fusion;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private Vector3 direction;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f;

    public void Init(Vector3 dir)
    {
        direction = dir;
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += direction * speed * Runner.DeltaTime;
        lifeTime -= Runner.DeltaTime;
        if (lifeTime <= 0)
            Runner.Despawn(Object);
    }
}
