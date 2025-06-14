using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

public class EngineSystem : AircraftSystem
{
    [Header("Specifications")]
    [SerializeField] float _maxThrust;
    [SerializeField] private float _fuelConsumptionRate;
    [SerializeField] private float _airIntakeValue; 

    [Header("Events To Listen For")] [SerializeField]
    private SystemButton_GameEvent _engineEngagedEvent;
    
    private SystemButton_EventListener _engineEngagedListener; 

    public NetworkVariable<float> CurrentThrust = new NetworkVariable<float>(0.0f);

    private FlightController2 _controller;
    private AircraftSystemButton _thrustThrottle; 

    private void Start()
    {
        _engineEngagedListener = new SystemButton_EventListener(OnEngineEngage, _engineEngagedEvent);
        _controller = FindFirstObjectByType<FlightController2>();
        //Online.OnValueChanged += OnlineStateChanged;

    }

    private void OnEngineEngage(AircraftSystemButton button)
    {
        _thrustThrottle = button; 
        
        OnEngineEngageRpc(button.PlayerId);
    }
    
    [Rpc(SendTo.Server)]
    private void OnEngineEngageRpc(ulong clientId)
    {
        _thrustThrottle.RotateAircraftSystemButton(30.0f);
        CurrentThrust.Value = _thrustThrottle.Value; 
    }

    private void OnlineStateChanged(bool oldValue, bool newValue)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (Online.Value)
        {
            // start turning the engines off, accelerating at the right rate yada yada yada
        }
        else
        {
            
        }
    }

    public void ChangeThrottleValue(float value)
    {
        if (IsServer)
        {
            CurrentThrust.Value = value; 
        }
    }
}

