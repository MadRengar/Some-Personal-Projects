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

    // 移除硬编码的配置，改为从AIAgentSettings读取
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

        if (agentSettings == null)
        {
            Debug.LogError("BTTacticalMovement: 未找到AIAgentSettings组件！");
            return;
        }

        currentTarget = transform.position;
        Debug.Log("AI开始战术移动（使用统一配置）");
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

            Debug.Log($"AI战术移动到: {tacticalTarget}");
        }

        UpdateMovementState();
        return TaskStatus.Success;
    }

    private Vector3 CalculateTacticalPosition()
    {
        Vector3 playerPos = player.Value.position;
        Vector3 currentPos = transform.position;

        // 从AIAgentSettings读取配置
        float safeDistance = agentSettings.tacticalSafeDistance;
        float playerFollowWeight = agentSettings.playerFollowWeight;
        float enemyAvoidWeight = agentSettings.enemyAvoidWeight;

        // 基础目标：玩家位置
        Vector3 targetDirection = (playerPos - currentPos).normalized;

        // 如果有敌人，计算战术调整
        if (nearestEnemy.Value != null)
        {
            Vector3 enemyPos = nearestEnemy.Value.position;
            float distToEnemy = Vector3.Distance(currentPos, enemyPos);

            if (distToEnemy < safeDistance)
            {
                // 敌人太近，计算既远离敌人又靠近玩家的方向
                Vector3 awayFromEnemy = (currentPos - enemyPos).normalized;
                Vector3 towardsPlayer = (playerPos - currentPos).normalized;

                // 混合两个方向：主要远离敌人，但倾向于玩家方向
                Vector3 tacticalDirection = Vector3.Lerp(awayFromEnemy, towardsPlayer, playerFollowWeight);
                targetDirection = tacticalDirection.normalized;

                Debug.Log($"战术后退：远离敌人 {enemyAvoidWeight} + 靠近玩家 {playerFollowWeight} (安全距离: {safeDistance})");
            }
        }

        // 计算目标位置
        float moveDistance = Mathf.Min(3f, Vector3.Distance(currentPos, playerPos));
        Vector3 tacticalTarget = currentPos + targetDirection * moveDistance;

        // 确保目标位置在NavMesh上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(tacticalTarget, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return playerPos; // 备选：直接前往玩家位置
    }

    private void UpdateMovementState()
    {
        if (animController == null || agentSettings == null) return;

        // 检查是否正在移动
        bool isMoving = agent.hasPath &&
                       agent.remainingDistance > agentSettings.stopDistance &&
                       agent.velocity.magnitude > 0.1f;

        // 更新移动状态
        bool currentMoving = animController.HasStateFlag(AIAnimationController.AIStateFlags.Moving);
        if (isMoving != currentMoving)
        {
            animController.SetMoving(isMoving);
            Debug.Log($"战术移动状态: {isMoving}");
        }
    }

    // 可选：在Scene视图中显示调试信息
    public override void OnDrawGizmos()
    {
        if (!hasValidTarget || agentSettings == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentTarget, 0.5f);
        Gizmos.DrawLine(transform.position, currentTarget);

        if (nearestEnemy.Value != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, agentSettings.tacticalSafeDistance);
        }
    }
}