using Fusion;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    [SerializeField] private Transform holdPoint;

    // 네트워크로 공유
    [Networked]
    public NetworkObject HeldNet { get; set; }
    public Item Held => HeldNet ? HeldNet.GetComponent<Item>() : null;
    public bool IsEmpty => HeldNet == null;

    // Item을 들기
    public void Pick(Item item)
    {
        if (item == null) return;
        if (!Object.HasStateAuthority) return;

        var no = item.GetComponent<NetworkObject>();
        if (no == null)
        {
            Debug.LogWarning("[Hand] NetworkObject 없는 아이템을 들려고 함");
            return;
        }

        HeldNet = no;
    }

    // 손 비우기
    public Item Drop(Transform newParent = null)
    {
        if (!Object.HasStateAuthority) return null;
        if (HeldNet == null) return null;

        var item = HeldNet.GetComponent<Item>();
        HeldNet = null;     // 네트워크에서 손 비우기

        // 부모 바꿔줄 거면 여기서
        if (item != null && newParent != null)
        {
            item.transform.SetParent(newParent);
            if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
            if (item.TryGetComponent(out Collider col)) col.enabled = true;
        }

        return item;
    }

    public Item Take()
    {
        if (!Object.HasStateAuthority) return null;
        if (HeldNet == null) return null;

        var item = HeldNet.GetComponent<Item>();
        HeldNet = null;
        return item;
    }

    //  콜백이 안 되니까 매 프레임 한 번 “손에 붙어야 하니?” 확인해서 붙여줌
    private void LateUpdate()
    {
        // 네트워크상으로 누가 Pick 하면 이 값이 바뀌어 있음
        if (HeldNet != null)
        {
            var t = HeldNet.transform;
            t.SetParent(holdPoint);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            if (t.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (t.TryGetComponent(out Collider col)) col.enabled = false;
        }
    }
}
