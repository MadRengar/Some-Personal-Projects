using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTMoveToPosition : Action
{
    public SharedVector3 pingPosition;
    public SharedBool pingCommandActive;
    private NavMeshAgent agent;
    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        float dist = Vector3.Distance(agent.transform.position, pingPosition.Value);
        if (agent == null)
        {
            Debug.LogError("FollowPlayer: AI 缺少 NavMeshAgent 组件！");
            return TaskStatus.Failure;
        }
        if(dist > agent.stoppingDistance)
        {
            if(pingCommandActive.Value)
            {
                agent.SetDestination(pingPosition.Value);
                return TaskStatus.Running;
            }
            else
            {
                agent.ResetPath();
                Debug.LogError("指令取消！");
                return TaskStatus.Failure;
            }
        }
        else
        {
            agent.ResetPath(); // 停下来，不再持续 SetDestination
            return TaskStatus.Success;
        }
    }
}
