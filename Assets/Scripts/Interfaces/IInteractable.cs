using UnityEngine;

public interface IInteractable
{
    // Returns true if this object can interact with the source item
    bool CanInteract(RoomObject sourceItem);

    // Performs the interaction
    void OnInteract(RoomObject sourceItem);
}
