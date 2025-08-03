using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BTMoveToSupplyAmmoPosition : Action
{
    [Header("Shared Variables")]
    public SharedString currentCommand;
    public SharedTransform ammoSupplyPos;
    public SharedBool aiIsInsideAmmoSupply;

    [Header("Ammo Refill Settings")]
    private float refillTime; // 补给需要的累计时间

    private NavMeshAgent agent;
    private AIAnimationController animController;
    private AIAgentSettings agentSettings;
    private WeaponManager weaponManager;
    private Animator animator;

    // 补给状态管理
    private float accumulatedRefillTime = 0f; // 累计补给时间
    private bool isRefilling = false; // 是否正在补给

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();
        agentSettings = GetComponent<AIAgentSettings>();
        animator = GetComponent<Animator>();
        // 获取WeaponManager
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();

        // 设置补给时间
        if (agentSettings != null)
        {
            refillTime = agentSettings.stayInSupplyAmmoAreaTime;
        }
        else
        {
            refillTime = 3.0f; // 默认补给时间
        }

        // 重置补给状态
        if (!aiIsInsideAmmoSupply.Value)
        {
            accumulatedRefillTime = 0f;  // 这会不断重置进度！
            isRefilling = false;
        }
    }

    public override TaskStatus OnUpdate()
    {
        // 检查命令是否仍然有效
        if (currentCommand == null || currentCommand.Value != "replenish_ammo")
        {
            return TaskStatus.Failure; // 明确返回Failure
        }

        // 检查补给点是否有效
        if (ammoSupplyPos == null || ammoSupplyPos.Value == null)
        {
            Debug.LogError("弹药补给点无效");
            return TaskStatus.Failure;
        }

        bool inAmmoRange = aiIsInsideAmmoSupply != null ? aiIsInsideAmmoSupply.Value : false;
        Debug.Log($"AI在补给区域内: {inAmmoRange}");

        if (inAmmoRange)
        {
            // 在补给区域内，开始补给
            HandleAmmoRefill();

            // 停止移动
            agent.ResetPath();
            animator.SetFloat("Speed", 0f);
            if (animController != null) animController.SetMoving(false, 0);
        }
        else
        {
            // 不在补给范围内，前往补给点
            Vector3 ammoPos = ammoSupplyPos.Value.position;

            if (agent != null)
            {
                agent.SetDestination(ammoPos);
            }

            if (animController != null)
            {
                float speed = agent != null ? agent.speed : 4f;
                animController.SetMoving(true, speed);
                Debug.Log($"设置移动动画，速度: {speed}");
            }
            // 重置补给进度
            accumulatedRefillTime = 0f;
            isRefilling = false;
        }

        Debug.Log("BTMoveToSupplyAmmoPosition 返回 TaskStatus.Running");
        return TaskStatus.Running;
    }

    /// <summary>
    /// 处理弹药补给
    /// </summary>
    private void HandleAmmoRefill()
    {
        if (!isRefilling)
        {
            isRefilling = true;
            Debug.Log("开始补给弹药...");
        }

        // 累计补给时间
        accumulatedRefillTime += Time.deltaTime;
        Debug.Log($"补给进度: {accumulatedRefillTime:F1}/{refillTime}s");

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
                Debug.Log("弹药已满，补给完成");
            }
        }
        else
        {
            Debug.LogWarning("WeaponManager无效，无法补给弹药");
        }

        // 清空命令，完成任务
        currentCommand.Value = "";
        Debug.Log("弹药补给任务完成");
    }

}