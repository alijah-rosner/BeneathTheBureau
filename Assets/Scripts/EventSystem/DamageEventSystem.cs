using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageEventSystem : NetworkBehaviour
{
    // Singleton instance
    public static DamageEventSystem Instance { get; private set; }

    // Registry for quick lookups
    private Dictionary<GameObject, I_Damageable> registry = new Dictionary<GameObject, I_Damageable>();

    // Events for monitoring/effects
    public event Action<SO_DamageData> OnDamageApplied;
    public event Action<SO_DamageData> OnRepairingApplied; 
    public event Action<GameObject> OnEntityDied;

    // Visual effect prefabs by damage type
    [SerializeField] private GameObject structuralDamageEffect;
    [SerializeField] private GameObject electricalDamageEffect;
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private GameObject explosionEffect;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Register a damageable entity
    public void Register(GameObject obj, I_Damageable damageable)
    {
        if (!registry.ContainsKey(obj))
        {
            registry.Add(obj, damageable);
        }
    }

    // Unregister a damageable entity
    public void Unregister(GameObject obj)
    {
        if (registry.ContainsKey(obj))
        {
            registry.Remove(obj);
        }
    }

    // Deal damage - the core functionality
    public void DealDamage(SO_DamageData data)
    {
        // On server, process directly
        if (IsServer)
        {
            ProcessDamage(data);
        }
        else
        {
            // On client, send to server via RPC
            NetworkObject targetNetObj = data.Target?.GetComponent<NetworkObject>();
            NetworkObject sourceNetObj = data.Source?.GetComponent<NetworkObject>();

            DealDamageServerRpc(
                targetNetObj ? targetNetObj.NetworkObjectId : 0,
                sourceNetObj ? sourceNetObj.NetworkObjectId : 0,
                data.Amount,
                (int)data.Type
            );
        }
    }

    private void ProcessDamage(SO_DamageData data)
    {
        if (data.Target == null) return;

        bool damageApplied = false;

        // Direct targeting - no broadcast
        if (registry.TryGetValue(data.Target, out I_Damageable damageable))
        {
            damageable.TakeDamage(data);
            damageApplied = true;
        }
        // Fallback
        else if (data.Target.TryGetComponent(out damageable))
        {
            damageable.TakeDamage(data);
            damageApplied = true;
        }

        if (damageApplied)
        {
            // Notify for effects
            OnDamageApplied?.Invoke(data);

        }
    }

    // Notify that an entity died
    public void NotifyDeath(GameObject entity)
    {
        OnEntityDied?.Invoke(entity);
    }


    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(ulong targetId, ulong sourceId, float amount,
                                     int damageType)
    {
        // Convert network IDs back to GameObjects
        GameObject target = null;
        GameObject source = null;

        if (targetId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObj))
        {
            target = targetObj.gameObject;
        }

        if (sourceId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(sourceId, out NetworkObject sourceObj))
        {
            source = sourceObj.gameObject;
        }

        // Create and process damage data
        SO_DamageData data = new SO_DamageData
        {
            Target = target,
            Source = source,
            Amount = amount,
            Type = (DamageType)damageType,
        };

        ProcessDamage(data);
    }
}