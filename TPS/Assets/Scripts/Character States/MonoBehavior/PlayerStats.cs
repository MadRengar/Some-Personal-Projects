using PlayerControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerData_SO playerData;

    [Header("Running State")]
    [SerializeField] private int currentHealth; // 当前生命值
    [SerializeField] private float currentStamina; // 当前体力值
    [SerializeField] private float currentSatiety; // 当前饱食度
    [SerializeField] private float currentInfectivity; // 当前感染率
    [SerializeField] private bool isAlive;

    public static event Action<int, int> OnHealthChanged; // 当前血量, 最大血量
    public static event Action<float, float> OnSatietyChanged; // 饱食度
    public static event Action<float, float> OnStaminaChanged; // 体力值

    [Header("Decay Settings")]
    [SerializeField] private float satietyDecayRate = 1f; // 每秒衰减量
    [SerializeField] private float staminaDecayRate = 1f; // 每秒衰减量

    private Coroutine satietyDecayCoroutine;
    private Coroutine staminaDecayCoroutine;
    private Coroutine staminaRecoverCoroutine;

    private PlayerInputSystem playerInputSystem;

    private void Awake()
    {
        playerInputSystem = GetComponent<PlayerInputSystem>();
        InitializePlayerState();
    }

    private void Start()
    {
        // 开始饱食度衰减
        StartSatietyDecay();
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
        OnSatietyChanged?.Invoke(currentSatiety, playerData.maxSatiety);
    }


    public void TakeDamage(int damageValue)
    {
        currentHealth -= damageValue;
        //Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    public void GetHealing(int healingValue)
    {
        if (currentHealth < playerData.maxHealth)
        {
            currentHealth += healingValue;
            //Debug.Log($"玩家获得{healingValue}点治疗，当前生命值：{currentHealth}");
            if(currentHealth > playerData.maxHealth)
            {
                currentHealth = playerData.maxHealth;
            }
            OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        }
    }

    public void GetFood(int foodValue)
    {
        if(currentSatiety < playerData.maxSatiety)
        {
            currentSatiety += foodValue;
            Debug.Log($"玩家获得{foodValue}点食物，当前饱食度：{currentSatiety}");
            if (currentSatiety > playerData.maxSatiety)
            {
                currentSatiety = playerData.maxSatiety;
            }
            OnSatietyChanged?.Invoke(currentSatiety, playerData.maxSatiety);
        }
    }

    /*饱食度衰减协程*/
    public void StartSatietyDecay()
    {
        if (satietyDecayCoroutine != null)
        {
            StopCoroutine(satietyDecayCoroutine);
        }
        satietyDecayCoroutine = StartCoroutine(SatietyDecayCoroutine());
    }

    public void StopSatietyDecay()
    {
        if (satietyDecayCoroutine != null)
        {
            StopCoroutine(satietyDecayCoroutine);
            satietyDecayCoroutine = null;
        }
    }

    private IEnumerator SatietyDecayCoroutine()
    {
        while (true)
        {
            yield return null; // 每帧执行，而不是每秒

            if (currentSatiety > 0)
            {
                currentSatiety -= satietyDecayRate * Time.deltaTime; // 乘以帧时间
                if (currentSatiety < 0)
                {
                    currentSatiety = 0;
                }

                OnSatietyChanged?.Invoke(currentSatiety, playerData.maxSatiety);
            }
        }
    }

    /*体力值变化协程*/
    public void StartStaminaDecay()
    {
        if (staminaDecayCoroutine != null)
        {
            StopCoroutine(staminaDecayCoroutine);
        }
        staminaDecayCoroutine = StartCoroutine(StaminaDecayCoroutine());
    }

    public void StopStaminaDecay()
    {
        if (staminaDecayCoroutine != null)
        {
            StopCoroutine(staminaDecayCoroutine);
            staminaDecayCoroutine = null;
        }
    }

    private IEnumerator StaminaDecayCoroutine()
    {
        while (true)
        {
            yield return null; // 每帧执行

            if (currentStamina > 0)
            {
                currentStamina -= staminaDecayRate * Time.deltaTime; // 乘以帧时间
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    if (playerInputSystem != null)
                    {
                        Debug.Log("体力耗尽，停止冲刺");
                        playerInputSystem.sprint = false;
                    }
                }

                OnStaminaChanged?.Invoke(currentStamina, playerData.maxStamina);
            }
        }
    }

    public void StartStaminaRecover()
    {
        if (staminaRecoverCoroutine != null)
        {
            StopCoroutine(staminaRecoverCoroutine);
        }
        staminaRecoverCoroutine = StartCoroutine(StaminaRecoverCoroutine());
    }

    public void StopStaminaRecover()
    {
        if (staminaRecoverCoroutine != null)
        {
            StopCoroutine(staminaRecoverCoroutine);
            staminaRecoverCoroutine = null;
        }
    }

    private IEnumerator StaminaRecoverCoroutine()
    {
        while (true)
        {
            yield return null; // 每帧执行

            if (currentStamina < playerData.maxStamina)
            {
                currentStamina += staminaDecayRate * Time.deltaTime; // 乘以帧时间
                if (currentStamina > playerData.maxStamina)
                {
                    currentStamina = playerData.maxStamina;
                }
                OnStaminaChanged?.Invoke(currentStamina, playerData.maxStamina);
            }
        }
    }
    // getter方法
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => playerData.maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => playerData.maxStamina;
    public float GetCurrentSatiety() => currentSatiety;
    public float GetMaxSatiety() => playerData.maxSatiety;
}
