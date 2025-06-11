using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraTargetEventListener : MonoBehaviour, I_EventListener<CameraTargetEventData>
{
    //[Header("Events")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;
    
    
    [Header("Default Settings")]
    [SerializeField] private GameObject _defaultCameraTarget;
    [SerializeField] private SO_CameraSettings _defaultCameraSettings;
    
    [Header("References")]
    [SerializeField] private CinemachineCamera _virtualCamera;
    
    // Current camera state
    private GameObject _currentTarget;
    private SO_CameraSettings _currentSettings;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        // Find virtual camera if not set
        if (_virtualCamera == null)
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
        }
    }
    
    private void OnEnable()
    {
        // _cameraTargetEvent = GlobalGameEvents.CameraTargetEvent;
        // _cameraTargetEvent.RegisterListener(this);
        // Register for events
        if (_cameraTargetEvent != null)
        {
            _cameraTargetEvent.RegisterListener(this);
            Debug.Log("Registered event listener");
        }
        
        // Set up default camera settings
        // if (_defaultCameraTarget != null && _defaultCameraSettings != null)
        // {
        //     _currentTarget = _defaultCameraTarget;
        //     _currentSettings = _defaultCameraSettings;
        //     
        //     _currentSettings.SetCameraSettingsTarget(_currentTarget);
        //     _virtualCamera.Follow = _currentTarget.transform;
        //     
        //     // Initialize yaw/pitch from the target orientation
        //     _currentSettings.CinemachineTargetYaw = _currentTarget.transform.rotation.eulerAngles.y;
        //     _currentSettings.CinemachineTargetPitch = 0f;
        //     Debug.Log("Camera set up correctly");
        //     //_virtualCamera.GetCinemachineComponent<Cinem>
        // }
    }
    
    private void OnDisable()
    {
        // Unregister from events
        if (_cameraTargetEvent != null)
        {
            _cameraTargetEvent.UnregisterListener(this);
        }
        
        // Stop any transitions
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    // Event listener implementation
    public void OnEventTriggered(CameraTargetEventData data)
    {
        // Update current target and settings
        _currentTarget = data.NewTarget;
        _currentSettings = data.CameraSettings;
        
        // Configure the camera settings
        _currentSettings.SetCameraSettingsTarget(_currentTarget);
        transform.parent.transform.parent = _currentTarget.transform.parent.transform.parent;
     
        _virtualCamera.Follow = _currentTarget.transform;
        
        Debug.Log($"Camera switched to target: {_currentTarget.name}");
    }
    
  
    public GameObject GetCurrentTarget() => _currentTarget;
    public SO_CameraSettings GetCurrentSettings() => _currentSettings;
}
