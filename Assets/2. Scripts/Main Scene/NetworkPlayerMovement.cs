using Fusion;
using UnityEngine;

public class NetworkPlayerMovement : NetworkBehaviour
{
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log("내가 조종하는 플레이어");
        }
    }
}
