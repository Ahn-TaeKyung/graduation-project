using Fusion;
using UnityEngine;

public class Furnace : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform slot;
    [SerializeField] private float smeltTime = 3f;
    [SerializeField] private ItemType inputType = ItemType.Ore;
    [SerializeField] private NetworkObject outputPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem[] vfxOnSmelt;
    [SerializeField] private ProgressBarController progressBar;

    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private float Timer { get; set; }
    [Networked] private bool InUse { get; set; }

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (InUse && Timer < smeltTime)
        {
            hint = "용해 중...";
            return false;
        }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 넣기" : "";
            return ok;
        }
        else
        {
            bool ok = (Timer >= smeltTime) && p.hand.IsEmpty;
            hint = ok ? "E - 꺼내기" : "";
            return ok;
        }
    }

    public void OnTap(PlayerInteractor p)
    {
        // 권한 없는 쪽은 요청만 보내게 하려면 여기에서 RPC_RequestTap(...)을 부르면 되고,
        // 지금은 일단 단순하게 상태 권한 있는 쪽만 처리하게 둘게.
        if (!Object || !Object.HasStateAuthority) return;

        // 넣기
        if (Stored == null)
        {
            if (p.hand.Held == null || p.hand.Held.type != inputType) return;

            var item = p.hand.Take();
            var itemNet = item.GetComponent<NetworkObject>();
            Stored = itemNet;
            Timer = 0f;
            InUse = true;

            Transform t = slot ? slot : transform;
            item.transform.SetParent(t);
            item.transform.localPosition = Vector3.zero;

            //  여기서 모든 클라에 “이 아이템은 이제 손에서 뗀 거고 꺼라”라고 알림
            RPC_HideHeldItem(itemNet);

            RPC_SmeltingVisual(true, smeltTime);
        }
        // 꺼내기
        else if (Timer >= smeltTime && p.hand.IsEmpty)
        {
            RPC_SmeltingVisual(false, 0f);

            var result = Runner.Spawn(
                outputPrefab,
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity,
                p.NetObj.InputAuthority
            );
            p.hand.Pick(result.GetComponent<Item>());

            Runner.Despawn(Stored);
            Stored = null;
            Timer = 0f;
            InUse = false;
        }
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }

    private void Update()
    {
        if (!Object) return;
        if (!Object.HasStateAuthority) return;
        if (Stored == null) return;
        if (!InUse) return;

        Timer += Time.deltaTime;
        if (Timer >= smeltTime)
        {
            InUse = false;
            RPC_SmeltingVisual(false, 0f);
        }
    }

    // === 시각 효과 공용 RPC ===
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SmeltingVisual(bool on, float duration)
    {
        if (on)
        {
            if (progressBar) progressBar.StartProgress(duration);
            if (animator) animator.SetBool("IsSmelting", true);
            if (vfxOnSmelt != null)
            {
                foreach (var ps in vfxOnSmelt) if (ps) ps.Play();
            }
        }
        else
        {
            if (progressBar) progressBar.StopProgress();
            if (animator) animator.SetBool("IsSmelting", false);
            if (vfxOnSmelt != null)
            {
                foreach (var ps in vfxOnSmelt) if (ps) ps.Stop();
            }
        }
    }

    // === 여기서 문제 났던 RPC 추가 ===
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_HideHeldItem(NetworkObject itemNet)
    {
        if (!itemNet) return;

        // 모든 클라에서 이 아이템을 비활성화
        itemNet.transform.SetParent(null);
        itemNet.gameObject.SetActive(false);
    }
}
