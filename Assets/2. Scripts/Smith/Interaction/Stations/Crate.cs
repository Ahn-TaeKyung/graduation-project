using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Spawn Settings")]
    [SerializeField] private Item orePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;      // Visual_box의 Animator
    [SerializeField] private string openBool = "IsOpen"; // Animator Bool 파라미터

    private Item spawned; // 꺼내놓은 재료(있으면 열림 유지)

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spawnPoint) spawnPoint = transform;
        SetOpen(false);
    }

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (spawned == null)
        {
            // 아직 아무것도 꺼내지 않음 → 손이 비어야 꺼낼 수 있음
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 재료 꺼내놓기" : "손이 비어야 함";
            return ok;                                   // 기존: 손이 비어야 Tap 가능. :contentReference[oaicite:0]{index=0}
        }
        else
        {
            // 이미 재료가 놓여 있음 → 손이 비면 집을 수 있음
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 재료 집기" : "손이 비어야 함";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        if (spawned == null)
        {
            // 1) 꺼내놓기: SpawnPoint에 생성만 하고 손에는 안 줌
            if (orePrefab == null) { Debug.LogWarning("[Crate] Ore Prefab 비어있음"); return; }
            Vector3 pos = spawnPoint.position;
            Quaternion rot = spawnPoint.rotation;
            spawned = Instantiate(orePrefab, pos, rot);
            // 손에 바로 주던 기존 동작을 분리했음. :contentReference[oaicite:1]{index=1}
            SetOpen(true);
        }
        else
        {
            // 2) 집기: 손이 비어 있으면 꺼내놓은 걸 집고 닫기
            if (!p.hand.IsEmpty) return;
            p.hand.Pick(spawned);
            spawned = null;
            SetOpen(false);
        }
    }

    public void OnHoldComplete(PlayerInteractor p) { }

    private void SetOpen(bool open)
    {
        if (animator && !string.IsNullOrEmpty(openBool))
            animator.SetBool(openBool, open);
    }
}
