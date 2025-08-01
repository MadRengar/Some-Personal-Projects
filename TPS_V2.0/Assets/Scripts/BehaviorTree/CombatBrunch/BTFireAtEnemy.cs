using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Collections;

public class BTFireAtEnemy : Action
{
    [Header("Enemy Targets")]
    public SharedTransform nearestEnemy; // 离AI最近的敌人
    public SharedTransform nearestEnemyToPlayer; // 离玩家最近的敌人

    [Header("State")]
    public SharedBool protectMode; // 保护模式标志位

    [Header("Weapon")]
    public WeaponManager weaponManager; // 武器引用

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
        // 根据保护模式选择攻击目标
        Transform targetEnemy = GetAttackTarget();

        if (targetEnemy == null)
        {
            Debug.Log("没有有效的攻击目标");
            return TaskStatus.Failure;
        }

        if (weaponManager == null)
        {
            Debug.Log("weaponManager == null");
            return TaskStatus.Failure;
        }

        // 攻击选定的目标
        AITryFire(targetEnemy);
        return TaskStatus.Success;
    }

    /// <summary>
    /// 根据保护模式选择攻击目标
    /// </summary>
    private Transform GetAttackTarget()
    {
        if (protectMode.Value && nearestEnemyToPlayer.Value != null)
        {
            // 保护模式：优先攻击威胁玩家的敌人
            Debug.Log($"[保护模式] 攻击目标: {nearestEnemyToPlayer.Value.name}（威胁玩家）");
            return nearestEnemyToPlayer.Value;
        }
        else if (nearestEnemy.Value != null)
        {
            // 普通模式：攻击最近的敌人
            //Debug.Log($"[普通模式] 攻击目标: {nearestEnemy.Value.name}（最近敌人）");
            return nearestEnemy.Value;
        }

        return null;
    }

    private void AITryFire(Transform target)
    {
        // 根据目标敌人的位置进行射击
        Vector3 shootOrigin = weaponManager.firePoint.position;
        Vector3 shootTarget = target.position + Vector3.up * 1f;
        Vector3 shootDir = (shootTarget - shootOrigin).normalized;

        // 用Physics.Raycast检测命中点
        RaycastHit hit;
        bool hasHit = Physics.Raycast(shootOrigin, shootDir, out hit, 100f, LayerMask.GetMask("EnemyLayer"));

        if (!hasHit)
        {
            // 没有击中点，不到达collider
            hit = new RaycastHit();
            // 不能到达collider, 没有point和normal时可以通过WeaponManager内部自行默认处理
        }

        if (weaponManager.cooldown > 0f)
        {
            return;
        }

        // 触发AI开火
        //Debug.Log("[AI Player] Firing!");
        weaponManager.TryFire(hit);
        weaponManager.cooldown = weaponManager.fireRate;
    }
}