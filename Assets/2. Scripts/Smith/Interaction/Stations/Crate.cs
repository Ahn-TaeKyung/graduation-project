using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Spawn Settings")]
    [SerializeField] private Item orePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;          // ← 자식(Visual_box)의 Animator
    [SerializeField] private string openTrigger = "Open";

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(); // 자동 탐색
        if (!spawnPoint) spawnPoint = transform;                       // 비었으면 자기 위치
    }

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        hint = p.hand.IsEmpty ? "E - 광석 꺼내기" : "손이 비어야 함";
        return p.hand.IsEmpty;                                         // 기존 로직 유지  :contentReference[oaicite:1]{index=1}
    }

    public void OnTap(PlayerInteractor p)
    {
        if (orePrefab == null)
        {
            Debug.LogWarning("[Crate] Ore Prefab이 비어있습니다!");
            return;
        }

        // 애니메이션 트리거
        if (animator && !string.IsNullOrEmpty(openTrigger))
            animator.SetTrigger(openTrigger);

        // 기존 스폰 → 손에 Pick (그대로 유지)  :contentReference[oaicite:2]{index=2}
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position + Vector3.up * 1f;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
        var item = Instantiate(orePrefab, pos, rot);
        p.hand.Pick(item);
    }

    public void OnHoldComplete(PlayerInteractor p) { }
}
