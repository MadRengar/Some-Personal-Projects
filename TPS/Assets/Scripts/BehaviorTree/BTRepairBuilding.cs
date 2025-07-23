using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTRepairBuilding : Action
{
    public SharedString currentCommand; // 当前指令

    private bool hasStartedRepair = false;

    public override void OnStart()
    {
        hasStartedRepair = false;
        Debug.Log("进入维修节点");
    }

    public override TaskStatus OnUpdate()
    {
        // 检查指令是否仍然有效
        if (currentCommand.Value != "repair_building")
        {
            return TaskStatus.Failure;
        }

        // 如果还没开始维修，则触发维修
        if (!hasStartedRepair)
        {
            StartRepair();
            hasStartedRepair = true;
        }

        // 持续维修状态
        return TaskStatus.Running;
    }

    private void StartRepair()
    {
        Debug.Log("AI开始维修建筑物");

        // 触发维修动画和逻辑的接口
        // 你可以在这里添加你的维修逻辑

        // 示例：获取动画控制器并触发维修
        var animController = GetComponent<AIAnimationController>();
        if (animController != null)
        {
            Debug.Log($"开始维修建筑物动画！");
            // animController.SetRepairing(); // 你需要在动画控制器中添加这个方法
        }

        // 示例：获取目标建筑物并开始维修
        var pingManager = GameManager.Instance.GetPingMarkerManager();
        if (pingManager != null)
        {
            GameObject targetBuilding = pingManager.GetCurrentMarkedBuilding();
            if (targetBuilding != null)
            {
                Debug.Log($"开始维修建筑物: {targetBuilding.name}");
                // 在这里添加你的维修逻辑
            }
        }
    }
}