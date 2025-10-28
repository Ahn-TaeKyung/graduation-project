using UnityEngine;

public class Furnace : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform slot;
    [SerializeField] private float smeltTime = 3f;
    [SerializeField] private ItemType inputType = ItemType.Ore;
    [SerializeField] private Item outputPrefab;

    // ★ 추가: 애니메이터(외형 쪽, Forge 모델에 달린 Animator)
    [SerializeField] private Animator animator; // 파라미터: Bool "IsSmelting"
    // (선택) 불/연기 VFX 제어하고 싶다면:
    [SerializeField] private ParticleSystem[] vfxOnSmelt;

    private Item stored;
    private float timer;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            hint = (p.hand.Held && p.hand.Held.type == inputType) ? "E - 화로에 넣기" : "광석 필요";
            return (p.hand.Held && p.hand.Held.type == inputType);
        }
        else if (timer >= smeltTime)
        {
            hint = p.hand.IsEmpty ? "E - 주조물 꺼내기" : "손이 비어야 함";
            return p.hand.IsEmpty;
        }
        hint = "용해 중...";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (stored == null)
        {
            // 넣기
            stored = p.hand.Take();
            stored.transform.SetParent(slot);
            stored.transform.localPosition = Vector3.zero;
            stored.gameObject.SetActive(false);
            timer = 0f;

            // ★ 제련 시작 → 애니메이션 ON
            SetSmelting(true);
        }
        else if (timer >= smeltTime && p.hand.IsEmpty)
        {
            // 꺼내기
            var outItem = Instantiate(outputPrefab);
            p.hand.Pick(outItem);
            Destroy(stored.gameObject);
            stored = null;
            timer = 0f;
            // 꺼낼 땐 이미 OFF여도 무방. 안전하게 꺼둠
            SetSmelting(false);
        }
    }

    private void Update()
    {
        if (stored != null && timer < smeltTime)
        {
            timer += Time.deltaTime;
            // ★ 제련 완료 시점에서 애니메이션 OFF
            if (timer >= smeltTime)
                SetSmelting(false);
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
                else    { if (ps.isPlaying)  ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
            }
        }
    }

    public void OnHoldComplete(PlayerInteractor p) { }
}
