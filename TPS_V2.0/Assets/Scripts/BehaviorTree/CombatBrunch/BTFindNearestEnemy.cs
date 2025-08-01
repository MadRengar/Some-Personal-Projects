using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestEnemy : Conditional
{
    [Header("Enemy References")]
    public SharedTransform nearestEnemy; // 原有：离AI最近的敌人
    public SharedTransform nearestEnemyToPlayer; // 新增：离玩家最近的敌人
    public SharedTransform player; // 玩家引用

    [Header("State Variables")]
    public SharedBool hasAmmo; // 从黑板读取
    public SharedBool protectMode; // 保护模式标志位

    [Header("Settings")]
    public float sightRadius; // AI感知/射击范围
    public LayerMask enemyLayer; // 敌人检测层级

    private AIAgentSettings agentSettings;
    private AIAnimationController animController;

    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
        animController = GetComponent<AIAnimationController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agentSettings != null)
        {
            sightRadius = agentSettings.sightRadius;
        }else
        {
            Debug.Log("AIAgentSettings为空！");
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius, enemyLayer);
        float minDist = float.MaxValue;
        Transform target = null;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                //Debug.Log("找到敌人！" + col.name);  
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = col.transform;
                }
            }
        }

        if (protectMode.Value && player.Value != null)
        {
            float minDistToPlayer = float.MaxValue;
            Transform targetToPlayer = null;

            foreach (var col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    float dist = Vector3.Distance(player.Value.position, col.transform.position);
                    if (dist < minDistToPlayer)
                    {
                        minDistToPlayer = dist;
                        targetToPlayer = col.transform;
                    }
                }
            }

            nearestEnemyToPlayer.Value = targetToPlayer;

            if (targetToPlayer != null)
            {
                //Debug.Log($"[保护模式] 离玩家最近的敌人: {targetToPlayer.name}, 距离: {minDistToPlayer:F1}m");
            }
        }
        else
        {
            // 非保护模式下清空玩家敌人变量
            nearestEnemyToPlayer.Value = null;
        }



        if (target != null)
        {
            nearestEnemy.Value = target;
            return TaskStatus.Success;
        }
        else
        {
            nearestEnemy.Value = null;
            if (hasAmmo.Value && animController != null)
            {
                animController.SetFiring(false);
            }
            //animController.OnLostEnemyTarget();
            return TaskStatus.Failure;
        }
    }
}
