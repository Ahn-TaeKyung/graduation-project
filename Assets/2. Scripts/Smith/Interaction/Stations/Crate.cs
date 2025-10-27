using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Spawn Settings")]
    [SerializeField] private Item orePrefab;
    [SerializeField] private Transform spawnPoint; // 📌 추가

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        hint = p.hand.IsEmpty ? "E - 광석 꺼내기" : "손이 비어야 함";
        return p.hand.IsEmpty;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (orePrefab == null)
        {
            Debug.LogWarning("[Crate] Ore Prefab이 비어있습니다!");
            return;
        }

        // spawnPoint 지정되어 있으면 그 위치에서 생성
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position + Vector3.up * 1f;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        var item = Instantiate(orePrefab, pos, rot);
        p.hand.Pick(item);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
}
