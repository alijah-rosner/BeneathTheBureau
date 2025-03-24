using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class BH_Turret : AircraftSystem
{
    // Start is called before the first frame update
    private bool _canInteract = false;
    private bool _hasShooter = false;


    public GameObject _projectile;

    [SerializeField] private Transform _origin;
    [SerializeField] private GameObject _head;

    private GameObject _host;

    private float xLook;
    private float yLook;

    private float mouseSens = 1000.0f;
    private Transform orientation;

    private Vector3 mouseMov = Vector3.zero;

    private float nextFire = 0.0f;
    private bool _timeToShoot = true;

    private Camera _playerCam;
    [SerializeField] Camera TurretCamera;

    private bool _newShooter = false;

    private Vector2 mouseMove = Vector2.zero;
    private Rigidbody playerRigidbody;
    private Quaternion targetRotation;

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

        if (_canInteract && !_hasShooter)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _hasShooter = true;
                Debug.Log("Piloting turret");
                PlayerController playerControl = _host.gameObject.GetComponent<PlayerController>();
                _playerCam = playerControl.GetCamera();
                _playerCam.gameObject.SetActive(false);
                TurretCamera.gameObject.SetActive(true);
            }

        }
        else if (_hasShooter && _canInteract)
        {
            UpdateCamera();

            if (Input.GetKey(KeyCode.Mouse0) && _timeToShoot)
            {
                ShootProjectileRpc();
                

                Debug.Log("Attempting to shoot");
                _timeToShoot = false;
            }

            nextFire += Time.deltaTime;

            if (nextFire > 0.25f)
            {
                _timeToShoot = true;
                nextFire = 0.0f;
            }



        }
    }

    [Rpc(SendTo.Everyone)]
    private void ShootProjectileRpc()
    {
        GameObject proj = Instantiate(_projectile, _origin.position, _head.transform.rotation);
        var netProj = proj.GetComponent<NetworkObject>();
        netProj.Spawn();


        proj.GetComponent<BH_Projectile>()._turretHost = _head;
    }

    private void UpdateCamera()
    {
        // Get mouse input for looking around
        mouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 1000;
        mouseMove.x = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 1000;

        // Update look angles
        xLook -= mouseMove.y;
        yLook += mouseMove.x;

        // Clamp vertical look
        xLook = Mathf.Clamp(xLook, -90f, 90f);
        //yLook = Mathf.Clamp(yLook, -90, 90f);

        // Get the head's up vector in world space
        Vector3 playerUp = _host.transform.up;

        // Create a rotation that aligns with the player's up direction
        Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, playerUp);

        // Create the base camera rotation (pitch and yaw)
        Quaternion lookRotation = Quaternion.Euler(xLook, yLook, 0);

        // Combine the rotations in the correct order
        targetRotation = alignRotation * lookRotation;

        // Apply the final rotation
        _head.transform.rotation = targetRotation;
        TurretCamera.transform.rotation = targetRotation;

    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (other.transform.tag == "Player")
        {
            _canInteract = true;
            _host = other.gameObject;
            Debug.Log("Can interact: true");
            _newShooter = true;
            GetComponent<NetworkObject>().ChangeOwnership(other.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void OnTriggerExit(UnityEngine.Collider other)
    {
        if (other.transform.tag == "Player")
        {
            if (_playerCam)
            {
                if (!_playerCam.isActiveAndEnabled)
                {
                    _playerCam.gameObject.SetActive(true);
                    TurretCamera.gameObject.SetActive(false);
                }
                _canInteract = false;
                _hasShooter = !_hasShooter; 
                //_host = null;
                Debug.Log("Can interact: false");

            }
        }

    }

}
