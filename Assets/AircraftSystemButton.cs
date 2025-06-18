using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class AircraftSystemButton : MonoBehaviour, I_Interactable
{
    [Header("Events Triggered")] 
    [SerializeField] private SystemButton_GameEvent _interactionEvent;

    [Header("Settings")]
    [SerializeField] private bool _isButton;

    [Header("Non-Button Settings")] 
    [SerializeField] private Vector3 _axisToRotate;
    [SerializeField] private float _maxRotationOnAxis;
    [SerializeField] private float _minRotationOnAxis;
    [SerializeField] private float _sensitivity;
    [SerializeField] private GameObject _draggable;
    private NetworkVariable<Vector3> _eulers = new();

    private ulong _playerId;
    private float _val;
    private bool _isBeingUsed = false;

    public ulong PlayerId => _playerId;
    public float Value => _val;
    private float _currentAngle;

    public void Interact(ulong netObjId)
    {
        _playerId = netObjId;

        _isBeingUsed = !_isBeingUsed;
        _currentAngle = GetCurrentAngle();
    }
    
    private void Update()
    {
        if (!_isBeingUsed)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _isBeingUsed = false; 
        }

        float delta = Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y");

        _currentAngle = Mathf.Clamp(_currentAngle + delta * (_sensitivity * Time.deltaTime), _minRotationOnAxis, _maxRotationOnAxis);

        Vector3 euler = transform.localEulerAngles;
        
        if (_axisToRotate.x != 0)
        {
            euler.x = _currentAngle;
        }
        else if (_axisToRotate.y != 0)
        {
            euler.y = _currentAngle;
        }
        else if (_axisToRotate.z != 0)
        {
            euler.z = _currentAngle;
        }
        _val = Mathf.InverseLerp(_minRotationOnAxis, _maxRotationOnAxis, _currentAngle) * 100f;
        _eulers.Value = euler; 
        transform.localEulerAngles = _eulers.Value;
    }

    private float GetCurrentAngle()
    {
        if (_axisToRotate.x != 0)
        {
            return transform.localEulerAngles.x;
        }

        if (_axisToRotate.y != 0)
        {
            return transform.localEulerAngles.y;
        }
        
        return transform.localEulerAngles.z;
    }

    public void RotateAircraftSystemButton(float deg)
    {
        //_handle.transform.localRotation = Quaternion.AngleAxis(_maxRotationOnAxis, _axisToRotate);
        //_handle.transform.localRotation = _look.
    }
    
}