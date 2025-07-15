using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTDefaultCombatMovement : Action
{
    public SharedTransform nearestEnemy;
    public SharedString currentCommand;
    public SharedBool hasAmmo;
    public SharedTransform ammoSupplyPos;
    public SharedBool aiIsInsideAmmoSupply;

    [Header("Ammo Refill Settings")]
    private float refillTime; // 补给需要的累计时间

    private NavMeshAgent agent;
    private AIAnimationController animController;
    private AIAgentSettings agentSettings;
    private WeaponManager weaponManager;

    // 补给状态管理
    private static float accumulatedRefillTime = 0f; // 累计补给时间（和平时用）
    private static int refillVisitCount = 0; // 进入补给点的次数（战斗时用）
    private static bool isRefilling = false; // 是否正在补给
    private bool wasInRangeLastFrame = false; // 上一帧是否在范围内
    private int requiredVisitCount; // 战斗时需要的进入次数

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
        agentSettings = GetComponent<AIAgentSettings>();
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();

        agent.speed = agentSettings.defaultCombatMoveSpeed;
        refillTime = agentSettings.stayInSupplyAmmoAreaTime;
        requiredVisitCount = agentSettings.requiredVisitCount;
    }

    public override TaskStatus OnUpdate()
    {
        // 检查条件
        if (currentCommand.Value == null || currentCommand.Value == "" || currentCommand.Value == "unknown")
        {
            // 基础信息
            Vector3 myPos = transform.position;
            Vector3 ammoPos = ammoSupplyPos.Value.position;

            // 从AIAgentSettings读取安全距离
            float safeDistance = agentSettings != null ? agentSettings.minCombatDistance : 6f;
            float retreatDistance = agentSettings != null ? agentSettings.optimalCombatDistance : 8f;

            // 根据弹药状态决定移动策略
            if (hasAmmo.Value)
            {
                // 有弹药且有敌人：正常战斗移动
                if (nearestEnemy.Value != null)
                {
                    Vector3 enemyPos = nearestEnemy.Value.position;
                    float distToEnemy = Vector3.Distance(myPos, enemyPos);
                    HandleNormalCombatMovement(myPos, enemyPos, distToEnemy, safeDistance, retreatDistance);
                }
                else
                {
                    // 有弹药但没敌人：检查是否还在移动
                    float currentSpeed = agent.velocity.magnitude;
                    if (currentSpeed > 0.1f)
                    {
                        // 还在移动，保持移动动画
                        animController.SetMoving(true, currentSpeed);
                    }
                    else
                    {
                        // 已经停止，设置为静止
                        animController.SetMoving(false, 0);
                        agent.ResetPath();
                    }
                }

                // 重置补给状态
                ResetRefillState();
            }
            else
            {
                // 没弹药：无论是否有敌人都要去补给点
                Debug.Log("没有弹药，前往补给点！");

                if (nearestEnemy.Value != null)
                {
                    // 有敌人威胁的补给
                    Vector3 enemyPos = nearestEnemy.Value.position;
                    float distToEnemy = Vector3.Distance(myPos, enemyPos);
                    float distToAmmo = Vector3.Distance(myPos, ammoPos);
                    HandleTacticalRetreatToAmmo(myPos, enemyPos, ammoPos, distToEnemy, distToAmmo, safeDistance);
                }
                else
                {
                    // 没有敌人威胁的补给
                    HandlePeacefulAmmoRefill(myPos, ammoPos);
                }
            }

            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }

    /// <summary>
    /// 处理正常战斗移动（有弹药时）
    /// </summary>
    private void HandleNormalCombatMovement(Vector3 myPos, Vector3 enemyPos, float distToEnemy, float safeDistance, float retreatDistance)
    {
        if (distToEnemy < safeDistance)
        {
            // 敌人太近，后退
            Vector3 retreatDirection = (myPos - enemyPos).normalized;
            Vector3 retreatTarget = myPos + retreatDirection * (retreatDistance - distToEnemy + 1f);

            agent.SetDestination(retreatTarget);
            animController.SetMoving(true, agent.speed);
        }
        else
        {
            // 距离合适，停止移动
            animController.SetMoving(false, 0);
            agent.ResetPath();
        }
    }

    /// <summary>
    /// 处理和平时期的弹药补给（没有敌人威胁）- 使用计时机制
    /// </summary>
    private void HandlePeacefulAmmoRefill(Vector3 myPos, Vector3 ammoPos)
    {
        bool inAmmoRange = aiIsInsideAmmoSupply.Value;

        if (inAmmoRange)
        {
            // 在补给点内，使用计时补给
            HandleTimedAmmoRefill();
            agent.ResetPath();
            animController.SetMoving(false, 0);
        }
        else
        {
            // 不在补给范围内，直接前往补给点
            agent.SetDestination(ammoPos);
            animController.SetMoving(true, agent.speed);

            // 记录状态变化
            if (wasInRangeLastFrame)
            {
                Debug.Log("离开补给区域，和平补给暂停");
            }
        }

        wasInRangeLastFrame = inAmmoRange;
    }

    /// <summary>
    /// 处理战术撤退到补给点（没弹药时有敌人）- 使用次数机制
    /// </summary>
    private void HandleTacticalRetreatToAmmo(Vector3 myPos, Vector3 enemyPos, Vector3 ammoPos, float distToEnemy, float distToAmmo, float safeDistance)
    {
        // 使用触发器系统判断是否在补给点
        bool inAmmoRange = aiIsInsideAmmoSupply.Value;

        // 检查是否刚进入补给区域
        if (inAmmoRange && !wasInRangeLastFrame)
        {
            refillVisitCount++;
            Debug.Log($"进入补给区域第 {refillVisitCount} 次，需要 {requiredVisitCount} 次");

            // 检查是否达到所需次数
            if (refillVisitCount >= requiredVisitCount)
            {
                CompleteAmmoRefill();
                return;
            }
        }

        // 处理移动逻辑
        if (inAmmoRange)
        {
            // 在补给点内，检查敌人威胁
            if (distToEnemy < safeDistance)
            {
                // 敌人太近，先跑出去拉开距离
                Vector3 escapeTarget = CalculateEscapePosition(myPos, enemyPos, ammoPos, safeDistance);
                agent.SetDestination(escapeTarget);
                animController.SetMoving(true, agent.speed);

                Debug.Log("敌人接近，撤离补给点（次数已记录）");
            }
            else
            {
                // 安全距离内，可以短暂停留
                agent.ResetPath();
                animController.SetMoving(false, 0);
            }
        }
        else
        {
            // 不在补给范围内，移动到补给点
            if (distToEnemy < safeDistance)
            {
                // 敌人很近，使用战术移动（躲避+靠近补给点）
                Vector3 tacticalTarget = CalculateTacticalMoveTarget(myPos, enemyPos, ammoPos, distToEnemy, safeDistance);
                agent.SetDestination(tacticalTarget);
            }
            else
            {
                // 安全距离，直接前往补给点
                agent.SetDestination(ammoPos);
            }

            animController.SetMoving(true, agent.speed);
        }

        wasInRangeLastFrame = inAmmoRange;
    }

    /// <summary>
    /// 处理计时式弹药补给逻辑（和平时期）
    /// </summary>
    private void HandleTimedAmmoRefill()
    {
        // 检查弹药是否已满，如果满了就不需要补给
        if (weaponManager != null && weaponManager.GetWeaponData() != null)
        {
            int currentReserve = weaponManager.GetReserveAmmo();
            int maxAmmo = weaponManager.GetWeaponData().maxReserveAmmo;

            if (currentReserve >= maxAmmo)
            {
                Debug.Log("弹药已满，停止补给");
                ResetRefillState();
                return;
            }
        }

        if (!isRefilling)
        {
            isRefilling = true;
            Debug.Log($"开始和平补给... 当前进度: {accumulatedRefillTime:F1}s");
        }

        // 累计补给时间
        accumulatedRefillTime += Time.deltaTime;
        Debug.Log($"和平补给进度: {accumulatedRefillTime:F1}/{refillTime}s");

        // 检查是否完成补给
        if (accumulatedRefillTime >= refillTime)
        {
            CompleteAmmoRefill();
        }
    }

    /// <summary>
    /// 完成弹药补给
    /// </summary>
    private void CompleteAmmoRefill()
    {
        if (weaponManager != null && weaponManager.GetWeaponData() != null)
        {
            int maxAmmo = weaponManager.GetWeaponData().maxReserveAmmo;
            int currentReserve = weaponManager.GetReserveAmmo();
            int ammoToAdd = maxAmmo - currentReserve;

            if (ammoToAdd > 0)
            {
                weaponManager.AddReserveAmmo(ammoToAdd);
                Debug.Log($"弹药补给完成！补充了 {ammoToAdd} 发弹药");
            }
            else
            {
                Debug.Log("弹药已满，无需补给");
            }
        }

        ResetRefillState();
    }

    /// <summary>
    /// 重置补给状态
    /// </summary>
    private void ResetRefillState()
    {
        accumulatedRefillTime = 0f;
        refillVisitCount = 0;
        isRefilling = false;
        wasInRangeLastFrame = false;
    }

    /// <summary>
    /// 计算逃离位置（拉开与敌人的距离）
    /// </summary>
    private Vector3 CalculateEscapePosition(Vector3 myPos, Vector3 enemyPos, Vector3 ammoPos, float safeDistance)
    {
        // 计算远离敌人的方向
        Vector3 awayFromEnemy = (myPos - enemyPos).normalized;

        // 逃离距离应该足够拉开安全距离
        float escapeDistance = safeDistance + 3f; // 多跑3米确保安全
        Vector3 escapeTarget = myPos + awayFromEnemy * escapeDistance;

        // 确保在NavMesh上
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(escapeTarget, out navHit, 10f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        // 如果找不到逃离位置，尝试向补给点相反方向移动
        Vector3 awayFromAmmo = (myPos - ammoPos).normalized;
        Vector3 alternativeEscape = myPos + awayFromAmmo * escapeDistance;

        if (NavMesh.SamplePosition(alternativeEscape, out navHit, 10f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return myPos + awayFromEnemy * 5f; // 最后的备用选择
    }

    /// <summary>
    /// 计算战术移动目标（向补给点移动同时躲避敌人）
    /// </summary>
    private Vector3 CalculateTacticalMoveTarget(Vector3 myPos, Vector3 enemyPos, Vector3 ammoPos, float distToEnemy, float safeDistance)
    {
        Vector3 toAmmo = (ammoPos - myPos).normalized;

        if (distToEnemy < safeDistance)
        {
            // 敌人很近，需要更多地考虑躲避
            Vector3 awayFromEnemy = (myPos - enemyPos).normalized;

            // 混合两个方向：70%向补给点，30%远离敌人
            Vector3 mixedDirection = (toAmmo * 0.7f + awayFromEnemy * 0.3f).normalized;
            return myPos + mixedDirection * 5f;
        }
        else
        {
            // 敌人不太近，主要向补给点移动
            return myPos + toAmmo * 5f;
        }
    }
}