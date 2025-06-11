using UnityEngine;
using Unity.Netcode;

public class ResourceSync : NetworkBehaviour
{
    NetworkVariable<float> resourceVal = new NetworkVariable<float>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }


    // Update is called once per frame
    void Update()
    {

    }
}
