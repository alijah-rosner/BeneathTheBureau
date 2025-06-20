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
    private NetworkVariable<bool> _hasShooter = new NetworkVariable<bool>(); 

    private Camera _playerCam;


    private bool _newShooter = false;

    private GameObject _camera;
    private float xLook = 0;
    private float yLook = 0;
    private Vector2 look;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasShooter.Value)
        {
            return;
        }
        UpdateCamera();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RequestOwnershipRpc(false);
            return; 
        }
        
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

    private void UpdateCamera()
    {

    
        // Apply the final rotation
        _head.transform.rotation = Camera.main.transform.rotation;
    
    }


    public void Interact(ulong netObjId)
    {
        if (!_hasShooter.Value)
        {
            _host = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            RequestOwnershipRpc(true);
            _cameraTargetEventData = new(_head, _cameraSettings);
            _cameraTargetEvent?.TriggerEvent(_cameraTargetEventData);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(bool v)
    {
        _hasShooter.Value = v; 
    }
}