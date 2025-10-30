using Fusion;
using UnityEngine;

public class Furnace : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform slot;     // 없어도 됨 (null 허용)
    [SerializeField] private float smeltTime = 3f;
    [SerializeField] private ItemType inputType = ItemType.Ore;
    [SerializeField] private NetworkObject outputPrefab;

    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem[] vfxOnSmelt;
    [SerializeField] private ProgressBarController progressBar;

    [Networked] private NetworkObject Stored { get; set; }
    [Networked] private bool InUse { get; set; }

    private float timer;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (InUse) { hint = ""; return false; }

        if (Stored == null)
        {
            bool ok = p.hand.Held && p.hand.Held.type == inputType;
            hint = ok ? "E - 화로에 넣기" : "";
            return ok;
        }
        else if (timer >= smeltTime)
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 주조물 꺼내기" : "";
            return ok;
        }

        hint = "용해 중...";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (!Object.HasStateAuthority) return;
        if (InUse) return;

        // 넣기
        if (Stored == null)
        {
            if (p.hand.Held == null || p.hand.Held.type != inputType) return;
            var item = p.hand.Take();
            Stored = item.GetComponent<NetworkObject>();
            timer = 0f;
            InUse = true;

            // 🔹 slot이 없으면 그냥 화로 Transform 기준
            Transform target = slot ? slot : transform;
            item.transform.SetParent(target);
            item.transform.localPosition = Vector3.zero;
            item.gameObject.SetActive(false);

            SetSmelting(true);
            if (progressBar) progressBar.StartProgress(smeltTime);
        }
        // 꺼내기
        else if (timer >= smeltTime && p.hand.IsEmpty)
        {
            if (progressBar) progressBar.StopProgress();

            var result = Runner.Spawn(outputPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, p.NetObj.InputAuthority);
            p.hand.Pick(result.GetComponent<Item>());

            Runner.Despawn(Stored);
            Stored = null;
            timer = 0f;
            InUse = false;
            SetSmelting(false);
        }
    }

    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }

    private void Update()
    {
        if (!Object.HasStateAuthority) return;
        if (Stored == null || timer >= smeltTime) return;

        timer += Time.deltaTime;

        if (timer >= smeltTime)
        {
            SetSmelting(false);
            if (progressBar) progressBar.StopProgress();
            InUse = false;
        }
    }

    private void SetSmelting(bool on)
    {
        if (animator) animator.SetBool("IsSmelting", on);
        if (vfxOnSmelt != null)
        {
            foreach (var ps in vfxOnSmelt)
            {
                if (!ps) continue;
                if (on) { if (!ps.isPlaying) ps.Play(); }
                else { if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
            }
        }
    }
}
