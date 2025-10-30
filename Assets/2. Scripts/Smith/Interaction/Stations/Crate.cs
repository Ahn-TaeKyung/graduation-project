using Fusion;
using UnityEngine;

public class Crate : NetworkBehaviour, IInteractable
{
    [Header("Spawn Settings")]
    [SerializeField] private NetworkObject orePrefab;   // Item → NetworkObject
    [SerializeField] private Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spawnPoint) spawnPoint = transform;
    }

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        bool ok = p.hand.IsEmpty;
        hint = ok ? "E - 재료 받기" : "손이 비어야 함";
        return ok;
    }

    public void OnTap(PlayerInteractor p)
    {
        // 손 안 비었으면 X
        if (!p.hand.IsEmpty) return;
        if (!orePrefab)
        {
            Debug.LogWarning("[Crate] Ore Prefab 비어있음");
            return;
        }

        // 서버/호스트만 스폰
        if (!Object.HasStateAuthority) return;

        var runner = NetworkManager.Instance.m_network_runner;

        // 1) 네트워크로 아이템 생성
        var spawned = runner.Spawn(
            orePrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            p.NetObj.InputAuthority   
        );

        // 2) 손에 들려주기
        var item = spawned.GetComponent<Item>();
        if (item != null)
            p.hand.Pick(item);

        // 3) 모든 클라에서 열기 애니메이션 재생
        RPC_PlayOpen();
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }

    // === 애니메이션 전체에게 날리기 ===
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayOpen()
    {
        PlayOpen();
    }

    private void PlayOpen()
    {
        if (!animator || string.IsNullOrEmpty(openTrigger)) return;
        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(openTrigger);
    }
}
