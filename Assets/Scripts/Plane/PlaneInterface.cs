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

    // Network Variables
    private NetworkVariable<ulong> currentPilotId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        aircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
        _cameraRequestData.NewTarget = GameObject.FindWithTag("AircraftCameraPosition");
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Deactivate pilot mode
            _playerController = pilotObj.GetComponent<FirstPersonController>();
            ReleasePilotControlServerRpc(currentPilotId.Value);
            _playerController.SetIsPilot(false);
            //playerController.SetInAircraft(true);
            GameObject camTarget = simObj.GetComponent<SimCharacter>().CameraPosition;
            CameraTargetEventData playerCameraData = new(camTarget, _onExitCameraSettings);

            _cameraTargetEvent?.TriggerEvent(playerCameraData);

            if (aircraft.TryGetComponent<FlightController2>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to pilot ID changes
        currentPilotId.OnValueChanged += OnPilotChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentPilotId.OnValueChanged -= OnPilotChanged;
    }


    [Rpc(SendTo.Server)]
    private void RequestPilotControlServerRpc(ulong requestingPlayerId)
    {
        if (currentPilotId.Value != ulong.MaxValue)
            return;

        // Set the new pilot
        currentPilotId.Value = requestingPlayerId;
  

        // Transfer aircraft ownership to the new pilot
        if (aircraft.TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(requestingPlayerId);
            GetComponent<NetworkObject>().ChangeOwnership(requestingPlayerId);
        }

        // Notify all clients about the pilot change
        UpdatePilotState(requestingPlayerId, true);
    }

    [Rpc(SendTo.Server)]
    private void ReleasePilotControlServerRpc(ulong pilotId)
    {
        if (currentPilotId.Value != pilotId)
            return;

        // Clear the pilot
        currentPilotId.Value = ulong.MaxValue;

        // Return aircraft ownership to server
        if (aircraft.TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(NetworkManager.ServerClientId);
        }

        // Notify all clients about the pilot change
        UpdatePilotState(pilotId, false);
    }

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
            CameraTargetEventData playerCameraData = new (simObj, _onExitCameraSettings);

            _cameraTargetEvent?.TriggerEvent(playerCameraData);

            if (aircraft.TryGetComponent<FlightController2>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }

    private void OnPilotChanged(ulong previousPilotId, ulong newPilotId)
    {
        // Handle any necessary state cleanup when pilot changes
        if (newPilotId == NetworkManager.Singleton.LocalClientId)
        {
            UpdatePilotState(newPilotId, true);
        }
        // If we were the previous pilot
        else if (previousPilotId == NetworkManager.Singleton.LocalClientId)
        {
            UpdatePilotState(previousPilotId, false);
        }
    }

    public void Interact(ulong netObjId)
    {
        Debug.Log("Interacting with interface");
        bool isEntering = false;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject playerNetObj);

        if (playerNetObj.IsLocalPlayer)
        {
            if (currentPilotId.Value == ulong.MaxValue)
            {
                isEntering = true;

                //_cameraTargetEvent.TriggerEvent(); 
                RequestPilotControlServerRpc(playerNetObj.OwnerClientId);
            }

            // If we're the pilot, release control
            else if (currentPilotId.Value == NetworkManager.Singleton.LocalClientId)
            {
                ReleasePilotControlServerRpc(playerNetObj.OwnerClientId);
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