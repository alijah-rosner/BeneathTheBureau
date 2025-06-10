using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class FungusReactorSystem : AircraftSystem
{
    [Tooltip("Debug Values - Don't assign")]
    [SerializeField] private NetworkVariable<float> fungalCharge = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> organicModifier = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> electricityThroughput = new NetworkVariable<float>();

    private float prevElectricity;

    public enum ReactorState
    {
        Underfed,
        Balanced,
        Overfed
    }
    [Header("Event that triggers listeners")]
    [SerializeField] Float_GameEvent onFungusValueChanged;
    [SerializeField] Float_GameEvent onOutputElectricityChanged;

    [Header("Event that Fungus System is listening for")]
    [SerializeField] Float_GameEventListener onFungusAdded;
    [SerializeField] Float_GameEventListener onOrganicMaterialAdded;

    // Attach functions that correspond to events the reactor is listening to
    private void Start()
    {
        resourceSetting = new ResourceSettings(ResourceType.Fungus, ResourceStatus.Balanced,
            true, 0.005f);
        onFungusAdded = new Float_GameEventListener(UpdateFungus);
        onOrganicMaterialAdded = new Float_GameEventListener(UpdateOrganicMaterial);
        prevElectricity = 0f;
        organicModifier.Value = 1f;
        fungalCharge.Value = 100f;
        resourceSetting.Output = fungalCharge.Value;
    }


    // Update values based off event triggered
    private void UpdateFungus(float v)
    {
        fungalCharge.Value = v;
    }

    private void UpdateOrganicMaterial(float v)
    {
        organicModifier.Value = v;
    }


    // Update output and trigger event if output has changed
    private void Update()
    {

        electricityThroughput.Value = fungalCharge.Value * organicModifier.Value;
        if (electricityThroughput.Value != prevElectricity)
        {
            Debug.Log("Changing Electricity.");
            Debug.Log("Previous: " + prevElectricity + "\nNew: " + electricityThroughput.Value);
            onOutputElectricityChanged?.TriggerEvent(electricityThroughput.Value);
            prevElectricity = electricityThroughput.Value;
        }
        resourceSetting.Output -= resourceSetting.rate;
        fungalCharge.Value = resourceSetting.Output;
    }

}
