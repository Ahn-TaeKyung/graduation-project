using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Spawn Settings")]
    [SerializeField] private Item orePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;                 // Visual_box의 Animator
    [SerializeField] private string openTrigger = "Open"; // Animator Trigger 파라미터

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spawnPoint) spawnPoint = transform;
    }

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool ok = p.hand.IsEmpty; // 손이 비어 있어야만 꺼낼 수 있음
        hint = ok ? "E - 재료 받기" : "손이 비어야 함";
        return ok;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (!p.hand.IsEmpty) return;
        if (!orePrefab)
        {
            Debug.LogWarning("[Crate] Ore Prefab 비어있음");
            return;
        }

        // 1) 재료 생성
        var item = Instantiate(orePrefab, spawnPoint.position, spawnPoint.rotation);

        // 2) 손에 바로 들려주기
        p.hand.Pick(item);

        // 3) 매번 열기 애니메이션 재생
        PlayOpen();
    }

    public void OnHoldComplete(PlayerInteractor p) { }

    private void PlayOpen()
    {
        if (!animator || string.IsNullOrEmpty(openTrigger)) return;
        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(openTrigger);
    }

    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
}
