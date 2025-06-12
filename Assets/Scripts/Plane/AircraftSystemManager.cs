using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class AircraftSystemManager : NetworkBehaviour
{
    [System.Serializable]
    public class NetworkSystemInfo
    {
        public GameObject systemObj;
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
   
    [Header("Input Event")]
    [SerializeField] private Float_GameEvent onValueChangeEvent;
    private Float_GameEventListener onInputValueChanged;
    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();

    //    if (IsServer)
    //    {
    //        SpawnNetworkChildren();
    //        ConnectElectricalSystems();
    //    }
    //    _onAircraftSpawnEvent?.TriggerEvent(gameObject);
                
    //}
    private void InitSystems()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnNetworkChildren();
            ConnectElectricalSystems();
        }
        
        _onAircraftSpawnEvent?.TriggerEvent(gameObject);
        foreach (NetworkSystemInfo sys in _systems)
        {
            _systemsHandler.AddSystem(sys.Name, sys.systemObj.GetComponent<AircraftSystem>());
        }
    }

    private void Start()
    {
        _onAircraftSpawnEvent?.TriggerEvent(gameObject);
        ConnectElectricalSystems();
        InitSystems();
    }

    private void SpawnNetworkChildren()
    {
        SetSystemParentRpc(NetworkManager.ServerClientId);
        ConnectElectricalSystems();
    }

    [Rpc(SendTo.Everyone)]
    private void SetSystemParentRpc(ulong requestingPlayerId)
    {
        for (int i = 0; i < _systems.Count; i++)
        {
           // _systems[i].systemObj.TrySetParent(transform, false);
        }
    }

    private void ConnectElectricalSystems()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            if (_systems[i].Name == "PlaneInterface")
            {
                Debug.Log("Grabbing Plane Interface");
                GetComponent<FlightController>().planeInterface = _systems[i].systemObj.gameObject;
                _systems[i].systemObj.GetComponent<PlaneInterface>().SetAircraftIncludingCamera(gameObject, GameObject.FindGameObjectWithTag("MouseFlightManager"));
            }
        }
    }

    private void ConnectMechanicalSystems()
    {

    }
}
