using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

//To Be Depricated
public class TestPlayerMovement : MonoBehaviour
{
  Rigidbody rigidBody;
  float forwardMovement;
  float lateralMovement;
  float moveSpeed = 40;
  Vector3 moveDirection;
  public Transform orientation;
  float accelerationFactor = 0.5f;
  float turnFactor = 1;
  
  bool isGrounded;
  Vector3 playerHeight;

  float jumpForce = 1.5f;

  private bool _inAircraft = false;
  private bool _isPilot = false;


  // [SerializeField] TMP_Text text_;

  void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    rigidBody = GetComponent<Rigidbody>();
  }

  void Update()
  {
    RaycastHit groundCheck;
    playerHeight = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
    isGrounded = Physics.SphereCast(playerHeight, .5f, Vector3.down, out groundCheck, 1f);

    PlayerInput();
  }

  void FixedUpdate()
  {
    if (_inAircraft)
    {
      MovePlayerInPlane();
    }
    else
    {
      MovePlayer();
    }

    ApplyGravity();
  }

  void PlayerInput()
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

  private void MovePlayer()
  {
    if (forwardMovement != 0 || lateralMovement != 0)
    {
      if (isGrounded)
      {
        accelerationFactor = Mathf.Clamp(accelerationFactor += 0.05f, 1, 2);
      }
      else
      {
        accelerationFactor = Mathf.Clamp(accelerationFactor -= 0.75f, 0, 2);

      }
    }
    else
    {
      accelerationFactor = Mathf.Clamp(accelerationFactor -= 0.75f, 0.5f, 2);
    }

    turnFactor = ((Mathf.Abs(Input.GetAxis("Mouse X"))) + 1);
    accelerationFactor = Mathf.Clamp(accelerationFactor /= turnFactor, 0, 1);
    moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;


    rigidBody.AddForce(moveDirection * (moveSpeed / 4f) * accelerationFactor, ForceMode.Acceleration);


  }
  void MovePlayerInPlane()
  {
    Rigidbody aircraftRigidbody = GameObject.FindObjectOfType<FlightController>().GetComponent<Rigidbody>();

    if (forwardMovement != 0 || lateralMovement != 0)
    {
      accelerationFactor = Mathf.Clamp(accelerationFactor += 0.05f, 1, 2);
    }
    else
    {
      accelerationFactor = Mathf.Clamp(accelerationFactor -= 0.75f, 0, 2);
    }

    // Calculate move direction in world space
    moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;

    // Apply movement force relative to aircraft
    Vector3 adjustedMoveDirection = moveDirection * moveSpeed * accelerationFactor;

    if (_inAircraft && aircraftRigidbody.velocity.magnitude > 2.0f)
    {
      rigidBody.rotation = aircraftRigidbody.rotation;
      transform.rotation = aircraftRigidbody.rotation;

      if (accelerationFactor == 0)
      {
        // Adjust velocity to match aircraft's velocity when stationary
        rigidBody.velocity = aircraftRigidbody.velocity;
      }
      else
      {
        // Add player's own force plus the aircraft velocity to give relative movement
        rigidBody.velocity = aircraftRigidbody.velocity + (adjustedMoveDirection * 0.01f);
      }
      rigidBody.AddForce(moveDirection * (moveSpeed / 8f) * accelerationFactor, ForceMode.Acceleration);
    }

    else
    {
      rigidBody.AddForce(moveDirection * (moveSpeed / 8f) * accelerationFactor, ForceMode.Acceleration);
    }

  }

  void ApplyGravity()
  {
    float gravityStrength = 9.81f;
    // Gravity constant



    // Apply normal gravity
    rigidBody.AddForce(Vector3.down * gravityStrength, ForceMode.Acceleration);
  }
  void Jump()
  {
    if (_inAircraft)
    {
      rigidBody.AddForce(Vector3.up * jumpForce * 6.0f, ForceMode.VelocityChange);

    }
    else
    {

      rigidBody.AddForce(Vector3.up * jumpForce * 5.0f, ForceMode.Impulse);
    }
  }
  public void SetIsPilot()
  {
    _isPilot = !_isPilot;
    if (!_isPilot)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }
    else
    {
      Cursor.lockState = CursorLockMode.Confined;
      Cursor.visible = true;
    }
  }
  public bool GetIsPilot()
  {
    return _isPilot;
  }
  public void SetInAircraft()
  {
    _inAircraft = !_inAircraft;
  }

  public bool GetInAircraft()
  {
    return _inAircraft;
  }
}
