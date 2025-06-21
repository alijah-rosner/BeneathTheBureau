using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class FuelInjectionSystem : AircraftSystem
{
    [Header("Events To Listen For")] 
    [SerializeField] private Float_GameEvent _onThrottleValueChanged; 

    [Header("Events To Trigger")]
    [SerializeField] private Float_GameEvent _onEngineFedFuelEvent;
    [SerializeField] private Float_GameEvent _onFuelDepletedEvent;
    
    [Header("Fuel Injection Settings")] 
    [SerializeField] private float _maxFuelCapacity;
    [SerializeField] private int _maxFuelPerTick;
    [SerializeField] private float _fuelInjectionTickRate;
    [SerializeField] private NetworkVariable<float> _currentFuelQuantity = new();

    private float _fuelInjectionTimer = 0f;
    
    private NetworkVariable<float> _currentInjectionRate = new();
    private Float_GameEventListener _onThrottleValueListener;
    private float _thrust; 
    
    private void Start()
    {
        _onThrottleValueListener = new Float_GameEventListener(OnThrottleChangedRpc, _onThrottleValueChanged);
    }

    [Rpc(SendTo.Server)]
    private void OnThrottleChangedRpc(float v)
    {
        _thrust = v; 
        _currentInjectionRate.Value = Mathf.InverseLerp(0, _maxFuelPerTick, v);
        _onEngineFedFuelEvent?.TriggerEvent(_thrust);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        if (_thrust > 0 && _currentFuelQuantity.Value <= 0f)
        {
            _onFuelDepletedEvent?.TriggerEvent(_thrust);
            return;
        }
        
        _fuelInjectionTimer += Time.deltaTime;
        if (_fuelInjectionTimer > _fuelInjectionTickRate)
        {
            _currentFuelQuantity.Value -= _currentInjectionRate.Value; 
            _fuelInjectionTimer = 0.0f;
        }

    }
}