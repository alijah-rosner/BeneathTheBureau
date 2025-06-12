using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;
public class ResourceSync : NetworkBehaviour
{
    List<float> systemValueList = new List<float>();
    //List<NetworkVariable<float>> systemOutputList = new List<NetworkVariable<float>>();
    [SerializeField] private AircraftSystem _system;

    public void InitSystemSync()
    {
        //foreach (ResourceSettings set in _system.GetSystemResourceSettings())
        //{
        //    //systemOutputList.Add(new NetworkVariable<float>(set.Output));
        //}
    }
    private float _lastVal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitSystemSync();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }    
    }


    // Update is called once per frame
    void Update()
    {
        // If you're the host, set the effective output for every player
        if (IsHost)
        {
            foreach (ResourceSettings resource in _system.GetSystemResourceSettings())
            {
                if (resource._prevOutput != resource.Output)
                {
                    resource.SetEffectiveOutput(resource.Output);
                    resource._prevOutput = resource.Output;
                }
            }
        }
    }

    public void UpdateSystemEffectiveValues()
    {
        if (IsHost)
        {
            foreach (ResourceSettings resource in _system.GetSystemResourceSettings())
            {
                if (resource._prevOutput != resource.Output)
                {
                    resource.SetEffectiveOutput(resource.Output);
                }
            }
        }
    }

    public List<ResourceSettings> GetSystemResources()
    {
        return _system.resourceSettings;
    }

    public AircraftSystem GetSystem()
    {
        return _system;
    }

}
