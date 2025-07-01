using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTAimAtEnemy : Action
{
    public SharedTransform nearestEnemy;
    public GameObject aiAimTarget;
    private AIAnimationController animController;

    [Header("Aiming Settings")]
    public float aimTurnSpeed = 15f; // 降低转向速度，避免影响移动

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

        Vector3 targetPos = nearestEnemy.Value.position;
        aiAimTarget.transform.position = targetPos;


        //Vector3 dir = (targetPos - transform.position).normalized;
        //dir.y = 0; // 防止上下抖动
        //transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 20f);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        bool isMovingFast = agent != null && agent.velocity.magnitude > 1f;

        if (!isMovingFast)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * aimTurnSpeed);
        }
        // 如果想更精准，可以等转向到一定角度后再射击
        return TaskStatus.Success;
    }
    public override void OnEnd()
    {
        // 结束瞄准
        if (animController != null)
        {
            animController.SetAiming(false);
            Debug.Log("AI停止瞄准 - 设置Aiming标志为false");
        }
    }
}
