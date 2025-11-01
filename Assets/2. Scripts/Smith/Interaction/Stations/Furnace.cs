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
        // 권한 없으면 서버에 요청
        if (!Object || !Object.HasStateAuthority)
        {
            RPC_RequestTap(p.NetObj.InputAuthority);
            return;
        }

        HandleTap(p);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestTap(PlayerRef who)
    {
        var p = FindPlayerByRef(who);
        if (p == null) return;
        HandleTap(p);
    }

    private void HandleTap(PlayerInteractor p)
    {
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

            // 전체에 이 아이템 끄라고 알리기
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SmeltingVisual(bool on, float duration)
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideHeldItem(NetworkObject itemNet)
    {
        if (!itemNet) return;
        itemNet.transform.SetParent(null);
        itemNet.gameObject.SetActive(false);
    }

    private PlayerInteractor FindPlayerByRef(PlayerRef who)
    {
        var all = UnityEngine.Object.FindObjectsByType<PlayerInteractor>(UnityEngine.FindObjectsSortMode.None);
        foreach (var pi in all)
        {
            if (pi.Object != null && pi.Object.InputAuthority == who)
                return pi;
        }
        return null;
    }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
    public void OnHoldComplete(PlayerInteractor p) { }

}
