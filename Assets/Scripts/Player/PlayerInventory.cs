using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour
{
    private readonly List<ItemType> items = new List<ItemType>();
    [SerializeField] private Transform _handSocket;

    public void AddItem(NetworkObject worldItem, ItemType itemId)
    {
        if (!IsServer) return;          // safety

        items.Add(itemId);
        AttachToHand(worldItem);
    }

    /* --- helper ----------------------------------------------------- */
    private void AttachToHand(NetworkObject itemNetObj)
    {
        itemNetObj.TrySetParent(NetworkObject, false);

        itemNetObj.transform.SetPositionAndRotation(
            _handSocket.position, _handSocket.rotation);

        itemNetObj.ChangeOwnership(OwnerClientId);

        var col = itemNetObj.GetComponent<Collider>();
        if (col) col.enabled = false;

        if (itemNetObj.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
    }
}
    //public void AddItem(NetworkObject worldItem, ItemType itemId)
    //{

    //    items.Add(itemId);

    //    HoldInHandServerRpc(worldItem.NetworkObjectId);
    //}

    //[Rpc(SendTo.Server)]
    //private void HoldInHandServerRpc(ulong netObjId)
    //{
    //    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out var obj))
    //    {
    //        bool outcome = obj.TrySetParent(_handSocket, false);
    //        Debug.Log("TrySetParent Attempt:" + outcome);
    //        obj.ChangeOwnership(OwnerClientId);
    //        obj.GetComponent<Collider>().enabled = false;
    //        var rb = obj.GetComponent<Rigidbody>();
    //        if (rb) rb.isKinematic = true;
    //    }
    //}
