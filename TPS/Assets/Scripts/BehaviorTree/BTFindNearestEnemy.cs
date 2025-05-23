using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestEnemy : Conditional
{
    public SharedTransform nearestEnemy;

    public float sightRadius = 20f; // AI感知/射击范围
    public LayerMask enemyLayer;    // 仅检测敌人
    private AIAgentSettings agentSettings;

    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
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
                Debug.Log("找到敌人！" + col.name);
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
            return TaskStatus.Success;
        }
        else
        {
            nearestEnemy.Value = null;
            return TaskStatus.Failure;
        }
    }
}
