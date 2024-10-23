using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
  private PlayerMovement playerMovement;
  Rigidbody rigidBody;
  float forwardMovement;
  float lateralMovement;
  float moveSpeed = 40;
  float topSpeed = 6;
  Vector3 moveDirection;
  public Transform orientation;
  float accelerationFactor = 1;
  float turnFactor = 1;
  bool isGrounded;
  bool isFlying;
  Vector3 playerHeight;
  float jumpForce = 7;


  // [SerializeField] TMP_Text text_;

  void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    //text_.text = GameObject.Find("Relay").GetComponent<TestRelay>().joinCode_;
    playerMovement = this;
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
    if (!isFlying)
    {

      if (isGrounded)
      {
        rigidBody.drag = 4.5f;
      }
      else
      {
        rigidBody.drag = .75f;
      }

      MovePlayer();
    }
  }

  void PlayerInput()
  {
    if (!isFlying)
    {

      forwardMovement = Input.GetAxisRaw("Vertical");
      lateralMovement = Input.GetAxisRaw("Horizontal");

      if (Input.GetKey(KeyCode.LeftShift))
      {
        topSpeed = 12;
      }
      else
      {
        topSpeed = 6;
      }


      if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
      {
        Jump();
      }
    }
    if (Input.GetKey(KeyCode.Q))
    {
      isFlying = !isFlying;
      gameObject.AddComponent(typeof(PlaneController));
    }
  }

  void MovePlayer()
  {
    if (!isFlying)
    {

      if (forwardMovement != 0 || lateralMovement != 0)
      {
        accelerationFactor = Mathf.Clamp(accelerationFactor += .06f, 0, 5);
      }
      else
      {
        accelerationFactor = 0;
      }

      turnFactor = ((Mathf.Abs(Input.GetAxis("Mouse X"))) + 1);
      accelerationFactor = Mathf.Clamp(accelerationFactor /= turnFactor, 0, 1);
      moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;

      if (rigidBody.velocity.magnitude < topSpeed && isGrounded)
      {
        rigidBody.AddForce(moveDirection * moveSpeed * accelerationFactor, ForceMode.Force);
      }
    }
  }

  void Jump()
  {
    rigidBody.AddForce(Vector3.up * jumpForce * 2.0f, ForceMode.Impulse);
  }
}
