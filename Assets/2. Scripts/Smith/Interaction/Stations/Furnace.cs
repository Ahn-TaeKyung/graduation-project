using UnityEngine;

public class Furnace : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform slot;
    [SerializeField] private float smeltTime = 3f;
    [SerializeField] private ItemType inputType = ItemType.Ore;
    [SerializeField] private Item outputPrefab;

    //  애니메이션 / VFX
    [SerializeField] private Animator animator;              
    [SerializeField] private ParticleSystem[] vfxOnSmelt;     // optional

    //  진행바(UI)
    [Header("UI")]
    [SerializeField] private ProgressBarController progressBar;   // World Space Canvas에 붙은 컨트롤러

    private Item stored;
    private float timer;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldDuration => 0f;

    // 선택: 프리팹이 켜져 있어도 시작 시 진행바를 자동으로 숨김
    private void Awake()
    {
        if (progressBar) progressBar.gameObject.SetActive(false);
    }

    public bool CanInteract(PlayerInteractor p, out string hint)
    {
        if (stored == null)
        {
            bool ok = (p.hand.Held && p.hand.Held.type == inputType);
            hint = ok ? "E - 화로에 넣기" : "광석 필요";
            return ok;
        }
        else if (timer >= smeltTime)
        {
            bool ok = p.hand.IsEmpty;
            hint = ok ? "E - 주조물 꺼내기" : "손이 비어야 함";
            return ok;
        }

        hint = "용해 중...";
        return false;
    }

    public void OnTap(PlayerInteractor p)
    {
        if (stored == null)
        {
            // 넣기
            if (p.hand.Held == null || p.hand.Held.type != inputType) return;

            stored = p.hand.Take();
            stored.transform.SetParent(slot);
            stored.transform.localPosition = Vector3.zero;
            stored.gameObject.SetActive(false);
            timer = 0f;

            //  애니메이션/이펙트 ON
            SetSmelting(true);
            //  진행바 시작
            if (progressBar) progressBar.StartProgress(smeltTime);
        }
        else if (timer >= smeltTime && p.hand.IsEmpty)
        {
            // 꺼내기
            var outItem = Instantiate(outputPrefab);
            p.hand.Pick(outItem);
            Destroy(stored.gameObject);
            stored = null;
            timer = 0f;

            //  안전하게 OFF
            SetSmelting(false);
            //  진행바 정지/숨김
            if (progressBar) progressBar.StopProgress();
        }
    }

    private void Update()
    {
        if (stored != null && timer < smeltTime)
        {
            timer += Time.deltaTime;

            // 제련 완료 시점
            if (timer >= smeltTime)
            {
                //  애니메이션/이펙트 OFF
                SetSmelting(false);
                //  진행바 정지/숨김
                if (progressBar) progressBar.StopProgress();
            }
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
                else    { if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
            }
        }
    }

    // Hold 기반 아님: 인터페이스 충족용
    public void OnHoldComplete(PlayerInteractor p) { }
    public void OnHoldStart(PlayerInteractor p) { }
    public void OnHoldCancel(PlayerInteractor p) { }
}
