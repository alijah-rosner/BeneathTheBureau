using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class ElectricalPowerSystem : AircraftSystem
{
    [SerializeField] private float electricityInput;
    [SerializeField] private float electricityOutput;

    private float prevInputVal;

    [Header("Output Events")]
    [SerializeField] Float_GameEvent onElectricityValueChanged;

    [Header("Input Event")]
    [SerializeField] private Float_GameEvent onValueChangeEvent;

    private Float_GameEventListener onInputValueChanged;

    private void Start()
    {
        resourceSetting = new ResourceSettings(ResourceType.Electricity, ResourceStatus.Balanced, true, 0.0f);

        onInputValueChanged = new Float_GameEventListener(UpdateElectricityInput, onValueChangeEvent);
        //onInputValueChanged = gameObject.AddComponent<Float_GameEventListener>();
        prevInputVal = 0f;
        onInputValueChanged = new Float_GameEventListener(UpdateElectricityInput, onValueChangeEvent);
        prevInputVal = 0f;
        electricityInput = 0f;
    }

    private void Update()
    {
        if (electricityInput <= 0)
        {
            Online = false;
        }
       else if (electricityInput != electricityOutput)
        {
            onElectricityValueChanged?.TriggerEvent(electricityInput);
        }
    }
    private void UpdateElectricityInput(float v)
    {
        Debug.Log("Old Electricity Val: " + prevInputVal);
        Debug.Log("New Electricity: " + v);
        prevInputVal = v;
        electricityInput = v;
        electricityOutput = v;
    }
}