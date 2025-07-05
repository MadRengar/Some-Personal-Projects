using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTDefaultCombatMovement : Action
{
    public SharedTransform player;
    public SharedTransform nearestEnemy;
    public SharedString currentCommand;

    private NavMeshAgent agent;
    private AIAnimationController animController;
    private AIAgentSettings agentSettings;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
        agentSettings = GetComponent<AIAgentSettings>();
        agent.speed = agentSettings.defaultCombatMoveSpeed;
    }

    public override TaskStatus OnUpdate()
    {
        // 检查条件
        if (currentCommand.Value == null || currentCommand.Value == "" || currentCommand.Value == "unknown")
        {
            if (nearestEnemy.Value != null)
            {

                // 计算移动位置
                Vector3 enemyPos = nearestEnemy.Value.position;
                Vector3 playerPos = player.Value.position;
                Vector3 myPos = transform.position;

                float distToEnemy = Vector3.Distance(myPos, enemyPos);

                // 从AIAgentSettings读取安全距离
                float safeDistance = agentSettings != null ? agentSettings.minCombatDistance : 6f;
                float retreatDistance = agentSettings != null ? agentSettings.optimalCombatDistance : 8f;

                animController.SetMoving(true, agent.speed);
                //animController.SetMoveDirection(1f); // 后退

                // 如果距离敌人太近，立即后退
                if (distToEnemy < safeDistance)
                {
                    Vector3 retreatDirection = (myPos - enemyPos).normalized;
                    Vector3 retreatTarget = myPos + retreatDirection * (retreatDistance - distToEnemy + 1f);

                    // 使用NavMeshAgent设置目标
                    if (agent != null)
                    {
                        agent.SetDestination(retreatTarget);
                    }
                    animController.SetMoving(true, agent.speed);
                    //animController.SetMoveDirection(-1f); // 后退

                }
                else
                {
                    animController.SetMoving(false, 0);

                }

                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }
}