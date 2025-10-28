using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    public Hand hand;
    public PlayerProximityHighlighter proximity;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;   // 구 Input API 사용

    private IInteractable current;
    private float holdTimer;

    private void Update()
    {
        // 현재 하이라이트된 대상에서 IInteractable 뽑기
        var target = (proximity != null) ? proximity.Current : null;
        current = (target != null) ? target.GetComponent<IInteractable>() : null;

        // 상호작용 불가면 타이머 리셋
        if (current == null || !current.CanInteract(this, out _))
        {
            holdTimer = 0f;
            return;
        }

        // Tap / Hold 처리 (구 Input API)
        if (current.Kind == InteractionKind.Tap)
        {
            if (Input.GetKeyDown(interactKey))
                current.OnTap(this);
        }
        else // Hold
        {
            if (Input.GetKey(interactKey))
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= current.HoldDuration)
                {
                    current.OnHoldComplete(this);
                    holdTimer = 0f;
                }
            }
            if (Input.GetKeyUp(interactKey))
                holdTimer = 0f;
        }
    }
}