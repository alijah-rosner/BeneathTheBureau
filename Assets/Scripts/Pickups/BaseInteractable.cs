using UnityEngine;
using Unity.Netcode;

public abstract class BaseInteractable : NetworkBehaviour, I_Interactable
{
    public abstract void Interact(ulong netObjId);

}
