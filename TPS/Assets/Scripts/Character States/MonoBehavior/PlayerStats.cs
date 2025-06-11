using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerDate_SO playerData;
    public PlayerAttackData_SO playerAttackData;

    [Header("Running State")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentStamina;
    [SerializeField] private int currentSatiety;
    [SerializeField] private int currentInfectivity;
    [SerializeField] private bool isAlive;

    public static event Action<int, int> OnHealthChanged; // 当前血量, 最大血量
    private void Awake()
    {
        InitializePlayerState();
    }

    private void InitializePlayerState()
    {
        currentHealth = playerData.maxHealth;
        currentStamina = playerData.maxStamina;
        currentSatiety = playerData.maxSatiety;
        currentInfectivity = playerData.maxInfectivity;
        isAlive = playerData.isAlive;

        //初始化事件
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }


    public void TakeDamage(int damageValue)
    {
        currentHealth -= damageValue;
        Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    public void GetHealing(int healingValue)
    {
        currentHealth += healingValue;
        Debug.Log($"玩家获得{healingValue}点治疗，当前生命值：{currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    // getter方法
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => playerData.maxHealth;
}
