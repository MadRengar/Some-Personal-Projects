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
        {
            animController.SetFiring(true);
        }
            
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

    /* Fix
     * OnStart / OnEnd 的调用时机：
     * OnStart()：节点刚被激活（刚切换到这个节点）时调用一次
     * OnEnd()：节点刚被退出（切换到其他节点或自己返回）时调用一次
     * animController.OnStopFiring()、animController.SetIdle()，这会立刻把动画切回Idle或非战斗状态。
     * 下一帧又进入攻击节点，又触发OnStartFiring，刚开始播Firing动画，还没来得及播出来，立刻又被OnEnd切走了。
     * 表现结果就是动画状态切换根本来不及显现，每帧都被打断，于是你看到的效果就是动画完全切不了。
     */

    private void AITryFire()
    {
        // 构造AI自己的射线（模拟玩家摄像机射线，从AI角色头部/中心发射到敌人）
        Vector3 shootOrigin = weaponManager.firePoint.position;
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
        //Debug.Log("[AI Player] Firing!");
        weaponManager.TryFire(hit);
        weaponManager.cooldown = weaponManager.fireRate;
    }
}
