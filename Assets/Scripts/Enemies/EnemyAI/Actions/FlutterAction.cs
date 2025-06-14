using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Flutter", story: "Flap your wings to fly!", category: "Action", id: "0ca61865aef0debdb0385fe29d9440a3")]
public partial class FlutterAction : Action
{
    public float flyTime;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

