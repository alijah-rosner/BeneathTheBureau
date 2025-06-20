using UnityEngine;
using Unity.Netcode;

public enum ItemType
{
    Fungus = 0,
    Weapon = 1
}
public class PickupItem : BaseInteractable
{
    [SerializeField] private ItemType m_ItemType;
    public override void Interact(ulong netObjId)
    {

        var client = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        var player = client.GetComponent<PlayerInventory>();
        player.AddItem(GetComponent<NetworkObject>(), m_ItemType);

    }
}
