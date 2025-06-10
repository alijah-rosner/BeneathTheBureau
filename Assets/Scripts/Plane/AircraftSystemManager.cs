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
    [SerializeField] private GameObject_GameEvent _onAircraftSpawnEvent;
    [SerializeField] private FlightSystemsHandler _systemsHandler;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnNetworkChildren();
            ConnectElectricalSystems();
        }
        _onAircraftSpawnEvent?.TriggerEvent(gameObject);
                
    }

    private void Start()
    {
        _onAircraftSpawnEvent?.TriggerEvent(gameObject);
        ConnectElectricalSystems();

    }

    private void SpawnNetworkChildren()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].systemObj = Instantiate(_systems[i].systemObj, _systems[i].spawnPos.transform.position, _systems[i].spawnPos.transform.rotation);
            _systems[i].systemObj.Spawn();
            _systemsHandler.AddSystem(_systems[i].Name, _systems[i].systemObj.gameObject.GetComponent<AircraftSystem>());
            //_systems[i].systemObj.TrySetParent(transform, false);
        } 
        SetSystemParentRpc(NetworkManager.ServerClientId);
        ConnectElectricalSystems();
    }

    [Rpc(SendTo.Everyone)]
    private void SetSystemParentRpc(ulong requestingPlayerId)
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].systemObj.TrySetParent(transform, false);
        }
    }

    private void ConnectElectricalSystems()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            if (_systems[i].Name == "PlaneInterface")
            {
                Debug.Log("Grabbing Plane Interface");
                GetComponent<FlightController2>().planeInterface = _systems[i].systemObj.gameObject;
                _systems[i].systemObj.GetComponent<PlaneInterface>().SetAircraftIncludingCamera(gameObject, GameObject.FindGameObjectWithTag("MouseFlightManager"));
            }
        }
    }

    private void ConnectMechanicalSystems()
    {

    }
}
