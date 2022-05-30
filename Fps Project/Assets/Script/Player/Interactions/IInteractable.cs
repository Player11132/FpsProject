using UnityEngine;

public interface IInteractable
{
    float HoldDuration { get; }

    bool HoldInteract { get; }

    bool is_Ineractable { get;}

    void OnInteract();
    void OnStartHover();
    void OnEndHover();
}
