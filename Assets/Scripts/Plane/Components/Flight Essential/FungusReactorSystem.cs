using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using NUnit.Framework;

public class FungusReactorSystem : AircraftSystem
{
    public enum ReactorState
    {
        Underfed,
        Balanced,
        Overfed
    }
       // [SerializeField] event Action<AircraftSystem> OnDamaged;
    
    

}
