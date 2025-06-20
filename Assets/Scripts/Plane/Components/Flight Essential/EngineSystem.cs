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
    [SerializeField] private float _percentagePower; 

    [Header("Events To Listen For")] 
    [SerializeField] private Float_GameEvent _engineEngagedEvent;

    [Header("Events To Trigger")] 
    [SerializeField] private Float_GameEvent _thrustCalculatedEvent; 

    private Float_GameEventListener _engineEngagedListener; 

    public NetworkVariable<float> CurrentThrust = new NetworkVariable<float>(0.0f);

    private FlightController2 _controller;

    private void Start()
    {
        _engineEngagedListener = new Float_GameEventListener(OnEngineEngage, _engineEngagedEvent);
        _controller = FindFirstObjectByType<FlightController2>();
    }

    private void OnEngineEngage(float v)
    {
        OnEngineEngageRpc(v);
    }
    
    [Rpc(SendTo.Server)]
    private void OnEngineEngageRpc(float v)
    {
        float decVal = v / 100f; 
        CurrentThrust.Value = _maxThrust * decVal;
        _thrustCalculatedEvent?.TriggerEvent(CurrentThrust.Value);
    }

}

