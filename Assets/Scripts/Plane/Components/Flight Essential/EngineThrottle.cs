using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class EngineThrottle : NetworkBehaviour, I_Interactable
{
    [Header("Events To Trigger")] 
    [SerializeField] private Float_GameEvent _onInteractionEvent;
    [SerializeField] private Float_GameEvent _onThrottleValueChanged; 
 
    [Header("Throttle Settings")] 
    [SerializeField] private Vector3 _axisToRotate;
    [SerializeField] private float _maxRotationOnAxis;
    [SerializeField] private float _minRotationOnAxis;
    [SerializeField] private float _sensitivity;
    [SerializeField] private GameObject _draggable;

    private NetworkVariable<Vector3> _networkedRotation = new NetworkVariable<Vector3>();
    private GameObject _camera; 
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
        _camera = GameObject.FindGameObjectWithTag("PlayerCamera");
        _camera.GetComponent<CinemachineInputAxisController>().enabled = false; 
        Debug.Log("Interacting with throttle: " + _isBeingUsed);
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
            _camera.GetComponent<CinemachineInputAxisController>().enabled = true; 
            return; 
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
        
        
        SyncRotationRpc(euler, _val);
        
        
        
        //_interactionEvent?.TriggerEvent(this);
    }
    
    [Rpc(SendTo.Server)]
    private void SyncRotationRpc(Vector3 eulAngle, float val)
    {
        _networkedRotation.Value = eulAngle;
        transform.localEulerAngles = _networkedRotation.Value;
        _val = val;
        _onThrottleValueChanged?.TriggerEvent(val);
    }

    private void SetIsBeingUsed(bool v)
    {
        _isBeingUsed = v; 
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

}