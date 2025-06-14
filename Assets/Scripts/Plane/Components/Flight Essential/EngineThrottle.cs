using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class Throttle : MonoBehaviour, I_Interactable
{
    private EngineSystem _engineSystem;
    private float _value;
    protected float Value;  

    private void Start()
    {
        //_engineSystem = 
    }
    public void Interact(ulong netObjId)
    {
        //ChangeThrustValue(10.0f);
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestThrottleChangeServerRpc()
    {
        //_engineSystem.CurrentThrust.OnValueChanged += ChangeThrustValue;
    }
}