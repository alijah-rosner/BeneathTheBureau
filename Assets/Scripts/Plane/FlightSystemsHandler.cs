using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class FlightSystemsHandler : MonoBehaviour
{
    //private Dictionary<string, AircraftSystem> _systemsList = new Dictionary<string, AircraftSystem>();
    private Dictionary<string, ResourceSync> _syncedSystemsList = new Dictionary<string, ResourceSync>();
    //private Dictionary<string, AircraftSystem> _systemsList = new Dictionary<string, AircraftSystem>();

    [SerializeReference] public List<ResourceSync> syncList = new List<ResourceSync>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResourceSync[] baseResources = gameObject.GetComponents<ResourceSync>();
        for (int i = 0; i < baseResources.Length; ++i)
        {
            syncList.Add(baseResources[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        foreach (ResourceSync resource in syncList)
        {
            resource.UpdateSystemEffectiveValues();
            Debug.Log("Resource: " + resource.GetSystem().SystemName);
            foreach(ResourceSettings res in resource.GetSystemResources())
            {
                Debug.Log("Val : " + res.GetEffectiveOutput());
            }
        }
    }

    //public void AddSystem(string sysName, AircraftSystem newSys)
    //{
    //    //_systemsList.Add(sysName, newSys.GetComponent<AircraftSystem>());
    //     ResourceSync res = gameObject.AddComponent<ResourceSync>();
    //    //ResourceSync newRes = new ResourceSync(newSys);
    //    res = newRes;
    //    _syncedSystemsList.Add(sysName, res);
    //}

    //public float GetSystemEffectiveOutput(string sysName)
    //{
    //    if (_syncedSystemsList.ContainsKey(sysName)
    //    {
    //        return _syncedSystemsList[sysName].GetEffectiveOutput()
    //    }
    //}

}
