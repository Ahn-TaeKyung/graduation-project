using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput {
  public float horizontal;
  public float vertical;
  public NetworkBool dash;   // Fusion에선 NetworkBool 사용
}
