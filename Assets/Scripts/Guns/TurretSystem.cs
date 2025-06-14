using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class TurretSystem : AircraftSystem, I_Interactable
{
    private NetworkVariable<bool> _canInteract = new NetworkVariable<bool>();


    public GameObject _projectile;

    [SerializeField] private Transform _origin;
    [SerializeField] private GameObject _head;

    [SerializeField] private CameraTargetEvent _cameraTargetEvent;
    [SerializeField] private SO_CameraSettings _cameraSettings; 
    private CameraTargetEventData _cameraTargetEventData;
        
    private GameObject _host;

   

    private float nextFire = 0.0f;
    private bool _timeToShoot = true;

    private Camera _playerCam;


    private bool _newShooter = false;

    private GameObject _camera; 


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        //UpdateCamera();

        if (Input.GetKey(KeyCode.Mouse0) && _timeToShoot)
        {
            ShootProjectileRpc();
            _timeToShoot = false;
        }

        nextFire += Time.deltaTime;

        if (nextFire > 0.25f)
        {
            _timeToShoot = true;
            nextFire = 0.0f;
        }
    }

    [Rpc(SendTo.Server)]
    private void ShootProjectileRpc()
    {
        GameObject proj = Instantiate(_projectile, _origin.position, _head.transform.rotation);
        var netProj = proj.GetComponent<NetworkObject>();
        netProj.Spawn();
    }

    // private void UpdateCamera()
    // {
    //     // Get mouse input for looking around
    //     mouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 1000;
    //     mouseMove.x = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 1000;
    //
    //     // Update look angles
    //     xLook -= mouseMove.y;
    //     yLook += mouseMove.x;
    //
    //     // Clamp vertical look
    //     xLook = Mathf.Clamp(xLook, -90f, 90f);
    //     //yLook = Mathf.Clamp(yLook, -90, 90f);
    //
    //     // Get the head's up vector in world space
    //     Vector3 playerUp = _host.transform.up;
    //
    //     // Create a rotation that aligns with the player's up direction
    //     Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, playerUp);
    //
    //     // Create the base camera rotation (pitch and yaw)
    //     Quaternion lookRotation = Quaternion.Euler(xLook, yLook, 0);
    //
    //     // Combine the rotations in the correct order
    //     targetRotation = alignRotation * lookRotation;
    //
    //     // Apply the final rotation
    //     _head.transform.rotation = targetRotation;
    //     TurretCamera.transform.rotation = targetRotation;
    //
    // }


    public void Interact(ulong netObjId)
    {
        if (_canInteract.Value)
        {
            _host = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            RequestOwnershipRpc(netObjId);
            _cameraTargetEventData = new(_head, _cameraSettings);
            _cameraTargetEvent?.TriggerEvent(_cameraTargetEventData);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong netObjId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(netObjId);
    }
}