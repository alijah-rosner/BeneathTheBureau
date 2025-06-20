using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayer : NetworkBehaviour
{
    [SerializeField] SimulatedPlaneVisual _visual;

    [SerializeField] FirstPersonController _firstPersonController;

    [SerializeField] PlayerRaycast _raycast;

    [SerializeField] CapsuleCollider _capsuleCollider;

    [SerializeField] PlayerInput _playerInput;

    [SerializeField] CharacterController _charController;

    private Transform _playerFollow; 

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _firstPersonController.enabled = false;
        _charController.enabled = false;
        _capsuleCollider.enabled = false;
        _visual.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //enabled = IsClient;

        if (!IsOwner)
        {
            enabled = false;
            _capsuleCollider.enabled = true;
            _charController.enabled = false;
            _raycast.enabled = false;
            _visual.enabled = false;
            return; 
        }
        _capsuleCollider.enabled = true;
        _charController.enabled = true;
        _playerInput.enabled = true;
        _firstPersonController.enabled = true;
        _raycast.enabled = true;
        _visual.enabled = true;

    }

    [Rpc(SendTo.Server)]
    public void InteractServerRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var obj))
        {
            if (obj.TryGetComponent<I_Interactable>(out var interactable))
            {
                Debug.Log("I'm Interacting widda network object!");
                interactable.Interact(GetComponent<NetworkBehaviour>().NetworkObjectId);
            }
        }
    }

    public void SetNewCameraFollow()
    {

    }
}
