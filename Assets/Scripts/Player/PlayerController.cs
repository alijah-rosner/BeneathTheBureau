using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float groundMoveSpeed = 40f;
    [SerializeField] private float aircraftMoveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float aircraftJumpForce = 9f;
    [SerializeField] private float groundAcceleration = 0.05f;
    [SerializeField] private float groundDeceleration = 0.75f;
    [SerializeField] private float aircraftAcceleration = 0.03f;
    [SerializeField] private float aircraftDeceleration = 0.5f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.015f;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer; // Assign this in the Inspector
    public Transform orientation;
    [SerializeField] private Camera _camera;

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

    private const float groundGravity = 9.81f;
    private const float flightGravityScale = 9f;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> netVelocity = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> netRelativePosition = new NetworkVariable<Vector3>();

    // Reference to the current aircraft
    private FlightController currentAircraft;
    private Vector3 lastAircraftPosition;
    private Quaternion lastAircraftRotation;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            return;
        }
        _camera.enabled = true;
        rigidBody = GetComponent<Rigidbody>();
        // Initialize network variables
        if (IsServer)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation;
            netVelocity.Value = Vector3.zero;
        }
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
        if (!IsOwner)
        {
            // Non-owners should interpolate to the networked position
            if (!_inAircraft.Value)
            {
                transform.position = Vector3.Lerp(transform.position, netPosition.Value, Time.fixedDeltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, netRotation.Value, Time.fixedDeltaTime * 10f);
                rigidBody.linearVelocity = netVelocity.Value;
            }
            else
            {
                FlightController aircraft = FindObjectOfType<FlightController>();
                if (aircraft != null)
                {
                    // Use relative position to aircraft for interpolation
                    Vector3 targetPos = aircraft.transform.position +
                        aircraft.transform.rotation * netRelativePosition.Value;
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 15f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, aircraft.transform.rotation, Time.fixedDeltaTime * 15f);
                }
            }
            return; // Early return for non-owners
        }

        if (_inAircraft.Value)
        {
            HandleAircraftMovement();
        }
        else
        {
            HandleGroundMovement();
        }

        ApplyGravity();

        // Only the owner updates network variables
        UpdateNetworkVariablesServerRpc(transform.position, transform.rotation, rigidBody.linearVelocity);
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
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            JumpServerRpc();
        }
    }

    private void HandleGroundMovement()
    {
        UpdateAcceleration(groundAcceleration, groundDeceleration);

        // Update turn factor based on mouse movement
        turnFactor = Mathf.Max(1f, Mathf.Abs(Input.GetAxis("Mouse X")) + 1f);
        accelerationFactor = Mathf.Clamp(accelerationFactor / turnFactor, 0f, 1f);

        // Calculate move direction
        Vector3 moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;

        // Apply movement force
        rigidBody.AddForce(moveDirection * groundMoveSpeed * accelerationFactor, ForceMode.Acceleration);

        // Apply drag only when grounded
        if (isGrounded)
        {
            rigidBody.linearDamping = 1f;
        }
        else
        {
            rigidBody.linearDamping = 0f;
        }
    }

    private void HandleAircraftMovement()
    {
        rigidBody.linearDamping = 0f;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        FlightController aircraft = FindObjectOfType<FlightController>();
        if (aircraft == null) return;

        // Store reference to current aircraft if not already set
        if (currentAircraft != aircraft)
        {
            currentAircraft = aircraft;
            lastAircraftPosition = aircraft.transform.position;
            lastAircraftRotation = aircraft.transform.rotation;
        }

        if (!aircraft.GetIsMoving())
        {
            HandleGroundMovement();
            return;
        }

        Rigidbody aircraftRb = aircraft.GetComponent<Rigidbody>();
        UpdateAcceleration(aircraftAcceleration, aircraftDeceleration);

        // Calculate relative position to aircraft
        Vector3 relativePos = transform.position - aircraft.transform.position;

        // Update position based on aircraft movement
        Vector3 aircraftDelta = aircraft.transform.position - lastAircraftPosition;
        Vector3 newPosition = transform.position + aircraftDelta;

        // Calculate relative movement within aircraft
        Vector3 moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;
        Vector3 relativeMovement = moveDirection /** aircraftMoveSpeed*/ * accelerationFactor;

        // Apply rotation with aircraft
        Quaternion rotationDelta = aircraft.transform.rotation * Quaternion.Inverse(lastAircraftRotation);
        Vector3 rotatedRelativePos = rotationDelta * relativePos;
        newPosition = aircraft.transform.position + rotatedRelativePos;

        // Update position and rotation
        transform.position = newPosition;
        transform.rotation = aircraft.transform.rotation;

        // Update velocity
        if (accelerationFactor < 0.05f && isGrounded)
        {
            rigidBody.linearVelocity = aircraftRb.linearVelocity;
        }
        else
        {
            rigidBody.linearVelocity = aircraftRb.linearVelocity + relativeMovement;
        }

        // Store current aircraft transform for next frame
        lastAircraftPosition = aircraft.transform.position;
        lastAircraftRotation = aircraft.transform.rotation;

        // Sync across network
        if (IsServer)
        {
            netRelativePosition.Value = relativePos;
            SyncPassengerPositionClientRpc(newPosition, aircraft.transform.rotation, rigidBody.linearVelocity);
        }
        else
        {
            SyncPassengerPositionServerRpc(relativePos);
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

        // Don't apply extra gravity when grounded

        rigidBody.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
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
            FlightController aircraft = FindObjectOfType<FlightController>();
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

    [ServerRpc]
    private void SyncPassengerPositionServerRpc(Vector3 relativePosition)
    {
        netRelativePosition.Value = relativePosition;
    }

    [ClientRpc]
    private void SyncPassengerPositionClientRpc(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (!IsOwner)
        {
            transform.position = position;
            transform.rotation = rotation;
            rigidBody.linearVelocity = velocity;
        }
    }

    public void SetInAircraft(bool r)
    {
        if (IsOwner)
        {
            _inAircraft.Value = r;

            // Reset aircraft reference when leaving
            if (!r)
            {
                currentAircraft = null;
            }

            // Sync the state change
            UpdateInAircraftStateServerRpc(r);
        }
    }

    [ServerRpc]
    private void UpdateInAircraftStateServerRpc(bool inAircraft)
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
