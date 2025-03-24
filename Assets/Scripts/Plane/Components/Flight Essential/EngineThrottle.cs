using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class Throttle : MonoBehaviour, I_Interactable
{
    private EngineSystem _engineSystem;
    private float _value; 

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