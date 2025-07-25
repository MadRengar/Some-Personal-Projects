using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTRepairBuilding : Action
{
    [Header("Shared Variables")]
    public SharedString currentCommand; // 当前指令
    public SharedBool pingCommandActive; // 指令是否激活

    [Header("Repair Settings")]
    public float swingInterval = 2f; // 挥击间隔

    private AIAnimationController animController;
    private AIHammerController hammerController;
    private PingMarkerManager pingManager;
    private bool hasStartedRepair = false;
    private float swingTimer = 0f;

    public override void OnStart()
    {
        // 只在第一次进入时初始化状态
        if (!hasStartedRepair)
        {
            swingTimer = 0f;
            hasStartedRepair = true;
        }

        // 获取组件引用
        animController = GetComponent<AIAnimationController>();
        hammerController = GetComponent<AIHammerController>();
        pingManager = GameManager.Instance.GetPingMarkerManager();
    }

    public override TaskStatus OnUpdate()
    {
        // 检查指令是否仍然有效
        if (!pingCommandActive.Value || currentCommand.Value != "repair_building")
        {
            Debug.Log("[BTRepairBuilding] 维修指令已取消");
            StopRepair();
            return TaskStatus.Failure;
        }

        // 检查是否仍然标记着建筑物
        if (pingManager != null && !pingManager.IsCurrentTargetBuilding())
        {
            Debug.Log("[BTRepairBuilding] 不再标记建筑物");
            StopRepair();
            return TaskStatus.Failure;
        }

        // 检查建筑物是否仍然存在
        GameObject targetBuilding = null;
        if (pingManager != null)
        {
            targetBuilding = pingManager.GetCurrentMarkedBuilding();
            if (targetBuilding == null)
            {
                Debug.LogWarning("[BTRepairBuilding] 目标建筑物已消失");
                StopRepair();
                return TaskStatus.Failure;
            }
        }

        // 检查建筑物是否已满血（维修过程中的实时检查）
        if (hammerController != null && hammerController.IsTargetBuildingFullHealth())
        {
            Debug.Log("[BTRepairBuilding] 建筑物维修完成，已满血");
            StopRepair();
            currentCommand.Value = ""; // 任务完成，清空指令
            return TaskStatus.Success;
        }

        // 如果刚开始维修，则启动维修状态
        if (hasStartedRepair && animController != null && !animController.HasStateFlag(AIAnimationController.AIStateFlags.Repairing))
        {
            StartRepair();
        }

        // 处理挥击计时
        swingTimer += Time.deltaTime;
        if (swingTimer >= swingInterval)
        {
            TriggerHammerSwing();
            swingTimer = 0f;
        }

        return TaskStatus.Running;
    }

    /// <summary>
    /// 开始维修
    /// </summary>
    private void StartRepair()
    {
        Debug.Log("[BTRepairBuilding] AI开始维修建筑物");

        // 启动维修动画和武器切换
        if (animController != null)
        {
            animController.SetRepairing(true);
        }

        // 显示目标建筑信息
        if (pingManager != null)
        {
            GameObject targetBuilding = pingManager.GetCurrentMarkedBuilding();
            if (targetBuilding != null)
            {
                Debug.Log($"[BTRepairBuilding] 开始维修建筑物: {targetBuilding.name}");
            }
        }
    }

    /// <summary>
    /// 触发锤子挥击
    /// </summary>
    private void TriggerHammerSwing()
    {
        if (animController != null)
        {
            animController.TriggerHammerSwing();
            Debug.Log("[BTRepairBuilding] 触发锤子挥击动画");
        }
        else
        {
            Debug.LogError("[BTRepairBuilding] AIAnimationController为空！");
        }
    }

    /// <summary>
    /// 停止维修
    /// </summary>
    private void StopRepair()
    {
        if (hasStartedRepair)
        {
            Debug.Log("[BTRepairBuilding] 停止维修");

            // 停止维修动画和武器切换
            if (animController != null)
            {
                animController.SetRepairing(false);
            }

            // 重置状态，准备下次维修
            hasStartedRepair = false;
            swingTimer = 0f;
        }
    }

    public override void OnEnd()
    {
        Debug.Log("[BTRepairBuilding] 退出维修节点");
        StopRepair();
    }

    /// <summary>
    /// 获取维修进度信息（用于调试）
    /// </summary>
    public string GetRepairStatus()
    {
        if (!hasStartedRepair)
            return "未开始维修";

        if (hammerController != null && hammerController.IsTargetBuildingFullHealth())
            return "建筑已满血";

        return $"维修中 (下次挥击: {swingInterval - swingTimer:F1}s)";
    }
}