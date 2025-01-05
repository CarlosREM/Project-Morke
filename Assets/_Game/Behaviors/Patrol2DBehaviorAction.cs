using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Patrol2D", 
    story: "[Agent] patrols along [Waypoints] (2D)", 
    category: "Action/Navigation", 
    id: "1297aad5dee6ef870ea540f608206b0b")]
public partial class Patrol2DBehaviorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
    [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new BlackboardVariable<float>(1.0f);

    private Rigidbody2D _rb;

    [CreateProperty]
    private Vector3 _currentTarget;
    [CreateProperty]
    private int _currentPatrolPoint = 0;
    [CreateProperty]
    private bool _waiting;
    [CreateProperty]
    private float _waypointWaitTimer;
    
    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        if (Waypoints.Value == null || Waypoints.Value.Count == 0)
        {
            LogFailure("No waypoints to patrol assigned.");
            return Status.Failure;
        }

        Initialize();

        _waiting = false;
        _waypointWaitTimer = 0.0f;

        MoveToNextWaypoint();
        return Status.Running;
    }


    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Waypoints.Value == null)
        {
            return Status.Failure;
        }

        if (_waiting)
        {
            if (_waypointWaitTimer > 0.0f)
            {
                _waypointWaitTimer -= Time.deltaTime;
            }
            else
            {
                _waypointWaitTimer = 0f;
                _waiting = false;
                MoveToNextWaypoint();
            }
        }
        else
        {
            float distance = GetDistanceToWaypoint();
            Vector3 agentPosition = Agent.Value.transform.position;

            if (distance <= DistanceThreshold)
            {
                _waypointWaitTimer = WaypointWaitTime.Value;
                _waiting = true;
            }
            else 
            {
                Vector3 toDestination = _currentTarget - agentPosition;
                toDestination.z = 0.0f;
                toDestination.Normalize();

                if (_rb)
                {
                    if (_rb.linearVelocity.magnitude  < Speed)
                        _rb.AddForce(toDestination * Speed);
                }
                else
                {
                    float speed = Mathf.Min(Speed, distance);

                    agentPosition += toDestination * (speed * Time.deltaTime);
                    Agent.Value.transform.position = agentPosition;
                    Agent.Value.transform.forward = toDestination;
                }
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    protected override void OnDeserialize()
    {
        Initialize();
    }

    private void Initialize()
    {
        _rb = Agent.Value.GetComponent<Rigidbody2D>();

        _currentPatrolPoint = 0;
    }

    private float GetDistanceToWaypoint()
    {
        Vector3 targetPosition = _currentTarget;
        Vector3 agentPosition = Agent.Value.transform.position;
        agentPosition.y = targetPosition.y; // Ignore y for distance check.
        return Vector3.Distance(
            agentPosition,
            targetPosition
        );
    }

    private void MoveToNextWaypoint()
    {
        _currentTarget = Waypoints.Value[_currentPatrolPoint++].transform.position;

        if (_currentPatrolPoint >= Waypoints.Value.Count)
            _currentPatrolPoint = 0;
    }
}

