using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTMoveToAmmoPoint : Action
{
    public SharedTransform ammoPoint;
    public SharedBool aiIsInsideAmmoSupply;
    private NavMeshAgent agent;
    private AIAnimationController animController;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null) return TaskStatus.Failure;

        if (!aiIsInsideAmmoSupply.Value)
        {
            agent.SetDestination(ammoPoint.Value.position);

            float speed = agent.velocity.magnitude;
            animController.SetMoving(speed > 0.1f, speed);

            return TaskStatus.Running;
        }
        else // 到达目标点
        {
            Debug.Log("到达目的地");
            agent.ResetPath();
            animController.SetMoving(false, 0f);
            return TaskStatus.Success;
        }
    }
}