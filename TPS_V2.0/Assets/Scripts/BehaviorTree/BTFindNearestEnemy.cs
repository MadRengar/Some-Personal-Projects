using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestEnemy : Conditional
{
    public SharedTransform nearestEnemy;
    public float sightRadius; // AI感知/射击范围
    public LayerMask enemyLayer;    // 仅检测敌人
    public SharedBool hasAmmo; // 从黑板读取

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

        if (target != null)
        {
            nearestEnemy.Value = target;
            // 发现敌人，切换到战斗待机状态
            //if (animController != null)
            //    animController.OnStartAiming();
            // 发现敌人时不再强制切换到瞄准状态，让BTAimAtEnemy节点来处理
            return TaskStatus.Success;
        }
        else
        {
            nearestEnemy.Value = null;
            //Debug.Log("BackToMove!");
            // 没有敌人，切换到idle状态
            if (hasAmmo.Value && animController != null)
            {
                animController.SetFiring(false);
            }
            //animController.OnLostEnemyTarget();
            return TaskStatus.Failure;
        }
    }
}
