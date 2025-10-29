using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    public Hand hand;
    public PlayerProximityHighlighter proximity;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    private IInteractable current;
    private float holdTimer;
    private bool isHolding;

    private void Update()
    {
        var target = (proximity != null) ? proximity.Current : null;
        current = (target != null) ? target.GetComponent<IInteractable>() : null;

        // 상호작용 불가면 리셋
        if (current == null || !current.CanInteract(this, out _))
        {
            if (isHolding) current?.OnHoldCancel(this);
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

            if (Input.GetKey(interactKey) && isHolding)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= current.HoldDuration)
                {
                    current.OnHoldComplete(this);
                    holdTimer = 0f;
                    isHolding = false;
                }
            }

            if (Input.GetKeyUp(interactKey) && isHolding)
            {
                current.OnHoldCancel(this);
                holdTimer = 0f;
                isHolding = false;
            }
        }
    }
}
