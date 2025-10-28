public enum InteractionKind { Tap, Hold }

public interface IInteractable
{
    bool CanInteract(PlayerInteractor player, out string hint);
    InteractionKind Kind { get; }
    float HoldDuration { get; }

    void OnTap(PlayerInteractor player);
    void OnHoldComplete(PlayerInteractor player);

    //  진행 바,애니메이션을 위해 추가
    void OnHoldStart(PlayerInteractor player);
    void OnHoldCancel(PlayerInteractor player);
}
