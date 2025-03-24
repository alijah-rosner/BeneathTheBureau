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
    [SerializeField] private NetworkVariable<float> fungalCharge;
    [SerializeField] private NetworkVariable<float> organicModifier;
    [SerializeField] private NetworkVariable<float> electricityThroughput;

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
    Float_GameEventListener onFungusAdded;
    Float_GameEventListener onOrganicMaterialAdded;

    // Attach functions that correspond to events the reactor is listening to
    private void Start()
    {
        onFungusAdded = new Float_GameEventListener(UpdateFungus);
        onOrganicMaterialAdded = new Float_GameEventListener(UpdateOrganicMaterial);
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
            onOutputElectricityChanged?.TriggerEvent(electricityThroughput.Value);
            prevElectricity = electricityThroughput.Value;
        }
    }

}
