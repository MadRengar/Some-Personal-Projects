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

    // 获取玩家某资源数量
    public int GetPlayerAmount(ResourceData_SO data)
    {
        var slot = playerResourceSlots.Find(s => s.data == data);
        return slot != null ? slot.quantity : 0;
    }

    // 获取AI某资源数量
    public int GetAIMount(ResourceData_SO data)
    {
        var slot = aiPlayerResourceSlots.Find(s => s.data == data);
        return slot != null ? slot.quantity : 0;
    }

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
        return true;
    }

    // 可选：尝试给指定目标加资源（便于后期扩展更多AI或NPC）
    public enum InventoryTarget { Player, AI }
    public bool TryAdd(ResourceData_SO data, int amount, InventoryTarget target)
    {
        switch (target)
        {
            case InventoryTarget.Player:
                return TryAddPlayer(data, amount);
            case InventoryTarget.AI:
                return TryAddAI(data, amount);
        }
        return false;
    }
}

