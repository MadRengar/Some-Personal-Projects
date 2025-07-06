using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITeammateState : MonoBehaviour
{
    public PlayerData_SO playerData;
    public InventoryManager inventoryManager;

    [Header("Running State")]
    public int currentHealth; // 当前生命值
    public float aiPlayerCurrentWeight; // 当前物资重量
    [SerializeField] private bool isAlive;

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
            isAlive = playerData.isAlive;
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

        int woodAmount = inventoryManager.GetAIResourceByType(ResourceType.Wood);
        int ironAmount = inventoryManager.GetAIResourceByType(ResourceType.Iron);

        return (woodAmount, ironAmount);
    }


    public void AITakeDamage(int damageValue)
    {
        if (!isAlive || GameManager.Instance.IsGameOver())
            return; // 已死亡或游戏结束时不再受伤
        currentHealth -= damageValue;
        //Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{currentHealth}");
        AIOnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        if (currentHealth <= 0f && isAlive)
        {
            AIPlayerDie();
        }
    }

    private void AIPlayerDie()
    {
        isAlive = false;
        currentHealth = 0;

        // 触发动画控制器的死亡状态
        var animController = GetComponent<AIAnimationController>();
        animController.SetDead(true);


        // 停止AI的移动和行为
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        GameManager.TriggerAIPlayerDeath();
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

    #region Getter
    public bool IsAlive()
    {
        return isAlive;
    }
    #endregion
}
