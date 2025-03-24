using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class AircraftSystemManager : NetworkBehaviour
{
    [System.Serializable]
    public class NetworkSystemInfo
    {
        public NetworkObject systemObj;
        public GameObject spawnPos;
        public string Name => 
            systemObj.TryGetComponent<AircraftSystem>(out var system) 
            ? system.SystemName 
            : "N/A";
    }

    [Header("Network Children")]
    [SerializeField] private List<NetworkSystemInfo> _systems = new List<NetworkSystemInfo>();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnNetworkChildren();
            ConnectElectricalSystems();
        }
        
    }

    private void SpawnNetworkChildren()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].systemObj = Instantiate(_systems[i].systemObj, _systems[i].spawnPos.transform.position, _systems[i].spawnPos.transform.rotation);
            _systems[i].systemObj.Spawn();
            _systems[i].systemObj.transform.parent = transform;
        }
    }

    private void ConnectElectricalSystems()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            if (_systems[i].Name == "PlaneInterface")
            {
                GetComponent<FlightController2>().planeInterface = _systems[i].systemObj.gameObject;
                _systems[i].systemObj.GetComponent<PlaneInterface>().SetAircraftIncludingCamera(gameObject, GameObject.FindGameObjectWithTag("MouseFlightManager"));
            }
        }
    }

    private void ConnectMechanicalSystems()
    {

    }
}
