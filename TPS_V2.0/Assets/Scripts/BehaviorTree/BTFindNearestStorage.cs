using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

public class BTFindNearestStorage : Conditional
{
    public SharedTransform nearestStorage;

    private InventoryManager inventoryManager;

    public override void OnStart()
    {
        inventoryManager = GameManager.Instance?.GetInventoryManager();
    }

    public override TaskStatus OnUpdate()
    {
        if (inventoryManager == null)
        {
            //Debug.LogError("[BTFindNearestStorage] 无法获取InventoryManager");
            return TaskStatus.Failure;
        }

        // 获取AI当前的资源数量
        int aiWoodAmount = inventoryManager.GetAIResourceByType(ResourceType.Wood);
        int aiIronAmount = inventoryManager.GetAIResourceByType(ResourceType.Iron);

        if (aiWoodAmount == 0 && aiIronAmount == 0)
        {
            //Debug.Log("[BTFindNearestStorage] AI背包为空，无需存储");
            nearestStorage.Value = null;
            return TaskStatus.Failure;
        }

        // 获取所有仓库
        var allStorages = inventoryManager.allStorages;
        var viableStorages = new List<StorageController>();

        // 筛选出能够存储至少部分资源的仓库
        foreach (var storage in allStorages)
        {
            if (storage != null && CanStoreAnyResources(storage, aiWoodAmount, aiIronAmount))
            {
                viableStorages.Add(storage);
            }
        }

        if (viableStorages.Count == 0)
        {
            //Debug.Log("[BTFindNearestStorage] 没有找到可用的仓库");
            nearestStorage.Value = null;
            return TaskStatus.Failure;
        }

        // 选择最近的仓库
        var aiPos = transform.position;
        var nearest = viableStorages
            .OrderBy(s => Vector3.Distance(s.transform.position, aiPos))
            .First();

        nearestStorage.Value = nearest.transform;
        //Debug.Log($"[BTFindNearestStorage] 选择仓库: {nearest.name}");

        return TaskStatus.Success;
    }

    private bool CanStoreAnyResources(StorageController storage, int woodAmount, int ironAmount)
    {
        // 检查仓库是否能存储至少1个木材或1个铁
        return storage.CanStoreResources(Mathf.Min(1, woodAmount), 0) ||
               storage.CanStoreResources(0, Mathf.Min(1, ironAmount));
    }
}