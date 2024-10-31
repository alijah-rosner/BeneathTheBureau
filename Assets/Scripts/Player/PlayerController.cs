using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerController : MonoBehaviour
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
  [SerializeField] private float groundCheckHeight = 1f;
  [SerializeField] private float groundCheckDistance = 1f;

  [Header("References")]
  public Transform orientation;

  private Rigidbody rigidBody;
  private float forwardMovement;
  private float lateralMovement;
  private float accelerationFactor;
  private float turnFactor = 1f;
  private bool isGrounded;
  private bool _inAircraft;
  private bool _isPilot;

  private const float BASE_GRAVITY = 9.81f;
  private const float AIRCRAFT_GRAVITY_MULTIPLIER = 5f;

  private void Start()
  {
    SetupCursor();
    rigidBody = GetComponent<Rigidbody>();
  }

  private void Update()
  {
    CheckGround();
    HandleInput();
  }

  private void FixedUpdate()
  {
    if (_inAircraft)
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
    isGrounded = Physics.SphereCast(checkPosition, groundCheckRadius, Vector3.down, out _, groundCheckDistance);
  }

  private void HandleInput()
  {
    if (!_isPilot)
    {
      forwardMovement = Input.GetAxisRaw("Vertical");
      lateralMovement = Input.GetAxisRaw("Horizontal");
    }

    if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
    {
      Jump();
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
    if (isGrounded && !_inAircraft)
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

      if (accelerationFactor < 0.05f)
      {
        rigidBody.velocity = aircraftRb.velocity;
      }
      else
      {
        // Calculate relative movement within the aircraft
        Vector3 localMoveDirection = aircraftRb.transform.TransformDirection(moveDirection);
        relativeVelocity = localMoveDirection * aircraftMoveSpeed * accelerationFactor;

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
    float gravityMultiplier = _inAircraft ? AIRCRAFT_GRAVITY_MULTIPLIER : 1f;
    float effectiveGravity = BASE_GRAVITY * gravityMultiplier;

    // Don't apply extra gravity when grounded
    if (!isGrounded)
    {
      rigidBody.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
    }
  }

  private void Jump()
  {
    float jumpPower = _inAircraft ? aircraftJumpForce : jumpForce;
    rigidBody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
  }

  private void SetupCursor()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  public void SetIsPilot()
  {
    _isPilot = !_isPilot;
    Cursor.lockState = _isPilot ? CursorLockMode.Confined : CursorLockMode.Locked;
    Cursor.visible = _isPilot;
  }

  public bool GetIsPilot() => _isPilot;

  public void SetInAircraft()
  {
    _inAircraft = !_inAircraft;
    // Reset acceleration when transitioning
    accelerationFactor = 0f;
  }

  public bool GetInAircraft() => _inAircraft;
}
