using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{

    [SerializeField] public PlaneComponent[] PlaneComponents;

    private Dictionary<PlaneComponent.PlaneComponentType, System.Action> _damageEffects;
    private Dictionary<PlaneComponent.PlaneComponentType, System.Action> _destructionEffects;
    private Dictionary<PlaneComponent.PlaneComponentType, System.Action> _repairEffects;

    private float _fuel = 100.0f;
    private float _fuelConsumptionRate = 1.0f;

    private float _thrust = 1.0f;
    private float _speed;

    private float _oxygen = 100.0f; 
    private float _oxygenConsumptionRate = 1.0f; 

    private float _energy = 100.0f;
    private float _energyConsumptionRate = 1.0f;

    private int _damagedEngines = 0;
    private int _destroyedEngines = 0;

    private int _damagedGuns = 0; /* hydraulics */
    private int _destroyedGuns = 0;

    public float Fuel => _fuel;
    public float FuelConsumptionRate => _fuelConsumptionRate;
    public float Thrust => _thrust;
    public float Speed => _speed;
    public float Oxygen => _oxygen;
    public float OxygenConsumptionRate => _oxygenConsumptionRate;
    public float Energy => _energy;
    public float EnergyConsumptionRate => _energyConsumptionRate;
    public float DamagedEngines => _damagedEngines;
    public float DestroyedEngines => _destroyedEngines;
    public float DamagedGuns => _damagedGuns;
    public float DestroyedGuns => DestroyedGuns; 


    private void Start()
    {
        _damageEffects = new()
        {
            { PlaneComponent.PlaneComponentType.Engine, HandleEngineDamaged },
            { PlaneComponent.PlaneComponentType.Navigation, HandleNavigationDamaged },
            { PlaneComponent.PlaneComponentType.Electrical, HandleElectricalDamaged },
            { PlaneComponent.PlaneComponentType.Hydraulics, HandleHydraulicsDamaged },
        };

        _destructionEffects = new()
        {
            { PlaneComponent.PlaneComponentType.Engine, HandleEngineDestruction },
            { PlaneComponent.PlaneComponentType.Navigation, HandleNavigationDestruction },
            { PlaneComponent.PlaneComponentType.Electrical, HandleElectricalDestruction },
            { PlaneComponent.PlaneComponentType.Hydraulics, HandleHydraulicsDestruction },
        };

        _repairEffects = new()
        {
            { PlaneComponent.PlaneComponentType.Engine, HandleEngineRepaired },
            { PlaneComponent.PlaneComponentType.Navigation, HandleNavigationRepaired },
            { PlaneComponent.PlaneComponentType.Electrical, HandleElectricalRepaired },
            { PlaneComponent.PlaneComponentType.Hydraulics, HandleHydraulicsRepaired },
        };


        foreach (var comp in PlaneComponents)
        {
            if (_damageEffects.ContainsKey(comp.componentType))
            {
                comp.OnDamaged += _damageEffects[comp.componentType];
                comp.OnRepaired += _repairEffects[comp.componentType];
                comp.OnDestruction += _destructionEffects[comp.componentType];
            }
        }
    }

    private void Update()
    {
    }

    private void OnDestroy()
    {
        foreach (var comp in PlaneComponents)
        {
            if (_damageEffects.ContainsKey(comp.componentType))
            {
                comp.OnDamaged -= _damageEffects[comp.componentType];
                comp.OnRepaired -= _repairEffects[comp.componentType];
                comp.OnDestruction -= _destructionEffects[comp.componentType];
            }
        }
    }


    ////////////////////////////////////////////* HANDLE SYSTEMS REPAIRED *////////////////////////////////////////////
    private void HandleHydraulicsRepaired()
    {
        throw new NotImplementedException();
    }

    private void HandleElectricalRepaired()
    {
        throw new NotImplementedException();
    }

    private void HandleNavigationRepaired()
    {
        throw new NotImplementedException();
    }

    private void HandleEngineRepaired()
    {
        throw new NotImplementedException();
    }

    ////////////////////////////////////////////* HANDLE SYSTEM DAMAGED *////////////////////////////////////////////
    private void HandleHydraulicsDamaged()
    {
        throw new NotImplementedException();
    }

    private void HandleElectricalDamaged()
    {
        throw new NotImplementedException();
    }

    private void HandleNavigationDamaged()
    {
        throw new NotImplementedException();
    }

    private void HandleEngineDamaged()
    {
        _damagedEngines++;
    }

    ////////////////////////////////////////////* HANDLE SYSTEM DESTRUCT *////////////////////////////////////////////
    private void HandleEngineDestruction()
    {
        // increase fuel consumption per engine destroyed
        // 
        _destroyedEngines--; 
    }

    private void HandleHydraulicsDestruction()
    {
        throw new NotImplementedException();
    }

    private void HandleElectricalDestruction()
    {
        throw new NotImplementedException();
    }

    private void HandleNavigationDestruction()
    {
        throw new NotImplementedException();
    }

}
