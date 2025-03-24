using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayer : NetworkBehaviour
{
    [SerializeField] FirstPersonController _firstPersonController;

    [SerializeField] PlayerRaycast _raycast;

    [SerializeField] CapsuleCollider _capsuleCollider;

    [SerializeField] PlayerInput _playerInput;

    private Transform _playerFollow; 

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _firstPersonController.enabled = false;
        _capsuleCollider.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        enabled = IsClient;

        if (!IsOwner)
        {
            enabled = false;
            _capsuleCollider.enabled = false;
            _raycast.enabled = false; 
            return; 
        }
        _capsuleCollider.enabled = true;
        _playerInput.enabled = true;
        _firstPersonController.enabled = true;
        _raycast.enabled = true; 

    }

    public void SetNewCameraFollow()
    {

    }
}
