using UnityEngine;

public interface IInteractable
{
    // Returns true if this object can interact with the source item
    bool CanInteract(RoomObject sourceItem);

    // Performs the interaction. Returns true if the item was consumed/destroyed.
    bool OnInteract(RoomObject sourceItem);
}
