using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTPatrolAroundPlayer : Action
{
    public SharedTransform player;
    public SharedTransform nearestEnemy;
    public SharedString currentCommand;

    // 不再设置参数，全部从AIAgentSettings读取
    private NavMeshAgent agent;
    private AIAnimationController animController;
    private AIAgentSettings agentSettings; // 统一配置来源

    private Vector3 currentPatrolTarget;
    private float lastDirectionChangeTime;
    private float arrivalTime;
    private bool waitingAtPoint = false;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
        agentSettings = GetComponent<AIAgentSettings>();

        if (agentSettings == null)
        {
            Debug.LogError("BTPatrolAroundPlayer: 未找到AIAgentSettings组件！");
            return;
        }

        GenerateNewPatrolPoint();
        lastDirectionChangeTime = Time.time;

        Debug.Log("AI开始巡逻（使用统一配置）");
    }

    public override TaskStatus OnUpdate()
    {
        if (!ShouldExecutePatrol())
        {
            return TaskStatus.Failure;
        }

        if (player.Value == null || agent == null || agentSettings == null)
            return TaskStatus.Failure;

        // 从AIAgentSettings读取配置
        float patrolRadius = agentSettings.patrolRadiusAroundPlayer;
        float waitTime = agentSettings.patrolWaitTime;
        float changeDirectionTime = agentSettings.idleDurationBeforePatrol * 2f; // 使用idle时间的2倍

        bool needNewPoint = Vector3.Distance(transform.position, currentPatrolTarget) < agentSettings.stopDistance ||
                           Time.time - lastDirectionChangeTime > changeDirectionTime;

        if (needNewPoint && !waitingAtPoint)
        {
            waitingAtPoint = true;
            arrivalTime = Time.time;
            agent.ResetPath();

            if (animController != null)
            {
                animController.SetMoving(false);
            }

            Debug.Log("AI到达巡逻点，开始等待");
        }

        if (waitingAtPoint)
        {
            if (Time.time - arrivalTime > waitTime)
            {
                GenerateNewPatrolPoint();
                waitingAtPoint = false;
                lastDirectionChangeTime = Time.time;
            }
        }
        else
        {
            if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < agentSettings.stopDistance))
            {
                agent.SetDestination(currentPatrolTarget);
                Debug.Log($"设置巡逻目标: {currentPatrolTarget}");
            }

            UpdateMovementState();
        }

        return TaskStatus.Success;
    }

    private bool ShouldExecutePatrol()
    {
        bool commandEmpty = currentCommand.Value == null ||
                           string.IsNullOrEmpty(currentCommand.Value) ||
                           currentCommand.Value.Trim() == "";

        bool noEnemy = nearestEnemy.Value == null;

        return commandEmpty && noEnemy;
    }

    private void GenerateNewPatrolPoint()
    {
        if (player.Value == null || agentSettings == null) return;

        // 从AIAgentSettings读取巡逻半径
        float patrolRadius = agentSettings.patrolRadiusAroundPlayer;

        Vector3 playerPos = player.Value.position;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(patrolRadius * 0.5f, patrolRadius);

        Vector3 randomDirection = new Vector3(
            Mathf.Cos(randomAngle) * randomRadius,
            0f,
            Mathf.Sin(randomAngle) * randomRadius
        );

        Vector3 targetPosition = playerPos + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, patrolRadius * 2f, NavMesh.AllAreas))
        {
            currentPatrolTarget = hit.position;
        }
        else
        {
            Vector3 forward = player.Value.forward * 3f;
            if (NavMesh.SamplePosition(playerPos + forward, out hit, 5f, NavMesh.AllAreas))
            {
                currentPatrolTarget = hit.position;
            }
            else
            {
                currentPatrolTarget = playerPos;
            }
        }
    }

    private void UpdateMovementState()
    {
        if (animController == null || agentSettings == null) return;

        bool isMoving = agent.hasPath &&
                       agent.remainingDistance > agentSettings.stopDistance &&
                       !agent.isStopped;

        if (isMoving != animController.HasStateFlag(AIAnimationController.AIStateFlags.Moving))
        {
            animController.SetMoving(isMoving);
            Debug.Log($"巡逻移动状态: {isMoving}");
        }
    }

    public override void OnEnd()
    {
        if (animController != null)
        {
            animController.SetMoving(false);
        }

        Debug.Log("AI结束巡逻");
    }
}