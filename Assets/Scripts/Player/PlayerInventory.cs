using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour
{
    private readonly List<ItemType> items = new List<ItemType>();
    [SerializeField] private Transform _handSocket;
    public void AddItem(NetworkObject worldItem, ItemType itemId)
    {

        items.Add(itemId);

        HoldInHand(worldItem);
    }

    private void HoldInHand(NetworkObject worldItem)
    {
       bool outcome = worldItem.TrySetParent(_handSocket, false);
        Debug.Log("TrySetParent Attempt:" + outcome);
        worldItem.ChangeOwnership(OwnerClientId);
        worldItem.GetComponent<Collider>().enabled = false;
        var rb = worldItem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }
}
