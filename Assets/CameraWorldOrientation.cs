using Unity.Cinemachine;
using UnityEngine;

public class CameraWorldOrientation : MonoBehaviour
{
    [Header("Cinemachine References")]
    [SerializeField] CinemachineBrain _brain; 
    [Header("Events To Listen For")]
    [SerializeField] GameObject_GameEvent _onSimPlaneSpawn;

    private GameObjectEventListener _gameObjectEventListener; 
    
    void Awake()
    {
        _gameObjectEventListener = new GameObjectEventListener(SetWorldUp, _onSimPlaneSpawn);
    }

    private void SetWorldUp(GameObject gameObject)
    {
        _brain.WorldUpOverride = gameObject.transform; 
    }
}
