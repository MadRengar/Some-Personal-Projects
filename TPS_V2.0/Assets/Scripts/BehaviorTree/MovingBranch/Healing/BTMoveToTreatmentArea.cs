using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;
public class BTMoveToTreatmentArea : Action
{
    public SharedTransform treatmentPos;
    public SharedString currentCommand;

    private NavMeshAgent agent;
    private AIAgentSettings settings;
    private AIAnimationController animController;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        settings = GetComponent<AIAgentSettings>();
        animController = GetComponent<AIAnimationController>();
        agent.speed = settings.pingMovinfSpeed;
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogError("FollowPlayer: AI 缺少 NavMeshAgent 组件！");
            return TaskStatus.Failure;
        }

        float dist = Vector3.Distance(agent.transform.position, treatmentPos.Value.position);

        if (dist >= agent.stoppingDistance)
        {
            Debug.Log("移动中！");
            if (currentCommand.Value == "go_heal") // 如果指令没有被玩家取消
            {
                agent.SetDestination(treatmentPos.Value.position);
                animController.SetMoving(true, 10f);
                return TaskStatus.Running;
            }
            else // 指令在agent移动中取消，停在原地
            {
                agent.ResetPath();
                animController.SetMoving(false, 0);
                currentCommand.Value = "";
                Debug.LogError("指令取消！");
                return TaskStatus.Failure;
            }
        }
        else // 到达目的地
        {
            Debug.Log("到达目的地！");
            animController.SetMoving(false, 0);
            agent.ResetPath(); // 停下来，不再持续 SetDestination
            currentCommand.Value = "";
            return TaskStatus.Success;
        }
    }
}
