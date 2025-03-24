using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Game Event Object", menuName = "GameEvents/Generic Event")]
public class GameEvent<T> : ScriptableObject
{
    public string EventName;

    public event Action<T> OnEventTriggered;

    //private List<I_EventListener<T>> listenerList = new();

    public void TriggerEvent(T data)
    {

        OnEventTriggered?.Invoke(data);
    }

    public void RegisterListener(I_EventListener<T> listener)
    {
        OnEventTriggered += listener.OnEventTriggered;
    }

    public void UnregisterListener(I_EventListener<T> listener)
    {
        OnEventTriggered -= listener.OnEventTriggered;
    }
}


public class GameEventDetails
{
    // network object
    // transform
    // int
    // float
    // bool
    //....
}