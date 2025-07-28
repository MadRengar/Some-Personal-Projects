// BTTacticalMovement.cs - 从AIAgentSettings统一读取配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTTacticalMovement : Action
{
    [Header("References")]
    public SharedTransform player;
    public SharedTransform nearestEnemy;

    private NavMeshAgent agent;
    private AIAnimationController animController;
    private AIAgentSettings agentSettings; // 统一配置来源

    private float lastUpdateTime = 0f;
    private Vector3 currentTarget;
    private bool hasValidTarget = false;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
        agentSettings = GetComponent<AIAgentSettings>(); // 获取配置组件
        currentTarget = transform.position;
        agent.speed = agentSettings.tacticalMoveSpeed;
    }

    public override TaskStatus OnUpdate()
    {
        if (player.Value == null || agent == null || agentSettings == null)
            return TaskStatus.Failure;

        // 从AIAgentSettings读取更新间隔
        float updateInterval = agentSettings.tacticalUpdateInterval;

        // 限制更新频率，提高性能
        if (Time.time - lastUpdateTime < updateInterval)
        {
            UpdateMovementState();
            return TaskStatus.Success;
        }

        lastUpdateTime = Time.time;

        // 计算战术移动目标
        Vector3 tacticalTarget = CalculateTacticalPosition();

        // 只有目标变化较大时才更新路径
        if (Vector3.Distance(currentTarget, tacticalTarget) > 1f)
        {
            currentTarget = tacticalTarget;
            agent.SetDestination(currentTarget);
            hasValidTarget = true;
        }

        UpdateMovementState();
        return TaskStatus.Success;
    }

    private Vector3 CalculateTacticalPosition()
    {
        Vector3 playerPos = player.Value.position;
        Vector3 currentPos = transform.position;

        float safeDistance = agentSettings.tacticalSafeDistance;
        float playerFollowWeight = agentSettings.playerFollowWeight;

        Vector3 targetPos = playerPos;

        if (nearestEnemy.Value != null)
        {
            Vector3 enemyPos = nearestEnemy.Value.position;
            float distToEnemy = Vector3.Distance(currentPos, enemyPos);

            if (distToEnemy < safeDistance)
            {
                Vector3 awayFromEnemy = (currentPos - enemyPos).normalized;
                Vector3 towardsPlayer = (playerPos - currentPos).normalized;

                Vector3 tacticalDirection = Vector3.Lerp(awayFromEnemy, towardsPlayer, playerFollowWeight);
                targetPos = currentPos + tacticalDirection * safeDistance; // 这里的safeDistance决定走多远
            }
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return playerPos;
    }


    private void UpdateMovementState()
    {
        if (animController == null || agentSettings == null) return;

        // 检查是否正在移动
        bool isMoving = agent.hasPath &&
                       agent.remainingDistance > agentSettings.stopFollowDistance &&
                       agent.velocity.magnitude > 0.1f;

        // 更新移动状态
        bool currentMoving = animController.HasStateFlag(AIAnimationController.AIStateFlags.Moving);
        if (isMoving != currentMoving)
        {
            animController.SetMoving(isMoving, 8f);
        }
    }
}