using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITeammateState : MonoBehaviour
{
    public PlayerData_SO playerData;
    public InventoryManager inventoryManager;

    [Header("Resource Data")]
    public ResourceData_SO woodResourceData;
    public ResourceData_SO ironResourceData;

    [Header("Running State")]
    public int currentHealth; // 当前生命值
    public float aiPlayerCurrentWeight; // 当前物资重量

    public static event Action<int, int> AIOnHealthChanged;

    void Start()
    {
        InitializeAIState();
    }

    private void InitializeAIState()
    {
        if (playerData != null)
        {
            currentHealth = playerData.maxHealth;
        }
        if (inventoryManager != null)
        {
            aiPlayerCurrentWeight = inventoryManager.GetAICurrentWeight();
        }

        // 初始化事件
        AIOnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    public (int wood, int iron) GetAiCurrentResourcesNum() // 元组
    {
        if (inventoryManager == null) return (0, 0);

        int woodAmount = inventoryManager.GetAIMount(woodResourceData);
        int ironAmount = inventoryManager.GetAIMount(ironResourceData);

        return (woodAmount, ironAmount);
    }


    public void AITakeDamage(int damageValue)
    {
        currentHealth -= damageValue;
        //Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{currentHealth}");
        AIOnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    public void AIGetHealing(int healingValue)
    {
        if (currentHealth < playerData.maxHealth)
        {
            currentHealth += healingValue;
            //Debug.Log($"玩家获得{healingValue}点治疗，当前生命值：{currentHealth}");
            if (currentHealth > playerData.maxHealth)
            {
                currentHealth = playerData.maxHealth;
            }
            AIOnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        }
    }
}
