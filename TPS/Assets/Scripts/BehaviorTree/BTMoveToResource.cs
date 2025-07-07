using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTMoveToResource : Action
{
    // 行为树黑板变量：目标资源的Transform
    public SharedTransform nearestResource;

    private NavMeshAgent agent;
    private AIAgentSettings agentSettings;
    private AIAnimationController animController;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        agentSettings = GetComponent<AIAgentSettings>();
        animController = GetComponent<AIAnimationController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogError("BTMoveToResource: 缺少NavMeshAgent！");
            return TaskStatus.Failure;
        }

        if (nearestResource.Value == null)
        {
            Debug.LogWarning("BTMoveToResource: 没有目标资源！");
            return TaskStatus.Failure;
        }

        float stopDistance = 2.0f;
        if (agentSettings != null)
            stopDistance = agentSettings.stopDistance;

        float dist = Vector3.Distance(transform.position, nearestResource.Value.position);

        // 是否到达资源
        if (dist > stopDistance)
        {
            agent.SetDestination(nearestResource.Value.position);
            animController.SetMoving(true, 7f);
            return TaskStatus.Running;
        }
        else
        {
            agent.ResetPath();
            animController.SetMoving(true, 7f);
            return TaskStatus.Success;
        }
    }
}
