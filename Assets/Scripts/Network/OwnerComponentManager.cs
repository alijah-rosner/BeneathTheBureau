using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class OwnerComponentManager : NetworkBehaviour
{
    [SerializeField] private Camera _camera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) 
        { 
            return; 
        }
        _camera.enabled = true;
    }
}
