using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class SystemButton_EventListener : GameEventListener<AircraftSystemButton>
{
    public SystemButton_EventListener(Action<AircraftSystemButton> response, GameEvent<AircraftSystemButton> trigger) :
        base(response, trigger)
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SystemButton_EventListener(Action<AircraftSystemButton> response) : base(response)
    {
    }
}