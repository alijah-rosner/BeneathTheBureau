using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "New Camera Settings Object", menuName = "CameraSettings/Data")]
public class SO_CameraSettings : ScriptableObject
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    internal GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] internal float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] internal float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] internal float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] internal bool LockCameraPosition = false;
    [SerializeField] internal float CinemachineTargetYaw;
    [SerializeField] internal float CinemachineTargetPitch;

    [SerializeField] internal float Threshold = 0.01f;
    
    public void SetCameraSettingsTarget(GameObject target)
    {
        CinemachineCameraTarget = target; 
    }
}