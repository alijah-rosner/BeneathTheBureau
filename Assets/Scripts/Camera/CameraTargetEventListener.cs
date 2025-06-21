using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraTargetEventListener : MonoBehaviour, I_EventListener<CameraTargetEventData>
{
    [Header("Events")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;

    
    
    [Header("Default Settings")]
    [SerializeField] private GameObject _defaultCameraTarget;
    [SerializeField] private SO_CameraSettings _defaultCameraSettings;
    
    [Header("References")]
    [SerializeField] private CinemachineCamera _virtualCamera;
    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private CinemachinePanTilt _panTilt;

    
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

        // Register for events
        if (_cameraTargetEvent != null)
        {
            _cameraTargetEvent.RegisterListener(this);
            Debug.Log("Registered event listener");
        }
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
        transform.parent.transform.parent = _currentTarget.transform.parent;
     
        _virtualCamera.Follow = _currentTarget.transform;
        _brain.WorldUpOverride = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;

        Debug.Log($"Camera switched to target: {_currentTarget.name}");
    }
    
  
    public GameObject GetCurrentTarget() => _currentTarget;
    public SO_CameraSettings GetCurrentSettings() => _currentSettings;
}
