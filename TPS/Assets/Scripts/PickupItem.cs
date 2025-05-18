using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public ResourceData_SO resourceData;
    public int amount = 1;
    public PlayerInputSystem playerInputSystem;

    public ResourcePoolManager poolManager;
    public ResourceType poolType;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInputSystem.pickUp)
        {
            if (inventoryManager != null)
            {
                bool success = inventoryManager.TryAddPlayer(resourceData, amount);
                if (success)
                {
                    //Destroy(gameObject);
                    ReturnToPool();
                    Debug.Log($"拾取成功：{resourceData.resourceName} x{amount} 当前重量：{inventoryManager.GetPlayerCurrentWeight()}");
                }
                else
                {
                    Debug.Log("背包超重，无法拾取");
                }
                playerInputSystem.pickUp = false;
            }
        }
    }

    /// <summary>
    /// 供AI调用：AI拾取该资源
    /// </summary>
    /// <returns>拾取是否成功</returns>
    public bool TryPickupByAI()
    {
        if (inventoryManager != null && resourceData != null)
        {
            bool success = inventoryManager.TryAddAI(resourceData, amount);
            if (success)
            {
                //Destroy(gameObject);
                ReturnToPool();
                Debug.Log($"AI拾取成功：{resourceData.resourceName} x{amount} 当前AI背包重量：{inventoryManager.GetAICurrentWeight()}");
                return true;
            }
            else
            {
                Debug.Log("AI背包超重，无法拾取");
                return false;
            }
        }
        Debug.LogWarning("AI拾取失败：未绑定InventoryManager或资源数据为空");
        return false;
    }

    // 玩家/AI拾取成功时调用：
    public void ReturnToPool()
    {
        if (poolManager != null)
        {
            poolManager.Return(poolType, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
