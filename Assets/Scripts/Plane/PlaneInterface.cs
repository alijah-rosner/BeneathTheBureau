using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlaneInterface : AircraftSystem, I_Interactable
{
    [Header("Components")]
    [SerializeField] private GameObject aircraft = null;
    [SerializeField] private GameObject _cameraTarget;
    [SerializeField] private CameraTargetEventData _cameraRequestData;

    [Header("Event Triggers & Data")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;
    [SerializeField] private SO_CameraSettings _onExitCameraSettings;

    private GameObject pilotObj;
    private FirstPersonController _playerController;

    private GameObject simObj;

    ulong curPilotId = ulong.MaxValue;
    // Network Variables
    //private NetworkVariable<ulong> currentPilotId = new NetworkVariable<ulong>(
    //    ulong.MaxValue,
    //    NetworkVariableReadPermission.Everyone,
    //    NetworkVariableWritePermission.Server
    //);

    private void Start()
    {
        aircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
        _cameraRequestData.NewTarget = GameObject.FindWithTag("AircraftCameraPosition");
    }

    public void TryAssignPilot(ulong pilotId)
    {
        aircraft.GetComponent<FlightController>().AssignPilot(pilotId);
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Deactivate pilot mode
            _playerController = pilotObj.GetComponent<FirstPersonController>();
            _playerController.SetIsPilot(false);
            //playerController.SetInAircraft(true);
            CameraTargetEventData playerCameraData = new(simObj, _onExitCameraSettings);

            _cameraTargetEvent?.TriggerEvent(playerCameraData);

            if (aircraft.TryGetComponent<FlightController2>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }

    // [Rpc(SendTo.Server)]

    private void UpdatePilotState(ulong pilotId, bool isPilot)
    {
        if (NetworkManager.Singleton.LocalClientId != pilotId)
        {
            return;
        }

        pilotObj = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (pilotObj == null) return;

        _playerController = pilotObj.GetComponent<FirstPersonController>();
        simObj = _playerController.GetComponent<SimulatedPlaneVisual>().GetVisual();
        if (_playerController == null) return;

        //Camera pilotCamera = playerController.GetCamera();

        if (isPilot)
        {
            // Activate pilot mode
            _playerController.SetIsPilot(true);
            _cameraTargetEvent?.TriggerEvent(_cameraRequestData);
            if (aircraft.TryGetComponent<FlightController2>(out var flightController))
            {
                flightController.enabled = true;
                flightController.TriggerController(true);
            }
        }
        else
        {
            // Deactivate pilot mode
            _playerController.SetIsPilot(false);
            //playerController.SetInAircraft(true);
            CameraTargetEventData playerCameraData = new(simObj, _onExitCameraSettings);

            _cameraTargetEvent?.TriggerEvent(playerCameraData);

            if (aircraft.TryGetComponent<FlightController2>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }
    public void Interact(ulong netObjId)
    {
        Debug.Log("Interacting with interface");
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject playerNetObj);
        if (playerNetObj.IsLocalPlayer)
        {
            if (curPilotId == ulong.MaxValue)
            {
                //_cameraTargetEvent.TriggerEvent(); 
                aircraft.GetComponent<FlightController>().AssignPilot(playerNetObj.OwnerClientId);
            }

            // If we're the pilot, release control
            else if (curPilotId == NetworkManager.Singleton.LocalClientId)
            {
                aircraft.GetComponent<FlightController>().ReleasePilotControlServerRpc(playerNetObj.OwnerClientId);
                curPilotId = ulong.MaxValue;
            }
        }
        // If no pilot, try to become pilot
    }

    public void SetAircraftIncludingCamera(GameObject Aircraft, GameObject Camera)
    {
        aircraft = Aircraft;
        Debug.Log("Aircraft set up correctly");
        // aircraftCamera = Camera.GetComponent<MouseFlightController>().cam.gameObject; 
    }
}