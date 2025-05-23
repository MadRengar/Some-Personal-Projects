using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTAimAtEnemy : Action
{
    public SharedTransform nearestEnemy;

    public override TaskStatus OnUpdate()
    {
        if (nearestEnemy.Value == null)
            return TaskStatus.Failure;

        Vector3 targetPos = nearestEnemy.Value.position;
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0; // 防止上下抖动

        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 20f);
        // 如果想更精准，可以等转向到一定角度后再射击
        return TaskStatus.Success;
    }
}
