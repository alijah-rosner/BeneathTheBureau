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

  private void Start()
  {
    SetupCursor();
    rigidBody = GetComponent<Rigidbody>();
  }

  private void Update()
  {
    if (!IsOwner) return; // Only the owner can control the player

    CheckGround();
    HandleInput();
  }

  private void FixedUpdate()
  {
    if (!IsOwner) return; // Only the owner can control the player

    if (_inAircraft.Value)
    {
      HandleAircraftMovement();
    }
    else
    {
      HandleGroundMovement();
    }

    ApplyGravity();
  }


  private void CheckGround()
  {
    Vector3 checkPosition = transform.position + Vector3.up * groundCheckHeight;
    isGrounded = Physics.SphereCast(checkPosition, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer);
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
      rigidBody.drag = 1f;
    }
    else
    {
      rigidBody.drag = 0f;
    }
  }

  private void HandleAircraftMovement()
  {
    FlightController aircraft = FindObjectOfType<FlightController>();
    if (aircraft == null) return;
    if (!aircraft.GetIsMoving())
    {
      HandleGroundMovement();
    }
    else
    {
      Rigidbody aircraftRb = aircraft.GetComponent<Rigidbody>();
      UpdateAcceleration(aircraftAcceleration, aircraftDeceleration);

      // Calculate relative movement
      Vector3 moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;
      Vector3 relativeVelocity = Vector3.zero;

      // Match aircraft rotation
      rigidBody.rotation = aircraftRb.rotation;
      transform.rotation = aircraftRb.rotation;

      if (accelerationFactor < 0.05f && isGrounded)
      {
        rigidBody.velocity = aircraftRb.velocity;
      }
      else
      {
        // Calculate relative movement within the aircraft
        relativeVelocity = moveDirection * aircraftMoveSpeed * accelerationFactor;

        // Combine aircraft velocity with relative movement
        rigidBody.velocity = aircraftRb.velocity + relativeVelocity;
      }
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

  public void SetInAircraft(bool r)
  {
    if (IsOwner)
    {
      _inAircraft.Value = r;
    }
  }

  public bool GetInAircraft() => _inAircraft.Value;
}
