using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class AircraftSystem : NetworkBehaviour, I_Damageable
{
    public enum ResourceType
    {
        Fuel,
        Electricity,
        Fungus,
        OrganicMaterial,
        Oxygen
    }

    public enum ResourceStatus
    {
        Under,
        Balanced,
        ReceivingOver
    }

    [System.Serializable]
    public class ResourceSettings
    {
        public ResourceType type;
        public ResourceStatus status; 

        public bool isProducer;
        public bool isConsumer;

        [Tooltip("Rate at which this system produces or consumes the resource per second")]
        public float rate = 0f;

        [Tooltip("Optimal rate for this system")]
        public float optimalRate = 0f;

        [Tooltip("Minimum rate needed for operation")]
        public float minRate = 0f;

        [Tooltip("Maximum rate before damage occurs")]
        public float maxRate = 0f;
    }

    // Network variables
    [NonSerialized] public NetworkVariable<float> CurrentStructuralIntregity = new NetworkVariable<float>(100.0f);
    [NonSerialized] public NetworkVariable<bool> ReceivingPower = new NetworkVariable<bool>(false);
    [NonSerialized] public NetworkVariable<bool> Online = new NetworkVariable<bool>(false);

    [Header("Basic System Settings")]
    [SerializeField] protected string _systemName;

    [Header("Resource Settings")]
    [SerializeField] protected List<ResourceSettings> _resourceSettings = new List<ResourceSettings>();

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
        // if (!IsServer)
        // {
        //     return;
        // }

        // if (CurrentStructuralIntregity.Value - data.Amount > 0)
        // {
        //     CurrentStructuralIntregity.Value -= data.Amount;

        //     if (CurrentStructuralIntregity.Value < _structuralFailureThreshold)
        //     {
        //         ReceivingPower.Value = false;
        //         OnResourceDepletion?.Invoke(this);
        //     }
        // }
        // else
        // {
        //     DamageEventSystem.Instance.NotifyDeath(gameObject);
        // }
        // OnDamaged?.Invoke(this);
    }

    public void SetPowerSource(AircraftSystem source)
    {
        powerSource = source; 
    }

}

