using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class SimCharacter : MonoBehaviour
{
    // DEPRECATED 
    private PlayerController _controller;
    public NetworkVariable<Vector3> simLocalPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> simLocalRotation = new NetworkVariable<Quaternion>();
    public PlayerController Controller => _controller;
    [SerializeField] private GameObject _cameraTarget;
    [SerializeField] private SO_CameraSettings _cameraSettings;

    [Header("Triggers the following events")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;

    private void Start()
    {
    }

    public void SetSimNetworkController(PlayerController obj)
    {
        _controller = obj;
    }

    public GameObject GetCameraPosition()
    {
        return _cameraTarget;
    }

    public PlayerController GetSimNetworkController()
    {
        return _controller; 
    }
    
    public void TriggerInitialCameraSpawn()
    {
        var eventData = new CameraTargetEventData(_cameraTarget, _cameraSettings);
        //GlobalGameEvents.CameraTargetEvent.TriggerEvent(eventData);
        _cameraTargetEvent?.TriggerEvent(eventData);
    }

}
