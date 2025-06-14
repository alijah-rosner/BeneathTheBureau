using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class BH_Projectile : NetworkBehaviour
{
    // Start is called before the first frame update
    private float _speed = 2000f;
    private Rigidbody rh;

    void Start()
    {
        rh = GetComponent<Rigidbody>();

        Vector3 vec = transform.forward;

        rh.AddForce(vec * _speed);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        rh = GetComponent<Rigidbody>();

        Vector3 vec = transform.forward;
        
        rh.AddForce(vec * _speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this, 3.0f);
    }
}
