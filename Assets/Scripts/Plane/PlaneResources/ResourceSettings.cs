using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;


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

    public ResourceSettings(ResourceType t, ResourceStatus s, bool isProd, float r)
    {
        type = t;
        status = s;
        isProducer = isProd;
        rate = r;
    }
    public ResourceSettings(ResourceType t, ResourceStatus s, bool isProd, float r,
        float optRate, float minR, float maxR)
    {
        type = t;
        status = s;
        isProducer = isProd;
        rate = r;
        optimalRate = optRate;
        minRate = minR;
        maxRate = maxR;
    }
    [SerializeReference] public Float_GameEvent onValueChangeEvent;
    public ResourceType type;
    public ResourceStatus status;

    public bool isProducer;
    //    public bool isConsumer;
    [Tooltip("System output value")]
    private float _output;
    public float _prevOutput;
    private float _serverOutput;
    public float Output
    {
        get => _output;
        set => _output = Mathf.Max(0f, value);
    }

    [Tooltip("Rate at which this system produces or consumes the resource per second")]
    public float rate = 0f;

    [Tooltip("Optimal rate for this system")]
    public float optimalRate = 0f;

    [Tooltip("Minimum rate needed for operation")]
    public float minRate = 0f;

    [Tooltip("Maximum rate before damage occurs")]
    public float maxRate = 0f;

    public void SetEffectiveOutput(float val)
    {
        _serverOutput = val;
    }

    public float GetEffectiveOutput()
    {
        return _serverOutput;
    }

}