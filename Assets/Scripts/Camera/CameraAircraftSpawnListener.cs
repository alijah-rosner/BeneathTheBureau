using Unity.Cinemachine;
using UnityEngine;

public class CameraAircraftSpawnListener : MonoBehaviour, I_EventListener<GameObject>
{
    [Header("Events")]
    [SerializeField] private GameObject_GameEvent _aircraftSpawnEvent;

    [SerializeField] private CinemachineBrain _brain; 
    private void OnEnable()
    {
        // Register for events
        if (_aircraftSpawnEvent != null)
        {
            _aircraftSpawnEvent.RegisterListener(this);
            Debug.Log("Registered event listener");
        }
    }
    
    private void OnDisable()
    {
        // Unregister from events
        if (_aircraftSpawnEvent != null)
        {
            _aircraftSpawnEvent.UnregisterListener(this);
        }
    }

    public void OnEventTriggered(GameObject data)
    {
        _brain.WorldUpOverride = data.transform;
    }
}
