using PlayerControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("State Building Ref")]
    public FoodSupplyController foodSupplyZone;

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
    private Coroutine hpRecoverCoroutine;

    private PlayerInputSystem playerInputSystem;
    private Animator animator;
    private ThirdPersonController controller;
    private CameraController cameraController;
    private TreatmentController treatmentArea;

    private void Awake()
    {
        playerInputSystem = GetComponent<PlayerInputSystem>();
        animator = GetComponent<Animator>();
        controller = GetComponent<ThirdPersonController>();
        cameraController = GetComponent<CameraController>();
        treatmentArea = CampZoneManager.Instance.GetTreatmentArea().GetComponent<TreatmentController>();
        InitializePlayerState();
    }

    private void Start()
    {
        // 开始饱食度衰减
        StartSatietyDecay();
        StartHpRecover();
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

    #region OnPlayerStateChange
    public void TakeDamage(int damageValue)
    {
        if (!isAlive || GameManager.Instance.IsGameOver())
            return; // 已死亡或游戏结束时不再受伤

        currentHealth -= damageValue;
        //Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);

        // 检查是否死亡
        if (currentHealth <= 0f && isAlive)
        {
            PlayerDie();
        }
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
            if(treatmentArea.IsPlayerInTreatmentArea())
            {
                yield return new WaitForSeconds(1f);
                if (currentHealth <= playerData.maxHealth)
                {
                    currentHealth += treatmentArea.GetPlayerRecoverRate();

                    if (currentHealth > playerData.maxHealth)
                    {
                        currentHealth = playerData.maxHealth;
                    }
                    OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
                }
            }
            else
            {
                yield return null;
            }
            
        }
    }
    #endregion

    #region Decay IEnumerator
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
            yield return null;

            if (currentSatiety > 0)
            {
                if(currentSatiety > playerData.maxSatiety)
                {
                    currentSatiety = playerData.maxSatiety;
                    OnSatietyChanged?.Invoke(currentSatiety, playerData.maxSatiety);
                    continue;
                }
                if (foodSupplyZone.isPlayerInRange())
                {
                    currentSatiety += foodSupplyZone.GetSupplySatietyPerSec() * Time.deltaTime;
                }
                else
                {
                    currentSatiety -= satietyDecayRate * Time.deltaTime; // 乘以帧时间
                }
                
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
                if(CampZoneManager.Instance.IsPlayerInCampZone())
                {
                    currentStamina -= 0f * Time.deltaTime;
                }
                else
                {
                    currentStamina -= staminaDecayRate * Time.deltaTime; // 乘以帧时间
                }
                
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    if (playerInputSystem != null)
                    {
                        //Debug.Log("体力耗尽，停止冲刺");
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
    #endregion

    // 玩家死亡处理（Key：触发入口）
    private void PlayerDie()
    {
        isAlive = false;
        currentHealth = 0;

        Debug.Log("玩家死亡！");

        // 触发全局死亡事件
        GameManager.TriggerPlayerDeath();
    }

    // 重置玩家状态（用于重新开始）
    public void ResetPlayer()
    {
        InitializePlayerState();
        controller.ResetPlayerController();
        cameraController.ResetDeathCamera();
    }

    // getter方法
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => playerData.maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => playerData.maxStamina;
    public float GetCurrentSatiety() => currentSatiety;
    public float GetMaxSatiety() => playerData.maxSatiety;
}
