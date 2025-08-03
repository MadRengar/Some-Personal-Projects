using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTStoreResources : Action
{
    public SharedTransform nearestStorage;
    public SharedString currentCommand;
    public SharedBool needStorage;

    private InventoryManager inventoryManager;

    public override void OnStart()
    {
        inventoryManager = GameManager.Instance?.GetInventoryManager();
    }

    public override TaskStatus OnUpdate()
    {
        if (inventoryManager == null || nearestStorage.Value == null)
        {
            Debug.LogError("[BTStoreResources] 缺少必要组件");
            return TaskStatus.Failure;
        }

        var storageController = nearestStorage.Value.GetComponent<StorageController>();
        if (storageController == null)
        {
            Debug.LogError("[BTStoreResources] 目标不是有效仓库");
            return TaskStatus.Failure;
        }

        // 获取AI当前资源
        int aiWoodAmount = inventoryManager.GetAIResourceByType(ResourceType.Wood);
        int aiIronAmount = inventoryManager.GetAIResourceByType(ResourceType.Iron);

        if (aiWoodAmount == 0 && aiIronAmount == 0)
        {
            // 背包空了，清空命令，完成任务
            currentCommand.Value = "";
            needStorage = false;
            Debug.Log("[BTStoreResources] AI背包为空，任务完成");
            return TaskStatus.Success;
        }

        // 计算仓库能存储多少资源
        var (maxWood, maxIron) = CalculateMaxStorable(storageController, aiWoodAmount, aiIronAmount);

        if (maxWood == 0 && maxIron == 0)
        {
            Debug.Log("[BTStoreResources] 当前仓库已满，寻找下一个仓库");
            return TaskStatus.Failure;
        }

        // 存储资源到仓库
        if (storageController.TryStoreResources(maxWood, maxIron))
        {
            // 从AI背包中移除已存储的资源
            RemoveResourcesFromAI(maxWood, maxIron);

            Debug.Log($"[BTStoreResources] 成功存储: 木材{maxWood}, 铁{maxIron}");

            // 检查背包是否清空，如果是则完成任务
            int remainingWood = inventoryManager.GetAIResourceByType(ResourceType.Wood);
            int remainingIron = inventoryManager.GetAIResourceByType(ResourceType.Iron);

            if (remainingWood == 0 && remainingIron == 0)
            {
                currentCommand.Value = "";
                Debug.Log("[BTStoreResources] 所有资源已存储，任务完成");
            }

            return TaskStatus.Success;
        }
        else
        {
            Debug.LogWarning("[BTStoreResources] 存储失败");
            return TaskStatus.Failure;
        }
    }

    private (int wood, int iron) CalculateMaxStorable(StorageController storage, int aiWood, int aiIron)
    {
        float remainingCapacity = storage.GetRemainingCapacity();

        // 优先存储铁（重量更大）
        float ironWeight = 2f;
        float woodWeight = 1f;

        int maxIron = Mathf.Min(aiIron, Mathf.FloorToInt(remainingCapacity / ironWeight));
        remainingCapacity -= maxIron * ironWeight;

        int maxWood = Mathf.Min(aiWood, Mathf.FloorToInt(remainingCapacity / woodWeight));

        return (maxWood, maxIron);
    }

    private void RemoveResourcesFromAI(int woodAmount, int ironAmount)
    {
        for (int i = inventoryManager.aiPlayerResourceSlots.Count - 1; i >= 0; i--)
        {
            var slot = inventoryManager.aiPlayerResourceSlots[i];
            if (slot.data == null) continue;

            if (slot.data.type == ResourceType.Wood && woodAmount > 0)
            {
                int removeAmount = Mathf.Min(slot.quantity, woodAmount);
                slot.quantity -= removeAmount;
                woodAmount -= removeAmount;

                if (slot.quantity <= 0)
                {
                    inventoryManager.aiPlayerResourceSlots.RemoveAt(i);
                }
            }
            else if (slot.data.type == ResourceType.Iron && ironAmount > 0)
            {
                int removeAmount = Mathf.Min(slot.quantity, ironAmount);
                slot.quantity -= removeAmount;
                ironAmount -= removeAmount;

                if (slot.quantity <= 0)
                {
                    inventoryManager.aiPlayerResourceSlots.RemoveAt(i);
                }
            }
        }

        inventoryManager.TriggerResourcesChangedEvent();
    }
}