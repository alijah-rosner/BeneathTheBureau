using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float groundMoveSpeed = 20f;
    [SerializeField] private float aircraftMoveSpeed = 20f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float aircraftJumpForce = 9f;

    [Space]
    [SerializeField] private float groundAcceleration = 2.0f;
    [SerializeField] private float groundDeceleration = 3.0f;
    [SerializeField] private float aircraftAcceleration = 2.0f;
    [SerializeField] private float aircraftDeceleration = 3.0f;

    [Space]
    [SerializeField] private float groundGravity = 2f;
    [SerializeField] private float flightGravityScale = 2f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.015f;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer; // Assign this in the Inspector
    public Transform orientation;
    private Camera _camera;
    [SerializeField] public GameObject SimPlayer;

    private Rigidbody rigidBody;
    private float forwardMovement;
    private float lateralMovement;
    private float accelerationFactor;
    private float turnFactor = 1f;

    [Header("Debug Tooltip")]
    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private NetworkVariable<bool> _inAircraft = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField]
    private NetworkVariable<bool> _isPilot = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private float maxVerticalAngle = 90f;
    private Transform _cameraTransform;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> netVelocity = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> netRelativePosition = new NetworkVariable<Vector3>();

    // Reference to the current aircraft
    private FlightController2 currentAircraft;
    private Vector3 lastAircraftPosition;
    private Quaternion lastAircraftRotation;

    private float verticalRotation = 0f;

    private static Dictionary<ulong, GameObject> allSimPlayers = new Dictionary<ulong, GameObject>();
    private bool simPlayerSetupComplete = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            return;
        }

        currentAircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft").GetComponent<FlightController2>();
        SimPlayer = Instantiate(SimPlayer, GameObject.FindGameObjectWithTag("SimulatedAircraft").transform);
        // SimPlayer.GetComponent<NetworkObject>().Spawn();
        SimPlayer.GetComponent<SimCharacter>().SetSimNetworkController(this);
        SimPlayer.transform.parent = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;
        SimPlayer.GetComponentInChildren<Camera>().enabled = true;
        _camera = SimPlayer.GetComponentInChildren<Camera>();
        _cameraTransform = _camera.transform;
        rigidBody = GetComponent<Rigidbody>();
        // Initialize network variables
        if (IsServer)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation;
            netVelocity.Value = Vector3.zero;
        }
        else
        {
            ClientSpawnRpc();
        }

        NotifySimPlayerCreatedServerRpc(OwnerClientId);
    }

    public NetworkVariable<Vector3> simLocalPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> simLocalRotation = new NetworkVariable<Quaternion>();

    [ServerRpc]
    private void NotifySimPlayerCreatedServerRpc(ulong playerId)
    {
        // Server tells all clients to create a copy of this player's SimPlayer
        NotifySimPlayerCreatedClientRpc(playerId);
    }

    [ClientRpc]
    private void NotifySimPlayerCreatedClientRpc(ulong playerId)
    {
        // Skip if this is our own SimPlayer - we already created it
        if (IsOwner && playerId == OwnerClientId) return;

        // Find the corresponding PlayerController
        PlayerController targetController = null;
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (pc.OwnerClientId == playerId)
            {
                targetController = pc;
                break;
            }
        }

        if (targetController == null) return;

        // Find the aircraft
        GameObject simAircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
        if (simAircraft == null) return;

        // Create a SimPlayer instance for this remote player
        GameObject remoteSimPlayer = Instantiate(SimPlayer, simAircraft.transform);

        // Set up the SimCharacter with reference to its controller
        SimCharacter simChar = remoteSimPlayer.GetComponent<SimCharacter>();
        simChar.SetSimNetworkController(targetController);

        // Disable camera and input components
        Camera cam = remoteSimPlayer.GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        // Store reference in the PlayerController
        targetController.SimPlayer = remoteSimPlayer;
    }

    [Rpc(SendTo.Everyone)]
    private void ClientSpawnRpc()
    {
        netPosition.Value = transform.position;
        netRotation.Value = transform.rotation;
        netVelocity.Value = Vector3.zero;
    }

    private void Awake()
    {
        // Initialize Rigidbody reference for ALL clients
        rigidBody = GetComponent<Rigidbody>();

        if (rigidBody == null)
        {
            Debug.LogError($"[PlayerController] No Rigidbody component found on {gameObject.name}");
        }
    }

    private void Start()
    {
        if (!IsOwner) return;


        SetupCursor();

    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            CheckGround();
            HandleInput();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        // Only the owner updates network variables //getting error clock this
        //UpdateNetworkVariablesServerRpc(transform.position, transform.rotation, rigidBody.linearVelocity);
    }

    private void LateUpdate()
    {
        if (IsOwner && SimPlayer != null)
        {
            // Get the aircraft
            GameObject simAircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
            if (simAircraft != null)
            {
                // Update SimPlayer position based on ground position
                // Convert world position to aircraft's local space
                Vector3 localPos = simAircraft.transform.InverseTransformPoint(transform.position);

                // Update NetworkVariable (will be synced to all clients)
                UpdateSimPositionServerRpc(localPos);
            }
        }

        // If we have remote SimPlayers to update
        UpdateRemoteSimPlayers();
    }

    // Helper field to store remote SimPlayer references
    public GameObject remoteSimPlayer;

    private void UpdateRemoteSimPlayers()
    {
        // Update all remote SimPlayers' positions
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            // Skip our own player
            if (pc.IsOwner) continue;

            // If this controller has a remote SimPlayer and network position
            if (pc.remoteSimPlayer != null)
            {
                // Find the aircraft
                GameObject simAircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
                if (simAircraft == null) continue;

                // Update position from NetworkVariable in local space
                pc.remoteSimPlayer.transform.localPosition = pc.simLocalPosition.Value;
                pc.remoteSimPlayer.transform.localRotation = pc.simLocalRotation.Value;
            }
        }
    }

    [ServerRpc]
    private void UpdateSimPositionServerRpc(Vector3 localPos)
    {
        simLocalPosition.Value = localPos;

        // You can also update rotation if needed
        // simLocalRotation.Value = localRot;
    }
    private void CheckGround()
    {
        int playerLayerMask = 1 << LayerMask.NameToLayer("isPlayer");
        int defaultLayerMask = 1 << LayerMask.NameToLayer("Default");
        int combinedLayerMask = (groundLayer | defaultLayerMask) & ~playerLayerMask;

        // Perform the SphereCast with the combined mask
        isGrounded = Physics.SphereCast(
            transform.position,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance,
            combinedLayerMask
        );
    }

    private void HandleInput()
    {
        if (!_isPilot.Value)
        {
            forwardMovement = Input.GetAxisRaw("Vertical");
            lateralMovement = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                JumpServerRpc();
            }
        }
    }

    private void HandleMovement()
    {

        if (_inAircraft.Value && !_isPilot.Value)
        {
            UpdateAcceleration(aircraftAcceleration, aircraftDeceleration);

            // Get the active camera's directions
            FlightController2 aircraft = FindFirstObjectByType<FlightController2>();

            if (_camera != null && aircraft != null)
            {
                // Get camera's forward and right, flattened to aircraft's local XZ plane
                Vector3 camForward = Vector3.ProjectOnPlane(_camera.transform.forward, aircraft.transform.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(_camera.transform.right, aircraft.transform.up).normalized;

                // Create movement direction using camera orientation instead of orientation object
                Vector3 moveDirection = (camForward * forwardMovement + camRight * lateralMovement).normalized;

                // Apply movement force
                rigidBody.AddForce(moveDirection * aircraftMoveSpeed * accelerationFactor, ForceMode.Acceleration);
            }

        }
        else
        {
            // Ground movement (unchanged)
            UpdateAcceleration(groundAcceleration, groundDeceleration);
            Vector3 moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;
            rigidBody.AddForce(moveDirection * groundMoveSpeed * accelerationFactor, ForceMode.Acceleration);
        }

        if (SimPlayer != null)
        {
            // Set SimPlayer position relative to aircraft in local space
            SimPlayer.transform.localPosition = transform.localPosition;

        }

    }


    private void UpdateAcceleration(float accelerationRate, float decelerationRate)
    {
        if (Mathf.Abs(forwardMovement) > 0.1f || Mathf.Abs(lateralMovement) > 0.1f)
        {
            accelerationFactor = Mathf.Clamp(accelerationFactor + accelerationRate, 0f, 1f);
        }
        else
        {
            accelerationFactor = Mathf.Clamp(accelerationFactor - decelerationRate, 0f, 1f);
        }
    }

    private void ApplyGravity()
    {
        float gravityMultiplier = _inAircraft.Value ? flightGravityScale : 1f;
        float effectiveGravity = groundGravity * gravityMultiplier;

        if (_inAircraft.Value)
        {
            FlightController2 aircraft = FindFirstObjectByType<FlightController2>();
            if (aircraft != null)
            {
                // Simply apply gravity in aircraft's down direction
                rigidBody.AddForce(-aircraft.transform.up * effectiveGravity, ForceMode.Acceleration);
            }
            else
            {
                // Fallback to world down if aircraft not found
                rigidBody.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
            }
        }
        else
        {
            // Standard gravity when on ground
            rigidBody.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
        }
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        JumpClientRpc();
    }

    [ClientRpc]
    private void JumpClientRpc()
    {
        isGrounded = false;
        if (_inAircraft.Value)
        {
            StartCoroutine(AirJump());
        }
        else
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private IEnumerator AirJump()
    {
        float duration = 0.35f; // Total duration of the jump
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            FlightController2 aircraft = FindFirstObjectByType<FlightController2>();
            if (aircraft != null)
            {
                Vector3 jumpDirection = aircraft.transform.up;
                rigidBody.AddForce(jumpDirection * aircraftJumpForce, ForceMode.Impulse);
            }
            timeElapsed += Time.fixedDeltaTime;
            yield return null; // Wait for the next frame
        }
    }

    private void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetIsPilot(bool r)
    {
        if (IsOwner)
        {
            _isPilot.Value = r;
            Cursor.lockState = _isPilot.Value ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = _isPilot.Value;
        }
    }

    public bool GetIsPilot() => _isPilot.Value;

    public bool GetInAircraft() => _inAircraft.Value;


    [ServerRpc]
    private void UpdateNetworkVariablesServerRpc(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (!IsOwner) return;
        netPosition.Value = position;
        netRotation.Value = rotation;
        netVelocity.Value = velocity;
    }

    public void SetInAircraft(bool r)
    {

        if (IsOwner)
        {
            // _inAircraft.Value = r;

            //// Reset aircraft reference when leaving
            //if (!r)
            //{
            //    currentAircraft = null;
            //    transform.parent = null;
            //}
            //else
            //{
            //   // transform.parent = GameObject.FindWithTag("Aircraft").transform;
            //}

            // Sync the state change
        }
        UpdateInAircraftStateRpc(r);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateInAircraftStateRpc(bool inAircraft)
    {
        _inAircraft.Value = inAircraft;
    }

    public Vector3 GetRelativeMovement()
    {
        // Return the current movement input as a local space vector
        return new Vector3(lateralMovement, 0, forwardMovement) * aircraftMoveSpeed;
    }
    public Camera GetCamera()
    {
        return _camera;
    }

    public void SetCamera(Camera cam)
    {
        _camera = cam;
    }

}

