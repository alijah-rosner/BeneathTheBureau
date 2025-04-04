using System;
using UnityEngine;

public class GameObjectEventListener : GameEventListener<GameObject>
{
    public GameObjectEventListener(Action<GameObject> response, GameEvent<GameObject> gameEvent) : base(response, gameEvent)
    {
    }
    public GameObjectEventListener(Action<GameObject> response) : base(response)
    {
    }

}
