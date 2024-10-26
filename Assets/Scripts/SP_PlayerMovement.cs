using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

//To Be Depricated
public class TestPlayerMovement : MonoBehaviour
{
    private DemoPlayerMovement playerMovement;
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

        MovePlayer();
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
    }

    void MovePlayer()
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
        accelerationFactor = Mathf.Clamp(accelerationFactor / turnFactor, 0, 1);
        moveDirection = (orientation.forward * forwardMovement + orientation.right * lateralMovement).normalized;

        if (accelerationFactor == 0)
        {
            rigidBody.velocity = GameObject.FindObjectOfType<FlightController>().GetComponent<Rigidbody>().velocity;
        }

        // Apply movement force
        rigidBody.AddForce(moveDirection * moveSpeed * accelerationFactor, ForceMode.Force);

        // Dynamically set max velocity as a proportion of aircraft speed
        float maxVelocity = GameObject.FindObjectOfType<FlightController>().GetComponent<Rigidbody>().velocity.magnitude * 0.85f;  // Allow passengers to move at 50% of aircraft speed

        // Cap velocity to avoid unintended high speeds
        if (rigidBody.velocity.magnitude > maxVelocity)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxVelocity;
        }
    }

    void Jump()
    {
        rigidBody.AddForce(Vector3.up * jumpForce * 2.0f, ForceMode.Impulse);
    }
}
