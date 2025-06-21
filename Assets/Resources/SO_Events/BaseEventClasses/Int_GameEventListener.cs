using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public class Int_GameEventListener : GameEventListener<int>
{
    public Int_GameEventListener(Action<int> response, GameEvent<int> trigger) : base(response, trigger){ }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Int_GameEventListener(Action<int> response) : base(response)
    {
    }
}