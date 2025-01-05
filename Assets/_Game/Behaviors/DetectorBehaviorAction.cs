using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectorBehaviorAction", story: "[Actor] detects [Target]", category: "Action", id: "7300cefdc1e2c8bec413c0446575da54")]
public partial class DetectorBehaviorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Actor;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private ObjectDetector _sensor;
    
    protected override Status OnStart()
    {
        return Status.Running;
    }

    private void Initialize()
    {
        
    }
    
    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

