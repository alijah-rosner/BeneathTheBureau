using UnityEngine;

public static class GlobalGameEvents
{
    public static CameraTargetEvent CameraTargetEvent;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Load()
    {
        CameraTargetEvent = Resources.Load<CameraTargetEvent>("SO_Events/Misc. Events/OnCameraChangeRequest");
    }
}