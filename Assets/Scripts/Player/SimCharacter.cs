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

    [Header("Triggers the following events")]
    [SerializeField] private CameraTargetEvent _cameraTargetEvent;

    [Header("Camera Event Information")]
    [SerializeField] private CameraTargetEventData _cameraRequestData; 

    private void Start()
    {
        _cameraTargetEvent?.TriggerEvent(_cameraRequestData); 
    }

    public void SetSimNetworkController(PlayerController obj)
    {
        _controller = obj;
    }

    public GameObject GetCameraPosition()
    {
        return _cameraRequestData.NewTarget; 
    }

    public PlayerController GetSimNetworkController()
    {
        return _controller; 
    }
    
    // NEW
}
