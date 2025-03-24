using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class EngineSystem : AircraftSystem
{
    [Header("Specifications")]
    [SerializeField] float _maxThrust;
    [SerializeField] float _accelerationRate;
    

    public NetworkVariable<float> CurrentThrust = new NetworkVariable<float>(0.0f);

    private FlightController2 _controller;
    private GameObject _throttle; 

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _controller = FindFirstObjectByType<FlightController2>();
        Online.OnValueChanged += OnlineStateChanged;
        
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

