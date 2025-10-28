using UnityEngine;

public enum InteractionKind { Tap, Hold }

public interface IInteractable
{
    bool CanInteract(PlayerInteractor player, out string hint);
    InteractionKind Kind { get; }
    float HoldDuration { get; }
    void OnTap(PlayerInteractor player);
    void OnHoldComplete(PlayerInteractor player);
}
