using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class ElectricalPowerSystem : AircraftSystem
{
    [SerializeField] private NetworkVariable<float> electricityInput = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> electricityOutput = new NetworkVariable<float>();

    private float prevInputVal;

    [Header("Output Events")]
    [SerializeField] Float_GameEvent onElectricityValueChanged;

    [Header("Input Event")]
    [SerializeField] private Float_GameEvent onValueChangeEvent;

    private Float_GameEventListener onInputValueChanged;

    private void Start()
    {
        //resourceSetting = new ResourceSettings(ResourceType.Electricity, ResourceStatus.Balanced, true, 0.0f);

        onInputValueChanged = new Float_GameEventListener(UpdateElectricityInput, onValueChangeEvent);
        //onInputValueChanged = gameObject.AddComponent<Float_GameEventListener>();
        prevInputVal = 0f;
        if (IsServer)
        {
            electricityInput.Value = 0.0f;
        }
    }
    public override void OnNetworkSpawn()
    {
        onInputValueChanged = new Float_GameEventListener(UpdateElectricityInput, onValueChangeEvent);
        if (IsServer)
        {
            prevInputVal = 0f;
            electricityInput.Value = 0f;
        }

    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        
        if (electricityInput.Value <= 0)
        {
            Online.Value = false;
        }
        if (electricityInput.Value != electricityOutput.Value)
        {
            onElectricityValueChanged?.TriggerEvent(electricityInput.Value);
        }
    }
    private void UpdateElectricityInput(float v)
    {
        Debug.Log("Old Electricity Val: " + prevInputVal);
        Debug.Log("New Electricity: " + v);
        prevInputVal = v;
        electricityInput.Value = v;
        electricityOutput.Value = v;
    }
}