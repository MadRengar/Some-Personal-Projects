using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTAimAtEnemy : Action
{
    [Header("Enemy Targets")]
    public SharedTransform nearestEnemy; // 离AI最近的敌人
    public SharedTransform nearestEnemyToPlayer; // 离玩家最近的敌人

    [Header("State")]
    public SharedBool protectMode; // 保护模式标志位

    [Header("Aim Settings")]
    public GameObject aiAimTarget;
    public float aimTurnSpeed = 15f; // 降低转向速度，避免优先移动

    public override TaskStatus OnUpdate()
    {
        // 根据保护模式选择瞄准目标
        Transform targetEnemy = GetAimTarget();

        if (targetEnemy == null)
        {
            return TaskStatus.Failure;
        }

        Vector3 targetPos = targetEnemy.position;
        aiAimTarget.transform.position = targetPos;

        // 检查AI是否在快速移动，移动时不转向
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        bool isMovingFast = agent != null && agent.velocity.magnitude > 1f;

        if (!isMovingFast)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0; // 防止上下倾斜
            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * aimTurnSpeed);
        }

        // 如果更精确瞄准，可以等转向到一定角度后再射击
        return TaskStatus.Success;
    }

    /// <summary>
    /// 根据保护模式选择瞄准目标
    /// </summary>
    private Transform GetAimTarget()
    {
        if (protectMode.Value && nearestEnemyToPlayer.Value != null)
        {
            // 保护模式：优先瞄准威胁玩家的敌人
            //Debug.Log($"[保护模式] 瞄准目标: {nearestEnemyToPlayer.Value.name}（威胁玩家）");
            return nearestEnemyToPlayer.Value;
        }
        else if (nearestEnemy.Value != null)
        {
            // 普通模式：瞄准最近的敌人
            //Debug.Log($"[普通模式] 瞄准目标: {nearestEnemy.Value.name}（最近敌人）");
            return nearestEnemy.Value;
        }

        return null;
    }
}
