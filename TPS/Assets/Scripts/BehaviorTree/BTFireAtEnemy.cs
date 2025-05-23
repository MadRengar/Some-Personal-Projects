using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Collections;

public class BTFireAtEnemy : Action
{
    public SharedTransform nearestEnemy;
    public WeaponManager weaponManager; // 手动拖引用
    private AIAnimationController animController;
    public override void OnStart()
    {
        animController = GetComponent<AIAnimationController>();
        // 开始射击
        if (animController != null)
            animController.OnStartFiring();
    }
    public override TaskStatus OnUpdate()
    {
        if (nearestEnemy.Value == null)
        {
            Debug.Log("nearestEnemy == null");
            return TaskStatus.Failure;
        }
        else if (weaponManager == null)
        {
            Debug.Log("weaponManager == null");
            return TaskStatus.Failure;
        }
        AITryFire();
        return TaskStatus.Success;
    }

    public override void OnEnd()
    {
        // 射击结束，回到战斗待机
        if (animController != null)
            animController.OnStopFiring();
    }

    private void AITryFire()
    {
        // 构造AI自己的射线（模拟玩家摄像机射线，从AI角色头部/中心发射到敌人）
        Vector3 shootOrigin = weaponManager.originShootPosition.position;
        Vector3 shootTarget = nearestEnemy.Value.position + Vector3.up * 1f;
        Vector3 shootDir = (shootTarget - shootOrigin).normalized;

        // 用Physics.Raycast模拟命中点
        RaycastHit hit;
        bool hasHit = Physics.Raycast(shootOrigin, shootDir, out hit, 100f, LayerMask.GetMask("EnemyLayer"));

        if (!hasHit)
        {
            // 填补射击点，不赋值collider
            hit = new RaycastHit();
            // 不能赋值collider, 只用point和normal时可以通过WeaponManager内部做默认处理
        }


        if(weaponManager.cooldown > 0f)
        {
            return;
        }

        // 触发AI开火
        Debug.Log("Firing!");
        weaponManager.TryFire(hit);
        weaponManager.cooldown = weaponManager.fireRate;
    }
}
