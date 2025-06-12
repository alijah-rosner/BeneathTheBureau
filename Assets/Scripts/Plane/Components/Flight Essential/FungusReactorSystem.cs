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
    [SerializeField] private float fungalCharge;
    [SerializeField] private float organicModifier;
    [SerializeField] private float electricityThroughput;

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
        //resourceSetting = new ResourceSettings(ResourceType.Fungus, ResourceStatus.Balanced,
        //    true, 0.005f);
        onFungusAdded = new Float_GameEventListener(UpdateFungus);
        onOrganicMaterialAdded = new Float_GameEventListener(UpdateOrganicMaterial);
        prevElectricity = 0f;
        organicModifier = 1f;
        fungalCharge = resourceSettings[0].Output;
        //resourceSetting.Output = fungalCharge;
    }


    // Update values based off event triggered
    private void UpdateFungus(float v)
    {
        fungalCharge = v;
    }

    private void UpdateOrganicMaterial(float v)
    {
        organicModifier = v;
    }


    // Update output and trigger event if output has changed
    private void Update()
    {

        electricityThroughput = resourceSettings[0].Output * organicModifier;
        if (electricityThroughput != prevElectricity)
        {
            Debug.Log("Changing Electricity.");
            Debug.Log("Previous: " + prevElectricity + "\nNew: " + electricityThroughput);
            onOutputElectricityChanged?.TriggerEvent(electricityThroughput);
            prevElectricity = electricityThroughput;
        }
        resourceSettings[0].Output -= resourceSettings[0].rate;
        //fungalCharge = resourceSetting.Output;
    }

}
