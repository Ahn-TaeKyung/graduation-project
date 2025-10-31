using Fusion;
using UnityEngine;

public class PlayerInteractor : NetworkBehaviour
{
    [Header("References")]
    public Hand hand;
    public PlayerProximityHighlighter proximity;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    private IInteractable current;
    private float holdTimer;
    private bool isHolding;

    public NetworkObject NetObj { get; private set; }

    private void Awake()
    {
        NetObj = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        // 이미 홀드 중이면 새로운 대상 찾지 말고, 지금 들고 있는 걸로만 진행
        if (isHolding)
        {
            // 키를 계속 누르고 있으면 타이머 증가
            if (Input.GetKey(interactKey) && current != null)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= current.HoldDuration)
                {
                    current.OnHoldComplete(this);
                    holdTimer = 0f;
                    isHolding = false;
                }
            }
            // 키 뗐으면 취소
            else if (Input.GetKeyUp(interactKey) && current != null)
            {
                current.OnHoldCancel(this);
                holdTimer = 0f;
                isHolding = false;
            }

            return; // ← 여기서 끝! 이 프레임엔 CanInteract 다시 안 묻는다
        }

        // 여기부터는 홀드 안 하는 평상시 로직
        var target = (proximity != null) ? proximity.Current : null;
        current = (target != null) ? target.GetComponent<IInteractable>() : null;

        // 상호작용 불가면 리셋
        if (current == null || !current.CanInteract(this, out _))
        {
            holdTimer = 0f;
            isHolding = false;
            return;
        }

        if (current.Kind == InteractionKind.Tap)
        {
            if (Input.GetKeyDown(interactKey))
                current.OnTap(this);
        }
        else
        {
            if (Input.GetKeyDown(interactKey))
            {
                current.OnHoldStart(this);
                holdTimer = 0f;
                isHolding = true;
            }
        }
    }
}
