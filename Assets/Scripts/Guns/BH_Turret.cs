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

    private Transform _origin;
    private GameObject _head;

    private GameObject _host;

    private float xLook;
    private float yLook;

    private float mouseSens = 1000.0f;
    private Transform orientation;

    private Vector3 mouseMov = Vector3.zero;

    private float nextFire = 0.0f;
    private bool _timeToShoot = true;


    void Start()
    {
        _origin = transform.GetChild(2).GetChild(0);
        _head = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (_canInteract && !_hasShooter)
        {
            if (Input.GetKey(KeyCode.E))
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
        if (_hasShooter)
        {
            mouseMov.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSens;
            mouseMov.x = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSens;

            xLook -= mouseMov.y;
            yLook -= mouseMov.x;

            xLook = Mathf.Clamp(xLook, -90f, 90f);
            yLook = Mathf.Clamp(yLook, -90f, 90f);
            _head.transform.rotation = Quaternion.Euler(0, yLook, xLook);

        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            _canInteract = true;
            _host = collision.gameObject;
            Debug.Log("Can interact: true");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            _canInteract = false;
            _host = null;
            Debug.Log("Can interact: false");
        }

    }
}
