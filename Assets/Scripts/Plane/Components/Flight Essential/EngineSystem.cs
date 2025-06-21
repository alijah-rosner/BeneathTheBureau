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
    [SerializeField] private Float_GameEvent _onEngineFedFuelEvent;
    [SerializeField] private Float_GameEvent _engineFuelDepeletedEvent;

    [Header("Events To Trigger")] 
    [SerializeField] private Float_GameEvent _thrustCalculatedEvent; 
    
    private Float_GameEventListener _engineEngagedListener; 
    private Float_GameEventListener _engineFuelDepletedListener; 

    public NetworkVariable<float> CurrentThrust = new NetworkVariable<float>(0.0f);

    private FlightController2 _controller;

    private void Start()
    {
        _engineEngagedListener = new Float_GameEventListener(OnEngineFedFuel, _onEngineFedFuelEvent);
        _engineFuelDepletedListener = new Float_GameEventListener(OnFuelDepleted, _engineFuelDepeletedEvent);
        _controller = FindFirstObjectByType<FlightController2>();
    }

    private void OnFuelDepleted(float v)
    {
        if (!IsOwner)
        {
            return;
        }
        OnFuelDepletedRpc(v);
    }

    private void OnEngineFedFuel(float v)
    {
        if (!IsOwner)
        {
            return;
        }
        OnEngineFedFuelRpc(v);
    }
    
    [Rpc(SendTo.Server)]
    private void OnEngineFedFuelRpc(float v)
    {
        float decVal = v / 100f; 
        CurrentThrust.Value = _maxThrust * decVal;
        _thrustCalculatedEvent?.TriggerEvent(CurrentThrust.Value);
    }
    
    [Rpc(SendTo.Server)]
    private void OnFuelDepletedRpc(float v)
    {
        CurrentThrust.Value = 0f;
        _thrustCalculatedEvent?.TriggerEvent(CurrentThrust.Value);
    }

}

