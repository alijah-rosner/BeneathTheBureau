using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class GameEventListener<T> : MonoBehaviour, I_EventListener<T>
{
    [Header("Game Event Settings")]
    [Tooltip("Event to register with")]
    [SerializeField] private GameEvent<T> _gameEvent;

    [Header("Game Event Response")]
    [Tooltip("Action to perform when the event is raised")]
    [SerializeField] public Action<T> _response;

    public GameEventListener(Action<T> response)
    {
        _response = response;
    }


    public void Initialize(Action<T> action)
    {
        _response = action;
    }

    protected virtual void OnEnable()
    {
        if (_gameEvent)
        {
            _gameEvent.RegisterListener(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (_gameEvent)
        {
            _gameEvent.UnregisterListener(this);
        }
    }

    public void OnEventTriggered(T data)
    {
        _response?.Invoke(data);
    }
}

// public class IntGameEventListener : GameEventListener<int>
// {
//     public IntGameEventListener(Action<int> response) : base(response)
//     {
//     }
// }

// public class Vector3GameEventListener : GameEventListener<Vector3>
// {
//     public Vector3GameEventListener(Action<Vector3> response) : base(response)
//     {
//     }
// }

// public class AircraftSystemGameEventListener : GameEventListener<AircraftSystem>
// {
//     public AircraftSystemGameEventListener(Action<AircraftSystem> response) : base(response)
//     {
//     }
// }

