using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestResource : Conditional
{
    // 行为树共享变量：输出最近资源的Transform
    public SharedTransform nearestResource;
    public SharedString currentCommand; // 当前指令
    public SharedBool needStorage;

    // 添加对背包管理器的引用
    private InventoryManager inventoryManager;
    private AIAgentSettings agentSettings;

    public override void OnStart()
    {
        // 获取背包管理器组件
        inventoryManager = GameManager.Instance?.GetInventoryManager();
        agentSettings = GetComponent<AIAgentSettings>();
        if (inventoryManager == null)
        {
            Debug.LogError("[BTFindNearestResource] 无法获取InventoryManager");
        }
    }

    public override TaskStatus OnUpdate()
    {
        // 查找所有激活的资源
        var pickups = GameObject.FindObjectsOfType<PickupItem>()
            .Where(p => p != null && p.gameObject.activeInHierarchy)
            .ToList();

        if (pickups.Count == 0)
        {
            nearestResource.Value = null;
            return TaskStatus.Failure;
        }

        bool match(ResourceType type)
        {
            // 根据Prompt文件来让行为与ResourceType的映射来写
            switch (currentCommand.Value)
            {
                case "collect_wood":
                    return type == ResourceType.Wood;
                case "collect_metal":
                case "collect_iron":  // 兼容多个指令名
                    return type == ResourceType.Iron;
                case "collect_ammo":
                    return type == ResourceType.Ammo;
                case "collect_food":
                    return type == ResourceType.Food;
                case "collect_all":
                    return true;
                default:
                    // 未识别命令
                    return false;
            }
        }

        // 命令需要
        bool validCommand =
            currentCommand.Value == "collect_wood" ||
            currentCommand.Value == "collect_metal" ||
            currentCommand.Value == "collect_iron" ||
            currentCommand.Value == "collect_ammo" ||
            currentCommand.Value == "collect_food" ||
            currentCommand.Value == "collect_all";

        if (!validCommand)
        {
            nearestResource.Value = null;
            Debug.LogWarning($"[BTFindNearestResource] 无效命令: {currentCommand.Value}");
            return TaskStatus.Failure;
        }

        // 先按类型和消耗性过滤，然后按距离排序
        var aiPos = transform.position;
        var sortedResources = pickups
            .Where(p => p.resourceData != null && match(p.resourceData.type))
            .Where(p => !p.resourceData.isConsuming) // 排除消耗性物品
            .OrderBy(p => Vector3.Distance(p.transform.position, aiPos))
            .ToList();

        if (sortedResources.Count == 0)
        {
            nearestResource.Value = null;
            //Debug.Log("[BTFindNearestResource] 没有找到符合条件的资源");
            return TaskStatus.Failure;
        }

        // 智能检查：只检查前几个最近的资源
        PickupItem selectedResource = null;
        int checkCount = 0;
        int maxCheckAfterFull = agentSettings.checkAvailableRecourseTime; // 最大检查资源是否装得下的次数

        bool foundFullBag = false;
        int additionalChecks = 0;

        foreach (var pickup in sortedResources)
        {
            checkCount++;

            if (CanAIPickupResource(pickup))
            {
                selectedResource = pickup;
                break; // 找到第一个能装下的就选它
            }
            else
            {
                // 装不下了
                if (!foundFullBag)
                {
                    foundFullBag = true;
                    //Debug.Log($"[BTFindNearestResource] 发现背包接近满载，继续检查后续{maxCheckAfterFull}个资源");
                }

                additionalChecks++;
                if (additionalChecks >= maxCheckAfterFull)
                {
                    //Debug.Log($"[BTFindNearestResource] 已检查{checkCount}个资源，背包空间不足");
                    break;
                }
            }
        }

        if (selectedResource != null)
        {
            nearestResource.Value = selectedResource.transform;
            needStorage.Value = false;
            //Debug.Log($"[BTFindNearestResource] 选择资源: {selectedResource.resourceData.resourceName} (检查了{checkCount}个)");
            return TaskStatus.Success;
        }
        else
        {
            nearestResource.Value = null;
            needStorage.Value = true;
            //Debug.Log($"[BTFindNearestResource] 未找到可收集资源 (共检查{checkCount}个)");
            return TaskStatus.Failure;
        }
    }

    /// <summary>
    /// 检查AI是否能装下特定的资源物品
    /// </summary>
    private bool CanAIPickupResource(PickupItem pickup)
    {
        if (inventoryManager == null || pickup == null || pickup.resourceData == null)
            return false;

        float currentWeight = inventoryManager.GetAICurrentWeight();
        float maxWeight = inventoryManager.aiPlayerMaxWeight;

        // 计算拾取这个资源后的重量
        float resourceWeight = pickup.resourceData.unitWeight * pickup.amount;
        float newWeight = currentWeight + resourceWeight;

        // 检查是否超重
        return newWeight <= maxWeight;
    }
}