using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTMoveToAmmoPoint : Action
{
    public SharedTransform ammoPoint; // 固定的补给点位置

    private NavMeshAgent agent;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null) return TaskStatus.Failure;

        float distance = Vector3.Distance(transform.position, ammoPoint.Value.position);

        if (distance <= 2f) // 到达补给点
        {
            agent.ResetPath();
            return TaskStatus.Success;
        }

        agent.SetDestination(ammoPoint.Value.position);
        return TaskStatus.Running;
    }
}