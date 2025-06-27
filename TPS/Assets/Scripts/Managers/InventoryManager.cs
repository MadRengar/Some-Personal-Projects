using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ResourceSlot
{
    public ResourceData_SO data;
    public int quantity;
}
public class InventoryManager : MonoBehaviour
{
    [Header("Player Inventory")]
    public List<ResourceSlot> playerResourceSlots = new List<ResourceSlot>();
    public float playerMaxWeight = 100f;

    [Header("AI Player Inventory")]
    public List<ResourceSlot> aiPlayerResourceSlots = new List<ResourceSlot>();
    public float aiPlayerMaxWeight = 100f;

    [Header("Running Data(Read Only)")]
    [SerializeField] private float playerCurrentWeight = 0f;
    [SerializeField] private float aiCurrentWeight = 0f;

    [Header("Storage Management")]
    public List<StorageController> allStorages = new List<StorageController>();

    [Header("Storage Debug Info (Read Only)")]
    [SerializeField] private List<StorageDebugInfo> storageDebugList = new List<StorageDebugInfo>();

    [System.Serializable]
    public class StorageDebugInfo
    {
        public string storageName;
        public int woodAmount;
        public int ironAmount;
        public float usedWeight;
        public float remainingCapacity;
        public float totalCapacity;
    }

    public static event Action OnResourcesChanged;
    private ResourcesUIController resourcesUIController;

    private void Start()
    {
        resourcesUIController = GetComponent<ResourcesUIController>();
    }

    #region Getter
    // 获取玩家当前背包重量
    public float GetPlayerCurrentWeight()
    {
        float total = 0f;
        foreach (var slot in playerResourceSlots)
        {
            if (slot.data != null)
                total += slot.data.unitWeight * slot.quantity;
        }
        return total;
    }

    // 获取AI当前背包重量
    public float GetAICurrentWeight()
    {
        float total = 0f;
        foreach (var slot in aiPlayerResourceSlots)
        {
            if (slot.data != null)
                total += slot.data.unitWeight * slot.quantity;
        }
        return total;
    }

    // 根据资源类型获取玩家资源数量
    public int GetPlayerResourceByType(ResourceType type)
    {
        int total = 0;
        foreach (var slot in playerResourceSlots)
        {
            if (slot.data != null && slot.data.type == type)
            {
                total += slot.quantity;
            }
        }
        return total;
    }

    // 根据资源类型获取AI资源数量
    public int GetAIResourceByType(ResourceType type)
    {
        int total = 0;
        foreach (var slot in aiPlayerResourceSlots)
        {
            if (slot.data != null && slot.data.type == type)
            {
                total += slot.quantity;
            }
        }
        return total;
    }

    // 获取玩家+AI的总资源数量（按类型）
    public int GetTotalResourceByType(ResourceType type)
    {
        return GetPlayerResourceByType(type) + GetAIResourceByType(type);
    }
    #endregion

    #region TryAdd Logic
    /// <summary>
    /// 尝试给玩家加资源
    /// </summary>
    public bool TryAddPlayer(ResourceData_SO data, int amount)
    {
        float addedWeight = data.unitWeight * amount;
        if (GetPlayerCurrentWeight() + addedWeight > playerMaxWeight)
            return false;

        var slot = playerResourceSlots.Find(s => s.data == data);
        if(slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            playerResourceSlots.Add(new ResourceSlot { data = data, quantity = amount });
        }

        playerCurrentWeight = GetPlayerCurrentWeight();

        OnResourcesChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 尝试给AI加资源
    /// </summary>
    public bool TryAddAI(ResourceData_SO data, int amount)
    {
        if (data == null) return false;
        float addedWeight = data.unitWeight * amount;
        if (GetAICurrentWeight() + addedWeight > aiPlayerMaxWeight)
            return false;

        var slot = aiPlayerResourceSlots.Find(s => s.data == data);
        if (slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            aiPlayerResourceSlots.Add(new ResourceSlot { data = data, quantity = amount });
        }
        
        aiCurrentWeight = GetAICurrentWeight();

        OnResourcesChanged?.Invoke();
        return true;
    }
    #endregion

    #region TryConsuming Logic
    public bool TryConsuming(int consumingWoodCount, int consumingIronCount)
    {
        // 检查资源是否足够
        int totalWood = GetTotalResourceByType(ResourceType.Wood);
        int totalIron = GetTotalResourceByType(ResourceType.Iron);

        if (totalWood < consumingWoodCount || totalIron < consumingIronCount)
        {
            Debug.LogWarning($"资源不足！木材需要{consumingWoodCount}，拥有{totalWood}；铁需要{consumingIronCount}，拥有{totalIron}");
            return false;
        }

        // 消耗木材
        ConsumeResourceByType(ResourceType.Wood, consumingWoodCount);
        // 消耗铁
        ConsumeResourceByType(ResourceType.Iron, consumingIronCount);

        // 更新显示数据
        playerCurrentWeight = GetPlayerCurrentWeight();
        aiCurrentWeight = GetAICurrentWeight();
        OnResourcesChanged?.Invoke();
        return true;
    }

    private void ConsumeResourceByType(ResourceType resourceType, int needAmount)
    {
        int remaining = needAmount;

        // 从玩家背包消耗
        for (int i = playerResourceSlots.Count - 1; i >= 0; i--)
        {
            var slot = playerResourceSlots[i];
            if (slot.data != null && slot.data.type == resourceType && remaining > 0)
            {
                int consumeAmount = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= consumeAmount;
                remaining -= consumeAmount;

                if (slot.quantity <= 0)
                {
                    playerResourceSlots.RemoveAt(i);
                }
            }
        }

        // 从AI背包消耗
        for (int i = aiPlayerResourceSlots.Count - 1; i >= 0; i--)
        {
            var slot = aiPlayerResourceSlots[i];
            if (slot.data != null && slot.data.type == resourceType && remaining > 0)
            {
                int consumeAmount = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= consumeAmount;
                remaining -= consumeAmount;

                if (slot.quantity <= 0)
                {
                    aiPlayerResourceSlots.RemoveAt(i);
                }
            }
        }
    }
    #endregion

    #region Storage Resource Methods

    /// <summary>
    /// 获取所有仓库中指定类型资源的总数量
    /// </summary>
    public int GetAllStorageResourceByType(ResourceType type)
    {
        int total = 0;
        StorageController[] allStorages = FindObjectsOfType<StorageController>();

        foreach (var storage in allStorages)
        {
            switch (type)
            {
                case ResourceType.Wood:
                    total += storage.GetStoredWood();
                    break;
                case ResourceType.Iron:
                    total += storage.GetStoredIron();
                    break;
            }
        }

        return total;
    }

    /// <summary>
    /// 获取所有仓库中的木头总数量
    /// </summary>
    public int GetAllStorageWood()
    {
        return GetAllStorageResourceByType(ResourceType.Wood);
    }

    /// <summary>
    /// 获取所有仓库中的铁块总数量
    /// </summary>
    public int GetAllStorageIron()
    {
        return GetAllStorageResourceByType(ResourceType.Iron);
    }

    /// <summary>
    /// 获取玩家背包+AI背包+所有仓库的资源总数量
    /// </summary>
    public int GetTotalResourceIncludingAllStorage(ResourceType type)
    {
        int inventoryTotal = GetTotalResourceByType(type); // 玩家+AI背包
        int storageTotal = GetAllStorageResourceByType(type); // 所有仓库
        return inventoryTotal + storageTotal;
    }


    /// <summary>
    /// 注册仓库到管理器中
    /// </summary>
    public void RegisterStorage(StorageController storage)
    {
        if (storage != null && !allStorages.Contains(storage))
        {
            allStorages.Add(storage);
            Debug.Log($"注册仓库: {storage.name}");
            UpdateStorageDebugInfo();
        }
    }

    /// <summary>
    /// 从管理器中移除仓库
    /// </summary>
    public void UnregisterStorage(StorageController storage)
    {
        if (storage != null && allStorages.Contains(storage))
        {
            allStorages.Remove(storage);
            Debug.Log($"移除仓库: {storage.name}");
            UpdateStorageDebugInfo();
        }
    }

    /// <summary>
    /// 更新仓库调试信息（在Inspector中显示）
    /// </summary>
    public void UpdateStorageDebugInfo()
    {
        storageDebugList.Clear();

        for (int i = 0; i < allStorages.Count; i++)
        {
            var storage = allStorages[i];
            if (storage != null)
            {
                var debugInfo = new StorageDebugInfo
                {
                    storageName = storage.name,
                    woodAmount = storage.GetStoredWood(),
                    ironAmount = storage.GetStoredIron(),
                    usedWeight = storage.GetCurrentUsedWeight(),
                    remainingCapacity = storage.GetRemainingCapacity(),
                    totalCapacity = storage.GetStorageCapacity()
                };
                storageDebugList.Add(debugInfo);
            }
        }
    }
    #endregion
    public enum InventoryTarget { Player, AI }

    public void TriggerResourcesChangedEvent()
    {
        OnResourcesChanged?.Invoke();
    }
}

