using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestResource : Conditional
{
    // 行为树黑板变量：输出最近资源的Transform
    public SharedTransform nearestResource;
    public SharedString currentCommand; // 当前指令

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
            // 根据Prompt文件里行为与ResourceType的映射来写
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

        // 命令校验
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


        // 按命令过滤
        var filtered = pickups
            .Where(p => p.resourceData != null && match(p.resourceData.type))
            .ToList();

        if (filtered.Count == 0)
        {
            nearestResource.Value = null;
            return TaskStatus.Failure;
        }

        // 选择最近的
        var aiPos = transform.position;
        var nearest = filtered
            .OrderBy(p => Vector3.Distance(p.transform.position, aiPos))
            .First();
        
        nearestResource.Value = nearest.transform;
        Debug.Log($"[BTFindNearestResource] {currentCommand.Value}，目标资源类型: {nearest.GetComponent<PickupItem>().resourceData.type}");
        
        return TaskStatus.Success;
    }
}
