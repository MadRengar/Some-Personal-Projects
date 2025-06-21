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
    [SerializeField] private int woodCount = 0;
    [SerializeField] private int ironCount = 0;
    [SerializeField] private float playerCurrentWeight = 0f;
    [SerializeField] private float aiCurrentWeight = 0f;

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

        woodCount = GetTotalResourceByType(ResourceType.Wood);
        ironCount = GetTotalResourceByType(ResourceType.Iron);
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
        
        woodCount = GetTotalResourceByType(ResourceType.Wood);
        ironCount = GetTotalResourceByType(ResourceType.Iron);
        aiCurrentWeight = GetAICurrentWeight();

        OnResourcesChanged?.Invoke();
        return true;
    }
    #endregion

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

        OnResourcesChanged?.Invoke();
        // 更新显示数据
        woodCount = GetTotalResourceByType(ResourceType.Wood);
        ironCount = GetTotalResourceByType(ResourceType.Iron);
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


    public enum InventoryTarget { Player, AI }
}

