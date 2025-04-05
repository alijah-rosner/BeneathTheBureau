using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class IntakeChamber : NetworkBehaviour, I_Interactable
{
    [SerializeField] private Float_GameEvent onIntakeAdded;

    [Header("Value bounds for output")]
    [SerializeField] private float IntakeMin;
    [SerializeField] private float IntakeMax;
    [SerializeField] private float DecayRate;

    private NetworkVariable<float> _intakeValue = new NetworkVariable<float>();
    private float _lastIntakeValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _intakeValue.Value = 0f;
        DecayRate = 1f;
        _lastIntakeValue = _intakeValue.Value;
        IntakeMin = 0f;
        IntakeMin = 100f;
        //onIntakeAdded?.TriggerEvent(OnIntakeChange(_intakeValue.Value));
    }

    // Update is called once per frame
    void Update()
    {
        if (_intakeValue.Value != _lastIntakeValue)
        {
            onIntakeAdded?.TriggerEvent(_intakeValue.Value);
            _lastIntakeValue = _intakeValue.Value;
        }
        UpdateIntakeValue();
    }

    // Decrement Intake by Decay Rate, clamp to bounds.
    private void UpdateIntakeValue()
    {
        if (_intakeValue.Value > IntakeMin && _intakeValue.Value < IntakeMax)
        {
            _intakeValue.Value -= DecayRate * Time.deltaTime;
            if (_intakeValue.Value > IntakeMax)
            {
                _intakeValue.Value = IntakeMax;
            }    
            else if (_intakeValue.Value < IntakeMin)
            {
                _intakeValue.Value = IntakeMin;

            }
        }
    }
    private void OnIntakeChange(float v)
    {
        _intakeValue.Value = v;
    }

    public void Interact(ulong netObjId)
    {
        _intakeValue.Value += 5f;
        Debug.Log("Yummo!");
    }
}
