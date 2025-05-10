using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ResourceSlot
{
    public ResourceData_SO data;
    public int quantity;
}
public class Inventory : MonoBehaviour
{

    public List<ResourceSlot> resourceSlots = new List<ResourceSlot>();
    public float maxWeight = 100f;
 
    public float CurrentWeight
    {
        get
        {
            float total = 0f;
            foreach(var slot in resourceSlots)
            {
                total += slot.data.unitWeight * slot.quantity;
            }
            return total;
        }
    }

    public int GetAmount(ResourceData_SO data)
    {
        var slot = resourceSlots.Find(s => s.data == data);
        return slot != null ? slot.quantity : 0;
    }

    public bool TryAdd(ResourceData_SO data, int amount)
    {
        float addedWeight = data.unitWeight * amount;
        if (CurrentWeight + addedWeight > maxWeight)
            return false;

        var slot = resourceSlots.Find(s => s.data == data);
        if(slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            resourceSlots.Add(new ResourceSlot { data = data, quantity = amount });
        }
        return true;
    }
}

