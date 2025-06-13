using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

// Resource Settings should be network variable

public class AircraftSystem : NetworkBehaviour, I_Damageable
{
    // Network variables
    [NonSerialized] public NetworkVariable<float> CurrentStructuralIntregity = new NetworkVariable<float>(100.0f);
    [NonSerialized] public NetworkVariable<bool> ReceivingPower = new NetworkVariable<bool>(false);
    [NonSerialized] public NetworkVariable<bool> Online = new NetworkVariable<bool>(false);

    [Header("Basic System Settings")]
    [SerializeField] protected string _systemName;

    [Header("Resource Settings")]
    [SerializeField] public List<ResourceSettings> resourceSettings = new List<ResourceSettings>();
    [Header("Damage Settings")]
    [SerializeField] protected float _structuralIntregity = 100.0f;
    [SerializeField] protected float _structuralFailureThreshold = 60.0f;

    //// Events
    //[SerializeField] GameEvent<AircraftSystem> OnDamage;
    //[SerializeField] GameEvent<AircraftSystem> OnRepair;
    //[SerializeField] GameEvent<AircraftSystem> OnInputValueChange; 
    //[SerializeField] GameEvent<AircraftSystem> OnOutputValueChange; 

    protected AircraftSystem powerSource;
    //private List<GameEventListener> _listeners; 

    public string SystemName => _systemName;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            DamageEventSystem.Instance?.Register(gameObject, this);
            // subsribe inputvalue change to the appropriate
            // same w the output value 
        }
    }

    private void Start()
    {
        transform.parent = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unregister from damage system
        if (IsServer)
        {
            DamageEventSystem.Instance?.Unregister(gameObject);
        }
    }


    public void TakeDamage(SO_DamageData data)
    {

    }

    public void SetPowerSource(AircraftSystem source)
    {
        powerSource = source;
    }

    public List<ResourceSettings> GetSystemResourceSettings()
    {
        return resourceSettings;
    }
}

