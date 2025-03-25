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
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpForce = 9f;

    [Space]
    [SerializeField] private float acceleration = 2.0f;
    [SerializeField] private float deceleration = 3.0f;


    [Space]
    [SerializeField] private float gravity = -15.0f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.015f;
    [SerializeField] private float jumpTimeout = 0.5f;
    [SerializeField] private float fallTimeout = 0.15f;

    [SerializeField] private InputSystems_NetworkActions inputActions;
    private PlayerInput _playerInput;
    private bool isGrounded;
    private bool isPilot = false;

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

    private CharacterController _controller;
    private GameObject _mainCamera;
    private GameObject _playerCamera;
    private Rigidbody _rigidBody;
    private bool _inAircraft;
    private float _accelerationFactor;
    // List<IntGameEventListener> listeningList;

    private void Awake()
    {
        Debug.Log("Awake called!");
    }


    void OnEnable()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
        //_controller = GetComponent<CharacterController>();


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
        CheckGround();
    }

    private void FixedUpdate()
    {
        if (!isPilot)
        {
            ApplyMovement();
        }
        ApplyGravity();
        CameraRotation();
    }

    private void PerformJump(InputAction.CallbackContext context)
    {
        if (isGrounded /* && _jumpTimeoutDelta <= 0.0f */)
        {
            _rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void ApplyMovement()
    {
        float targetSpeed = moveSpeed;
        Vector2 moveVal = movement.ReadValue<Vector2>();

        if (moveVal == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        UpdateAcceleration(acceleration, deceleration);
        Vector3 moveDirection = (transform.forward * moveVal.y + transform.right * moveVal.x).normalized;
        transform.localPosition += moveDirection * acceleration * Time.deltaTime;
       // _rigidBody.AddForce(moveDirection * targetSpeed * _accelerationFactor, ForceMode.Acceleration);
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
        //if (_inAircraft)
        //{
        //    FlightController2 aircraft = FindFirstObjectByType<FlightController2>();
        //    if (aircraft != null)
        //    {
        //        // Simply apply gravity in aircraft's down direction
        //        _rigidBody.AddForce(-aircraft.transform.up * gravity, ForceMode.Acceleration);
        //    }
        //    else
        //    {
        //        // Fallback to world down if aircraft not found
        //        _rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        //    }
        //}
        //else
        //{
        // Standard gravity when on ground
        _rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        //  }
    }

    private void CameraRotation()
    {
        //// if there is an input and camera position is not fixed
        //if (look.GetControlMagnitude() >= _cameraSettings.Threshold)
        //{
        //    //don't multiply mouse input by time.deltatime;
        //    float deltatimemultiplier = 1f;

        //    Vector2 lookVal = look.ReadValue<Vector2>();

        //    _cameraSettings.CinemachineTargetYaw += lookVal.x * deltatimemultiplier;
        //    _cameraSettings.CinemachineTargetPitch += lookVal.y * deltatimemultiplier;
        //}

        //// clamp our rotations so our values are limited 360 degrees
        //_cameraSettings.CinemachineTargetYaw = ClampAngle(_cameraSettings.CinemachineTargetYaw, float.MinValue, float.MaxValue);
        //_cameraSettings.CinemachineTargetPitch = ClampAngle(_cameraSettings.CinemachineTargetPitch, _cameraSettings.BottomClamp, _cameraSettings.TopClamp);

        //// cinemachine will follow this target
        //_cameraSettings.CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cameraSettings.CinemachineTargetPitch + _cameraSettings.CameraAngleOverride,
        //    0.0f, 0.0f);

        //_rigidBody.rotation = Quaternion.Euler(0.0f, _rigidBody.rotation.eulerAngles.y, 0.0f);

        //transform.localRotation = Quaternion.Euler(0, _playerCamera.transform.rotation.y, 0);
        float cameraYRotation = _playerCamera.transform.localRotation.eulerAngles.y;

        // Create a new rotation that only rotates around the Y axis
        Quaternion targetRotation = Quaternion.Euler(0, cameraYRotation, 0);

        // Apply the rotation to your character
        transform.localRotation = targetRotation;
        //if (cameraForward.magnitude > 0.01f)
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

        //    float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);


        //    float rotationThreshold = 5.0f;
        //    if (angleDifference > rotationThreshold)
        //    {
        //        // Apply rotation smoothly
        //        float rotationSmoothTime = 0.2f;
        //        transform.rotation = Quaternion.Slerp(
        //            transform.rotation,
        //            targetRotation,
        //            Time.deltaTime / rotationSmoothTime);
        //    }
        //}
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
    }
}
