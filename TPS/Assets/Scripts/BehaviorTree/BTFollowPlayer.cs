using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTFollowPlayer : Action
{
    public SharedTransform player;

    private NavMeshAgent agent;
    /*
     “Node 脚本与其他脚本通信”并不是行为树设计的推荐模式。
      通常，Behavior Designer 的 Task（行为树节点）应当是“使用外部数据”，而不是成为“数据提供者”。
     */
    private float stopDistance; // 跟随玩家的距离
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
        float dist = Vector3.Distance(transform.position, player.Value.position);

        if (agentSettings != null)
        {
            stopDistance = agentSettings.stopDistance;
            //agent.stoppingDistance = stopDistance;
        }

        if (player.Value == null)
        {
            Debug.LogWarning("FollowPlayer: player 未设置！");
            return TaskStatus.Failure;
        }

        if (agent == null)
        {
            Debug.LogError("FollowPlayer: AI 缺少 NavMeshAgent 组件！");
            return TaskStatus.Failure;
        }

        if (dist > stopDistance)
        {
            agent.SetDestination(player.Value.position);
            // 设置移动状态标志
            if (animController != null)
            {
                animController.SetMoving(true, 10f);
            }
            return TaskStatus.Success;
        }
        else
        {
            agent.ResetPath(); // 停下来，不再持续 SetDestination

            if (animController != null)
            {
                animController.SetMoving(false, 0);
            }
            return TaskStatus.Success;
        }
    }
}
