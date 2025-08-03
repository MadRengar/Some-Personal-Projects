using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTMoveToStorage : Action
{
    public SharedTransform nearestStorage;

    private NavMeshAgent agent;
    private AIAgentSettings agentSettings;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        agentSettings = GetComponent<AIAgentSettings>();
        agent.speed = agentSettings.findResourceSpeed;
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogError("[BTMoveToStorage] 缺少NavMeshAgent");
            return TaskStatus.Failure;
        }

        if (nearestStorage.Value == null)
        {
            Debug.LogWarning("[BTMoveToStorage] 没有目标仓库");
            return TaskStatus.Failure;
        }

        float stopDistance = 3.0f; // 仓库交互距离
        if (agentSettings != null)
            stopDistance = agentSettings.stopToStorageDis;

        float dist = Vector3.Distance(transform.position, nearestStorage.Value.position);

        if (dist > stopDistance)
        {
            agent.SetDestination(nearestStorage.Value.position);
            return TaskStatus.Running;
        }
        else
        {
            agent.ResetPath();
            return TaskStatus.Success;
        }
    }
}