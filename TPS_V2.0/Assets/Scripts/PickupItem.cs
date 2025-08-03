using PlayerControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [HideInInspector] public InventoryManager inventoryManager;
    [HideInInspector] public PlayerInputSystem playerInputSystem;
    [HideInInspector] public ResourcePoolManager poolManager;
    [HideInInspector] public ResourceSpawner resourceSpawner;
    public PlayerStats playerStats;
    public ResourceData_SO resourceData;
    public int amount;
    public ResourceType poolType;
    public bool isConsuming;

  
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInputSystem.pickUpPressed)
        {
            if(isConsuming)
            {               
                if(poolType == ResourceType.MedicalKit)
                {
                    if(playerStats != null)
                    {
                        playerStats.GetHealing(resourceData.restoreValues);
                        ReturnToPool();
                    }             
                    
                }
                else if(poolType == ResourceType.Food)
                {
                    if (playerStats != null)
                    {
                        playerStats.GetFood(resourceData.restoreValues);
                        ReturnToPool();
                    }                                 
                }
                return;
            }

            if (inventoryManager != null)
            {
                bool success = inventoryManager.TryAddPlayer(resourceData, amount);
                if (success)
                {
                    //Destroy(gameObject);
                    ReturnToPool();
                    //Debug.Log($"拾取成功：{resourceData.resourceName} x{amount} 当前重量：{inventoryManager.GetPlayerCurrentWeight()}");
                }
                else
                {
                    Debug.Log("背包超重，无法拾取");
                }
                playerInputSystem.pickUpPressed = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInputSystem != null)
        {
            playerInputSystem.pickUpPressed = false;
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
        // 从资源管理列表中移除
        if (resourceSpawner != null)
        {
            resourceSpawner.UnregisterActiveResource(gameObject);
        }

        // 归还到对象池
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
