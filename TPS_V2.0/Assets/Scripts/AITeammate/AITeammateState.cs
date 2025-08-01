using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITeammateState : MonoBehaviour
{
    public PlayerData_SO aiPlayerData;
    public InventoryManager inventoryManager;

    [Header("Running State")]
    [SerializeField] private int currentHealth; // 当前生命值
    [SerializeField] private float aiPlayerCurrentWeight; // 当前物资重量
    [SerializeField] private bool isAlive;

    public static event Action<int, int> AIOnHealthChanged;

    private Coroutine hpRecoverCoroutine;
    private TreatmentController treatmentArea;

    void Start()
    {
        treatmentArea = CampZoneManager.Instance.GetTreatmentArea().GetComponent<TreatmentController>();
        StartHpRecover();
        InitializeAIState();
        InitializeAIInventory();
    }



    public void InitializeAIState()
    {
        if (aiPlayerData != null)
        {
            currentHealth = aiPlayerData.maxHealth;
            isAlive = aiPlayerData.isAlive;
        }

        AIOnHealthChanged?.Invoke(currentHealth, aiPlayerData.maxHealth);
    }

    public void InitializeAIInventory()
    {
        if (inventoryManager != null)
        {
            aiPlayerCurrentWeight = inventoryManager.GetAICurrentWeight();
        }
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
        AIOnHealthChanged?.Invoke(currentHealth, aiPlayerData.maxHealth);
        if (currentHealth <= 0f && isAlive)
        {
            AIPlayerDie();
        }
    }

    private void AIPlayerDie()
    {
        isAlive = false;
        currentHealth = 0;

        // 清空背包
        inventoryManager.ClearAIInventory();

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
        if (currentHealth < aiPlayerData.maxHealth)
        {
            currentHealth += healingValue;
            //Debug.Log($"玩家获得{healingValue}点治疗，当前生命值：{currentHealth}");
            if (currentHealth > aiPlayerData.maxHealth)
            {
                currentHealth = aiPlayerData.maxHealth;
            }
            AIOnHealthChanged?.Invoke(currentHealth, aiPlayerData.maxHealth);
        }
    }

    public void StartHpRecover()
    {
        if (hpRecoverCoroutine != null)
        {
            StopCoroutine(hpRecoverCoroutine);
        }

        hpRecoverCoroutine = StartCoroutine(HpRecoverCoroutine());
    }

    public void StopHpRecover()
    {
        if (hpRecoverCoroutine != null)
        {
            StopCoroutine(hpRecoverCoroutine);
            hpRecoverCoroutine = null;
        }
    }

     private IEnumerator HpRecoverCoroutine()
    {
        while(true)
        {
            if(treatmentArea.IsAIPlayerInTreatmentArea())
            {
                yield return new WaitForSeconds(1f);
                if (currentHealth <= aiPlayerData.maxHealth)
                {
                    currentHealth += treatmentArea.GetAIPlayerRecoverRate();

                    if (currentHealth > aiPlayerData.maxHealth)
                    {
                        currentHealth = aiPlayerData.maxHealth;
                    }
                    AIOnHealthChanged?.Invoke(currentHealth, aiPlayerData.maxHealth);
                }
            }
            else
            {
                yield return null;
            }
            
        }
    }

    #region Getter
    public bool IsAlive()
    {
        return isAlive;
    }

    public int GetAICurrentHealth()
    {
        return currentHealth;
    }
    #endregion
}
