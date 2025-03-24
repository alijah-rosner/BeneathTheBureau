using UnityEngine;
using System;
using System.Collections.Generic;

public class AircraftSystemGameEventListener : GameEventListener<AircraftSystem>
{
    public AircraftSystemGameEventListener(Action<AircraftSystem> response) : base(response)
    {
    }
}
