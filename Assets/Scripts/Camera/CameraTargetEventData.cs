using UnityEngine;

[System.Serializable]
public class CameraTargetEventData
{
    public GameObject NewTarget;
    public SO_CameraSettings CameraSettings;
    
    public bool ResetRotation;
    public bool SmoothTransition;
    public float TransitionTime = 0.5f;

    public CameraTargetEventData(GameObject newTarget, SO_CameraSettings settings)
    {
        NewTarget = newTarget;
        CameraSettings = settings; 
    }
}