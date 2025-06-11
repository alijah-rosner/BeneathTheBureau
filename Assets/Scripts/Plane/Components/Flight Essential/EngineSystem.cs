using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class EngineSystem : AircraftSystem
{
    [Header("Specifications")]
    [SerializeField] float _maxThrust;
    [SerializeField] float _accelerationRate;
    

    private FlightController2 _controller;
    private GameObject _throttle;

    private void Start()
    {
        _controller = FindFirstObjectByType<FlightController2>();
    }

    private void OnlineStateChanged(bool oldValue, bool newValue)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {

    }

    public void ChangeThrottleValue(float value)
    {

    }
}

