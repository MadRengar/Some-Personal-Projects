using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTAimAtEnemy : Action
{
    public SharedTransform nearestEnemy;
    public GameObject aiAimTarget;
    private AIAnimationController animController;

    public override void OnStart()
    {
        animController = GetComponent<AIAnimationController>();
        // 开始瞄准
        if (animController != null)
            animController.OnStartAiming();
    }

    public override TaskStatus OnUpdate()
    {
        if (nearestEnemy.Value == null)
        {
            return TaskStatus.Failure;
        }
        //Debug.Log("Aiming!");
        Vector3 targetPos = nearestEnemy.Value.position;
        aiAimTarget.transform.position = targetPos;
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0; // 防止上下抖动
        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 20f);

        // 如果想更精准，可以等转向到一定角度后再射击
        return TaskStatus.Success;
    }
    //public override void OnEnd()
    //{
    //    // 停止瞄准，回到战斗待机
    //    if (animController != null)
    //        animController.SetIdle();
    //}
}
