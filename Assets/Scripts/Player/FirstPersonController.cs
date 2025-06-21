using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Windows;

public class FirstPersonController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpForce = 9f;

    [Space]
    [SerializeField] private float acceleration = 2.0f;
    [SerializeField] private float deceleration = 3.0f;


    [Space]
    [SerializeField] private float gravity = -9f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.015f;
    [SerializeField] private float jumpTimeout = 0.5f;
    [SerializeField] private float fallTimeout = 0.15f;

    [SerializeField] private InputSystems_NetworkActions inputActions;
    private PlayerInput _playerInput;
    private bool isGrounded;
    
    [SerializeField] private bool isPilot = false;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer; // Assign this in the Inspector
    //private Camera _camera;
    [SerializeField] public GameObject SimPlayer;

    [SerializeField] SO_CameraSettings _cameraSettings;
    [SerializeField] GameObject CameraTarget;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private InputAction movement;
    private InputAction jump;
    private InputAction interact;
    private InputAction look;
    private Vector3 velocity; 

    private CharacterController _controller;
    private GameObject _mainCamera;
    private GameObject _playerCamera;
    private Rigidbody _rigidBody;
    private bool _inAircraft;
    private float _accelerationFactor;
    // List<IntGameEventListener> listeningList;
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
        _controller = GetComponent<CharacterController>();


        //  listeningList.Add(new IntGameEventListener(OnInt));
        // CameraTarget = GetComponent<SimulatedPlaneVisual>().GetVisualObj; 
        _cameraSettings.SetCameraSettingsTarget(CameraTarget);
        _cameraSettings.CinemachineTargetYaw = _cameraSettings.CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        inputActions = new InputSystems_NetworkActions();
        inputActions.Player.Enable();

        movement = inputActions.Player.Movement;
        jump = inputActions.Player.Jump;
        look = inputActions.Player.Look;

        inputActions.Player.Jump.performed += PerformJump;

        _playerInput = GetComponent<PlayerInput>();
        _rigidBody = GetComponent<Rigidbody>();
        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;
        Debug.Log("OnEnable on First Person Controller called!");
    }


    // Update is called once per frame
    void Update()
    {
        if (_playerCamera && _mainCamera)
        {
            CheckGround();
            if (!isPilot)
            {
                ApplyMovement();
                ApplyGravity();
                CameraRotation();
            }
        }
        else
        {
            _playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        }
        

    }

    private void FixedUpdate()
    {

    }

    private void PerformJump(InputAction.CallbackContext context)
    {
        
        //if (isGrounded /* && _jumpTimeoutDelta <= 0.0f */)
        //{
        //    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        //}
    }

    private void ApplyMovement()
    {
        float targetSpeed = moveSpeed;
        Vector2 targetDirection = movement.ReadValue<Vector2>();

        if (targetDirection ==  Vector2.zero)
        {
            targetSpeed = 0.0f; 
        }
        targetDirection.Normalize();

        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Use camera-relative movement
        Vector3 moveDirection = (camForward * targetDirection.y + camRight * targetDirection.x) * targetSpeed + Vector3.up * velocity.y;


        _controller.Move(moveDirection * Time.deltaTime);
    }

    private void UpdateAcceleration(float accelerationRate, float decelerationRate)
    {
        Vector2 moveVal = movement.ReadValue<Vector2>();
        if (Mathf.Abs(moveVal.y) > 0.1f || Mathf.Abs(moveVal.x) > 0.1f)
        {
            _accelerationFactor = Mathf.Clamp(_accelerationFactor + accelerationRate, 0f, 1f);
        }
        else
        {
            _accelerationFactor = Mathf.Clamp(_accelerationFactor - decelerationRate, 0f, 1f);
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded/* && velocity.y < 0*/)
        {
            velocity.y = -9f; 
        }
        velocity.y += gravity * Time.deltaTime;
    }

    private void CameraRotation()
    {
        
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, _mainCamera.transform.eulerAngles.y, transform.localRotation.eulerAngles.x);
        //transform.localRotation = Quaternion.AngleAxis(_mainCamera.transform.rotation.y, Vector3.up);
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
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

    public void SetIsPilot(bool truthval)
    {
        isPilot = truthval;
        Debug.Log(truthval);
        if (truthval)
        {
            inputActions.Disable();
            inputActions.Pilot.Enable();
        }
        else
        {
            inputActions.Pilot.Disable();
            inputActions.Enable();
        }
    }
}
