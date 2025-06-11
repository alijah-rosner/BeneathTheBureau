using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FlightController : NetworkBehaviour
{

    [Header("Components")]
    [SerializeField] public MouseFlightController controller = null;
    [SerializeField] public GameObject planeInterface = null;

    [Header("Physics")]
    [Tooltip("Force to push plane forwards with")] public float thrust = 100f;
    [Tooltip("Rate at which pitch adjusts from Altimeter")] public float pitchModifier = 25f;
    [Tooltip("Max speed plane can reach")] public float maxThrust = 100f;
    [Tooltip("Force to accelerate thrust")] public float acceleration = 50f;
    [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
    [Tooltip("Multiplier for all forces")] public float forceMult = 1000f;

    [Header("Turbulence")]
    [Tooltip("Slight rumble effect intensity")] public float rumbleForce = 0.05f;
    [Tooltip("Turbulence intensity")] public float turbulenceForce = 2.5f;
    [Tooltip("Rotational turbulence intensity")] public float turbulenceTorqueForce = 5.0f;
    [Tooltip("Probability of turbulence to occur (between 0 and 1)")] public float turbulenceChance = 0.01f;


    [Header("Autopilot")]
    [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 5f;
    [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 10f;

    [Header("Input")]
    [SerializeField] [Range(-1f, 1f)] private float pitch = 0f;
    [SerializeField] [Range(-1f, 1f)] private float yaw = 0f;
    [SerializeField] [Range(-0.25f, 0.25f)] private float roll = 0f;

    public float Pitch { set { pitch = Mathf.Clamp(value, -1f, 1f); } get { return pitch; } }
    public float Yaw { set { yaw = Mathf.Clamp(value, -1f, 1f); } get { return yaw; } }
    public float Roll { set { roll = Mathf.Clamp(value, -1f, 1f); } get { return roll; } }

    private Rigidbody rigid;

    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float curThrust;

    //Whether or not to recieve input
    private bool _isControlling = false;
    public bool _isPowered = false;

    //Container of all rigidbodies inside plane
    private HashSet<Rigidbody> _passengers = new();

    // Network variables to sync state
    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> netVelocity = new NetworkVariable<Vector3>();
    private NetworkVariable<float> netThrust = new NetworkVariable<float>();
    private NetworkVariable<float> netPitch = new NetworkVariable<float>();
    private NetworkVariable<float> netYaw = new NetworkVariable<float>();
    private NetworkVariable<float> netRoll = new NetworkVariable<float>();

    private NetworkVariable<Vector3> netAircraftVelocity = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> netAngularVelocity = new NetworkVariable<Vector3>();


    private bool _isMoving => rigid.linearVelocity.magnitude > 10f;
    private bool _hasStartedMoving = false;
    private bool _turbulenceFlag = false;

    //NetworkVariables for synchronization
    private NetworkVariable<Vector3> netLocalAcceleration = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> netTurbulenceFlag = new NetworkVariable<bool>();

    // Store previous velocity as NetworkVariable
    private NetworkVariable<Vector3> netPreviousLocalVelocity = new NetworkVariable<Vector3>();

    // Pilot Handling
    private ulong curPilotId = ulong.MaxValue;
    [SerializeField] private GameObject _cameraTarget;
    [SerializeField] private CameraTargetEventData _cameraRequestData;

    [Header("Event Triggers & Data")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;
    [SerializeField] private SO_CameraSettings _onExitCameraSettings;

    
    public void TriggerController(bool r)
    {
        _isControlling = r;
    }

    public bool GetIsMoving()
    {
        return _isMoving;
    }
    public bool GetController()
    {
        return _isControlling;
    }

    public HashSet<Rigidbody> GetPassengers()
    {
        return _passengers;
    }

    private void Awake()
    {
        curThrust = 0f;
        rigid = GetComponent<Rigidbody>();
        _cameraRequestData.NewTarget = GameObject.FindWithTag("AircraftCameraPosition");
        controller = GameObject.FindGameObjectWithTag("MouseFlightManager").GetComponent<MouseFlightController>();

        if (controller == null)
            Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");
    }

    private void Update()
    {
        if (!IsOwner) return;
        // Update altitude based on plane interface tool.

        UpdateAltimeter();

        // Calculate the autopilot inputs.

        float autoYaw = 0f;
        float autoPitch = 0f;
        float autoRoll = 0f;
        if (NetworkManager.Singleton.LocalClientId == curPilotId)
        {
            RunAutopilotRpc(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);
        }

        yaw = autoYaw;
        pitch = autoPitch;
        roll = autoRoll;

        UpdateFlightControlsServerRpc(pitch, yaw, roll, thrust);

    }
    private void UpdateFlightSystems()
    {
        thrust *= (GetComponent<FlightSystemsHandler>().GetElectricityThroughput() / 100.0f);
    }
    private void FixedUpdate()
    {
        if (!IsServer)
        {
            // Clients interpolate to the networked position/rotation
            //transform.position = Vector3.Lerp(transform.position, netPosition.Value, Time.fixedDeltaTime * 10f);
            //            transform.rotation = Quaternion.Lerp(transform.rotation, netRotation.Value, Time.fixedDeltaTime * 10f);            
            rigid.linearVelocity = netVelocity.Value;
            return;
        }

        //if (!IsOwner)
        //{
        //    return;
        //}
        curThrust = Mathf.Clamp(curThrust + acceleration * Time.fixedDeltaTime, 0f, maxThrust);
        //currentPitch = Mathf.Lerp(currentPitch, netPitch.Value, Time.deltaTime * 2f);
        //currentRoll = Mathf.Lerp(currentRoll, netRoll.Value, Time.deltaTime * 2f);
        currentPitch = netPitch.Value;
        currentRoll = netRoll.Value;

        // Apply forces
        rigid.AddRelativeForce(Vector3.forward * curThrust * forceMult, ForceMode.Force);
        rigid.AddRelativeTorque(new Vector3(
            turnTorque.x * currentPitch,
            turnTorque.y * netYaw.Value,
            -turnTorque.z * currentRoll) * forceMult,
            ForceMode.Force);

        ClampRotation();

        // Update network variables on the server
        netPosition.Value = transform.position;
        netRotation.Value = transform.rotation;
        netVelocity.Value = rigid.linearVelocity;

        // Check if the aircraft has started moving
        if (!_hasStartedMoving && _isMoving)
        {
            _hasStartedMoving = true;
            SetPassengersInAircraft(true);
        }

        AddTurbulenceEffect();
        //UpdatePassengers();
    }

    public void RunAutopilotRpc(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
    {
        // Convert destination to local space.
        var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
        var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

        yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitch = -Mathf.Clamp(localFlyTarget.y, -0.5f, 0.5f);
        var relativeRoll = Mathf.Clamp(localFlyTarget.x, -0.35f, 0.35f);

        // Manuevers the aircraft to fly wings level.
        // This can be done by zeroing out the Y component of the aircraft's right.
        var wingsLevelRoll = Mathf.Clamp(transform.right.y, -1f, 1f);

        // Blend between auto level and banking into the target.
        var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
        roll = Mathf.Lerp(wingsLevelRoll, relativeRoll, wingsLevelInfluence);
    }

    private void UpdateAltimeter()
    {
        if (_isControlling)
        {
            float keyboardPitch = Input.GetAxis("Vertical");
            {
                if (Mathf.Abs(keyboardPitch) > .25f)
                {
                    float roundedPitch = -Mathf.Sign(keyboardPitch);
                    float currentAltitude = controller.GetDestinationAltitude();
                    float newAltitude = currentAltitude + (roundedPitch * pitchModifier * Time.deltaTime);
                    controller.SetDestinationAltitude(newAltitude);
                }
            }
        }
    }

    public void AssignPilot(ulong pilotId)
    {
        if (curPilotId != ulong.MaxValue)
            return;

        // Set the new pilot
        curPilotId = pilotId;


        // Transfer aircraft ownership to the new pilot
        if (TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(pilotId);
            GetComponent<NetworkObject>().ChangeOwnership(pilotId);
        }

        // Notify all clients about the pilot change
        UpdatePilotState(pilotId, true);
    }
    private void AddTurbulenceEffect()
    {
        // Add consistent rumble
        Vector3 rumble = new Vector3(
    Random.Range(-1f, 1f),
    Random.Range(-1f, 1f),
    Random.Range(-1f, 1f)) * rumbleForce;

        rigid.AddForce(rumble, ForceMode.Acceleration);
        if (!IsServer) return;

        // Occasionally add turbulence
        if (Random.value < turbulenceChance)
        {
            Vector3 turbulence = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f
            ) * turbulenceForce;

            rigid.AddForce(turbulence, ForceMode.Acceleration);

            Vector3 turbulenceTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * turbulenceTorqueForce;

            rigid.AddTorque(turbulenceTorque, ForceMode.Acceleration);
        }
    }

    private void SetPassengersInAircraft(bool inAircraft)
    {
        if (_isMoving)
        {
            foreach (Rigidbody passenger in _passengers)
            {
                passenger.linearDamping = 0f;
                if (passenger.GetComponent<PlayerController>())
                {
                    passenger.GetComponent<PlayerController>().SetInAircraft(inAircraft);
                }
            }
        }
    }

    // Convert angles greater than 180 to a negative equivalent angle
    // Clamp to keep it from exceeding desired values
    private void ClampRotation()
    {
        Vector3 currentRotation = transform.localEulerAngles;

        // Handle pitch 
        float pitch = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;
        pitch = Mathf.Clamp(pitch, -15f, 15f);

        // Handle roll 
        float roll = currentRotation.z > 180 ? currentRotation.z - 360 : currentRotation.z;
        roll = Mathf.Clamp(roll, -25f, 25f);

        // Update the rotation
        transform.localEulerAngles = new Vector3(pitch, currentRotation.y, roll);
    }

    [ServerRpc]
    private void UpdateFlightControlsServerRpc(float pitch, float yaw, float roll, float thrust)
    {
        // Update network variables on the server
        netPitch.Value = pitch;
        netYaw.Value = yaw;
        netRoll.Value = roll;
        netThrust.Value = thrust;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPassengerServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            return;

        var rb = networkObject.GetComponent<Rigidbody>();
        if (rb != null && _isMoving)
        {
            _passengers.Add(rb);
            rb.TryGetComponent<PlayerController>(out PlayerController pc);
            if (pc)
            {
                pc.SetInAircraft(true);
            }
            AddPassengerClientRpc(networkObjectId);
        }
    }


    [ClientRpc]
    private void AddPassengerClientRpc(ulong networkObjectId)
    {
        if (IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            var rb = networkObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                _passengers.Add(rb);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePassengerServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            return;

        var rb = networkObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            _passengers.Remove(rb);
            RemovePassengerClientRpc(networkObjectId);
        }
    }

    [ClientRpc]
    private void RemovePassengerClientRpc(ulong networkObjectId)
    {
        if (IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            var rb = networkObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                _passengers.Remove(rb);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if (!_passengers.Contains(rb))
            {
                netObj.gameObject.transform.parent = transform;
                AddPassengerServerRpc(netObj.NetworkObjectId);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            netObj.gameObject.transform.parent = transform;
            AddPassengerServerRpc(netObj.NetworkObjectId);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            netObj.TryGetComponent<PlayerController>(out PlayerController localPlayer);
            localPlayer.SetInAircraft(false);
            netObj.gameObject.transform.parent = null;
            RemovePassengerServerRpc(netObj.NetworkObjectId);
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

    [Rpc(SendTo.Everyone)]
    public void ReleasePilotControlServerRpc(ulong pilotId)
    {
        if (curPilotId != pilotId)
            return;

        // Clear the pilot
        curPilotId = ulong.MaxValue;

        // Return aircraft ownership to server
        if (TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(NetworkManager.ServerClientId);
        }

        // Notify all clients about the pilot change
        UpdatePilotState(pilotId, false);
    }
    [Rpc(SendTo.Server)]
    private void RequestPilotControlServerRpc(ulong requestingPlayerId)
    {
        if (curPilotId != ulong.MaxValue)
            return;

        // Set the new pilot
        curPilotId = requestingPlayerId;


        // Transfer aircraft ownership to the new pilot
        if (TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(requestingPlayerId);
            GetComponent<NetworkObject>().ChangeOwnership(requestingPlayerId);
        }

        // Notify all clients about the pilot change
        UpdatePilotState(requestingPlayerId, true);
    }

    private void UpdatePilotState(ulong pilotId, bool isPilot)
    {

        if (NetworkManager.Singleton.LocalClientId != pilotId)
        {
            return;
        }

        GameObject pilotObj = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (pilotObj == null) return;

        FirstPersonController _playerController = pilotObj.GetComponent<FirstPersonController>();
        GameObject simObj = _playerController.GetComponent<SimulatedPlaneVisual>().GetVisual();
        if (_playerController == null) return;

        //Camera pilotCamera = playerController.GetCamera();

        if (isPilot)
        {
            // Activate pilot mode
            _playerController.SetIsPilot(true);
            _cameraTargetEvent?.TriggerEvent(_cameraRequestData);
            if (TryGetComponent<FlightController>(out var flightController))
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

            if (TryGetComponent<FlightController>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }
}