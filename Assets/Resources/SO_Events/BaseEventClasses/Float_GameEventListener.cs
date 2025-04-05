using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class Float_GameEventListener : GameEventListener<float>
{
    public Float_GameEventListener(Action<float> response, GameEvent<float> trigger) : base(response, trigger){ }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Float_GameEventListener(Action<float> response) : base(response)
    {
    }

}
