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
    [SerializeField] private NetworkVariable<float> organicMaterial = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> fungalCharge = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> organicModifier = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> electricityThroughput = new NetworkVariable<float>();
    [SerializeField] private float _fungusGrowthTickRate;
 

    private float prevElectricity;
    private float _fungusGrowthTimer = 0.0f;
    
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
        //fungalCharge.OnValueChanged += On
        if (IsServer)
        {
        }

        //resourceSetting.Output = fungalCharge.Value;
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
        if (!IsServer)
        {
            return;
        }

        _fungusGrowthTimer += Time.deltaTime;
        if (_fungusGrowthTimer > _fungusGrowthTickRate)
        {
            _fungusGrowthTimer = 0.0f;
            
            float consumption = fungalCharge.Value / 25f;
            organicMaterial.Value = Mathf.Max(0, organicMaterial.Value - consumption);
            

            float growthRate = Mathf.Log(organicMaterial.Value + 1f, 10f) * (1f - fungalCharge.Value / 100f);
            fungalCharge.Value = Mathf.Min(100f, fungalCharge.Value + growthRate);
            TriggerFungusValueEventRpc();
            //onFungusValueChanged?.TriggerEvent(fungalCharge.Value);
        }
        
        
        electricityThroughput.Value = Mathf.Pow(fungalCharge.Value / 100f, 0.75f) * 100f;
        if (electricityThroughput.Value != prevElectricity)
        {
            Debug.Log("Changing Electricity.");
            Debug.Log("Previous: " + prevElectricity + "\nNew: " + electricityThroughput.Value);
            TriggerElectricalValueEventRpc();       
            prevElectricity = electricityThroughput.Value;
        }
        //resourceSetting.Output -= resourceSetting.rate;
        //fungalCharge.Value = resourceSetting.Output;
    }
    
    [Rpc(SendTo.Everyone)]
    private void TriggerFungusValueEventRpc()
    {
        onFungusValueChanged?.TriggerEvent(fungalCharge.Value);
    }
    
    [Rpc(SendTo.Everyone)]
    private void TriggerElectricalValueEventRpc()
    {
        onOutputElectricityChanged?.TriggerEvent(electricityThroughput.Value);
    }
}
