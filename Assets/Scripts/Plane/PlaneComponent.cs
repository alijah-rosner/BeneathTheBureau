using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneComponent : MonoBehaviour
{
    public enum PlaneComponentType
    {
        Default,
        Structure,
        Fuselage,
        Engine,
        Navigation,
        Electrical,
        Hydraulics
    }
    public PlaneComponentType componentType = PlaneComponentType.Default;
    public event Action OnDamaged;
    public event Action OnRepaired;
    public event Action OnDestruction;

    private float _durability = 100.0f;
    private float _maxDurability = 100.0f;
    public float Durability => _durability;
    
    public void Damage(float damage)
    {
        if (_durability - damage > 0)
        {
            _durability -= damage;
            //OnDamaged?.Invoke();
        }
        else
        {
            _durability = 0;
            Destroy(gameObject);
            //OnDestruction?.Invoke();
        }
    }

    public void Repair(float repairAmount)
    {
        if (_durability <= _maxDurability)
        {
            _durability += repairAmount;
            Mathf.Clamp(_durability, 0, 100);
            OnRepaired?.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Damage(50f); 
    }

}
