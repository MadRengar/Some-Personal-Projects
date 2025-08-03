using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTHasValidBuildingCommand : Conditional
{
    public SharedBool pingCommandActive; // 指令是否激活
    public SharedVector3 pingPosition; // Marker的位置
    public SharedTransform player;
    public SharedString currentCommand; // 当前指令
    public SharedBool isTargetBuilding; // 标记目标是否为建筑物

    private AIAgentSettings agentSettings;
    private PingMarkerManager pingManager;

    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
        pingManager = GameManager.Instance.GetPingMarkerManager();
    }

    public override TaskStatus OnUpdate()
    {
        if (pingCommandActive.Value)
        {
            if (currentCommand.Value == "repair_building" || currentCommand.Value == "defend_building")
            {
                if (pingPosition.Value == Vector3.zero)
                {
                    RadioPopController.Instance.ShowMessage(MessageKey.Ping_illegal, RadioPopController.MessageType.Error);
                    Debug.Log("清空指令");
                    currentCommand.Value = ""; // 清空指令
                    return TaskStatus.Failure;
                }

                // 检查标记类型，必须是建筑物才能执行建筑物任务
                if (!isTargetBuilding.Value)
                {
                    RadioPopController.Instance.ShowMessage(MessageKey.Ping_illegal, RadioPopController.MessageType.Error);
                    Debug.Log("清空指令");
                    currentCommand.Value = ""; // 清空指令
                    return TaskStatus.Failure;
                }

                // 检查建筑物是否仍然存在
                GameObject markedBuilding = null;
                if (pingManager != null)
                {
                    markedBuilding = pingManager.GetCurrentMarkedBuilding();
                    if (markedBuilding == null)
                    {
                        Debug.Log("[BTHasValidBuildingCommand] 标记的建筑物已消失");
                        Debug.Log("清空指令");
                        currentCommand.Value = ""; // 清空指令
                        return TaskStatus.Failure;
                    }
                }

                // 新增：检查建筑物是否需要维修（只对repair_building指令检查）
                if (currentCommand.Value == "repair_building")
                {
                    if (IsBuildingFullHealth(markedBuilding))
                    {
                        RadioPopController.Instance.ShowMessage(MessageKey.Building_FullHealth, RadioPopController.MessageType.Warning);
                        Debug.Log("清空指令");
                        currentCommand.Value = ""; // 清空指令
                        return TaskStatus.Failure;
                    }
                }

                RadioPopController.Instance.ShowMessage(MessageKey.PingMove_success, RadioPopController.MessageType.Info);
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }

    /// <summary>
    /// 检查建筑物是否已满血
    /// </summary>
    private bool IsBuildingFullHealth(GameObject building)
    {
        if (building == null) return true;

        // 检查不同类型建筑的血量
        TurretController turret = building.GetComponent<TurretController>();
        if (turret != null)
        {
            return turret.IsFullHealth();
        }

        GeneratorController generator = building.GetComponent<GeneratorController>();
        if (generator != null)
        {
            return generator.IsFullHealth();
        }

        StorageController storage = building.GetComponent<StorageController>();
        if (storage != null)
        {
            return storage.IsFullHealth();
        }

        return true; // 默认认为已满血，不需要维修
    }
}