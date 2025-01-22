using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BH_Turret : MonoBehaviour
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

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_canInteract && !_hasShooter)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _hasShooter = true;
                Debug.Log("Piloting turret");
            }

        }
        else if (_hasShooter && _canInteract)
        {

            if (Input.GetKey(KeyCode.Mouse0) && _timeToShoot)
            {
                GameObject proj = Instantiate(_projectile, _origin.position, _head.transform.rotation);


                proj.GetComponent<BH_Projectile>()._turretHost = _head;

                Debug.Log("Attempting to shoot");
                _timeToShoot = false;
            }

            nextFire += Time.deltaTime;

            if (nextFire > 0.5f)
            {
                _timeToShoot = true;
                nextFire = 0.0f;
            }



        }
    }

    private void FixedUpdate()
    {
        if ( _hasShooter && _newShooter)
        {
            PlayerController playerControl = _host.gameObject.GetComponent<PlayerController>();
            _playerCam = playerControl.GetCamera();
            _playerCam.gameObject.SetActive(false);
            TurretCamera.gameObject.SetActive(true);
            _newShooter = false; 
        }

        if (_hasShooter && !_newShooter)
        {
            Ray ray = _playerCam.ScreenPointToRay(Input.mousePosition);

            // Create a plane at the turret's height
            Plane horizontalPlane = new Plane(Vector3.up, _head.transform.position);

            if (horizontalPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);

                // Calculate direction to target
                Vector3 directionToTarget = targetPoint - _head.transform.position;
                directionToTarget.y = 0; // Keep rotation horizontal

                if (directionToTarget != Vector3.zero)
                {
                    // Use LookRotation instead of Euler angles
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    // Apply smooth rotation
                    _head.transform.rotation = Quaternion.Slerp(_head.transform.rotation, targetRotation, 5f * Time.deltaTime);
                }
            }
        }

    }



    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (other.transform.tag == "Player")
        {
            _canInteract = true;
            _host = other.gameObject;
            Debug.Log("Can interact: true");
            _newShooter = true; 
        }
    }

    private void OnTriggerExit(UnityEngine.Collider other)
    {
        if (other.transform.tag == "Player")
        {
            _canInteract = false;
            _host = null;
            Debug.Log("Can interact: false");
        }

    }

}
