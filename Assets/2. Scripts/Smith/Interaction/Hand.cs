using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    public Item Held { get; private set; }
    public bool IsEmpty => Held == null;

    public void Pick(Item item)
    {
        Held = item;
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;
    }

    public Item Drop(Transform parent = null)
    {
        if (Held == null) return null;
        var it = Held;
        Held = null;
        it.transform.SetParent(parent);
        if (it.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
        if (it.TryGetComponent(out Collider col)) col.enabled = true;
        return it;
    }

    public Item Take()
    {
        var it = Held;
        Held = null;
        return it;
    }
}
