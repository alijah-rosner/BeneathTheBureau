using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BH_Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    private float _speed = 1000f;
    private Rigidbody rh;
    public GameObject _turretHost;
    void Start()
    {
        rh = GetComponent<Rigidbody>();

        Vector3 vec = transform.rotation.normalized * Vector3.forward;

        //Vector3 quat = _turretHost.transform.position * _turretHost.transform.rotation;

        //Vector3 force = _turretHost.transform.forward * _speed;
        rh.AddForce(vec * _speed);

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        //rh.AddForce(Vector3.forward * _speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this, 3.0f);
    }
}
