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

    [Header("Network Children")] [SerializeField]
    private List<NetworkSystemInfo> _systems = new List<NetworkSystemInfo>();

    [SerializeField] private GameObject_GameEvent _onAircraftSpawnEvent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }

    private void Start()
    {
        _onAircraftSpawnEvent?.TriggerEvent(gameObject);
        if (IsServer)
        {
            SpawnNetworkChildren();
        }

    }

    private void SpawnNetworkChildren()
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].systemObj = Instantiate(_systems[i].systemObj, _systems[i].spawnPos.transform.localPosition,
                _systems[i].spawnPos.transform.localRotation);
            _systems[i].systemObj.Spawn();
            //_systemsHandler.AddSystem(_systems[i].Name, _systems[i].systemObj.gameObject.GetComponent<AircraftSystem>());
            if (_systems[i].systemObj.TrySetParent(transform, false))
            {
                Debug.Log("System " + _systems[i].Name + " parented");
            }
            else
            {
                Debug.Log("System " + _systems[i].Name + " failed to parent");
            }
        }

        //SetSystemParentRpc(NetworkManager.ServerClientId);
        //ConnectElectricalSystems();
    }

    [Rpc(SendTo.Everyone)]
    private void SetSystemParentRpc(ulong requestingPlayerId)
    {
        for (int i = 0; i < _systems.Count; i++)
        {
            if (_systems[i].systemObj.TrySetParent(transform, false))
            {
                Debug.Log("System" + _systems[i].Name + " parented");
            }
            else
            {
                Debug.Log("System" + _systems[i].Name + " failed to parent" + " parent = " + transform);
            }
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
                _systems[i].systemObj.GetComponent<PlaneInterface>().SetAircraftIncludingCamera(gameObject,
                    GameObject.FindGameObjectWithTag("MouseFlightManager"));
            }
        }
    }

    private void ConnectMechanicalSystems()
    {
    }
}