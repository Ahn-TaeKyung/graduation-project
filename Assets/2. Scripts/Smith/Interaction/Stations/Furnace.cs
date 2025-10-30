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
        if (!Object.HasStateAuthority) return;

        // 넣기
        if (Stored == null)
        {
            if (p.hand.Held == null || p.hand.Held.type != inputType) return;

            var item = p.hand.Take();
            Stored = item.GetComponent<NetworkObject>();
            Timer = 0f;
            InUse = true;

            Transform t = slot ? slot : transform;
            item.transform.SetParent(t);
            item.transform.localPosition = Vector3.zero;
            item.gameObject.SetActive(false);

            RPC_SmeltingVisual(true, smeltTime);   // ← 모두에게 “시작” 알림
        }
        // 꺼내기
        else if (Timer >= smeltTime && p.hand.IsEmpty)
        {
            RPC_SmeltingVisual(false, 0f);

            var result = Runner.Spawn(outputPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, p.NetObj.InputAuthority);
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

    // 서버만 시간 잰다
    private void Update()
    {
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

    // === 여기서부터는 “모두”가 실행하는 부분 ===
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
}
