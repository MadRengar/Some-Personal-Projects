using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStats : MonoBehaviour
{
    public ZombieData_SO zombieData; // 只读配置引用
    public ZombieAttackData_SO zombieAttackData;
    private GameTimeManager gameTimeManager;
    public enum ZombieOrigin
    {
        DaySpawn,    // 白天生成的温和僵尸
        NightSpawn   // 夜晚生成的狂暴僵尸
    }


    [Header("Zombie Origin Tracking")]
    [SerializeField] private ZombieOrigin spawnOrigin = ZombieOrigin.DaySpawn;


    [Header("Running Stats")]
    public int currentHealth;
    public bool isAlive;
    public bool isBerserk;

    private Animator animator;
    private ZombieFSM fsm;
    private int dayCount;
    private int maxLimitHealth = 200;
    private int maxLimitAttack = 50;
    #region Read from Data_SO
    public int MaxHealth
    {
        get
        {
            if (zombieData != null)
            {
                return zombieData.maxHealth;
            }
            else
            {
                return 0;
            }
        }
    }

    public bool IsAlive
    {
        get
        {
            if (zombieData != null)
            {
                return zombieData.isAlive;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion

    private void Awake()
    {
        animator = GetComponent<Animator>();
        //spawnManager = GetComponent<ZombieManager>();
        //原因：prefab 在运行时被对象池实例化时，其 Inspector 是失效的 所以ZombieSpawn为空

        fsm = GetComponent<ZombieFSM>();
        gameTimeManager = GameManager.Instance.GetGameTimeManager();
    }
    /* OnEnable() 是 Unity 提供的一个回调函数
     * 触发时机：
     * 1.脚本首次激活（enabled）
     * 2.GameObject 从 SetActive(false) 被重新设为 SetActive(true) 时
     */
    private void OnEnable()
    {
        ResetZombie();
    }

    /// <summary>
    /// 初始化/重置僵尸状态（用于对象池复用）
    /// </summary>
    public void ResetZombie()
    {
        //Debug.Log("已被重置！");
        currentHealth = MaxHealth + 5 * (dayCount - 1);
        if (currentHealth >= maxLimitHealth)
        {
            currentHealth = maxLimitHealth;
        }

        isAlive = IsAlive;
        isBerserk = false;// 与时间相关

        if (animator != null)
        {
            animator.SetBool("isAlive", true);
        }
        if (fsm != null)
        {
            fsm.ResetZombieFSM();
        }
        else
        {
            Debug.Log("状态机为空！");
        }
        
    }

    public void TakeDamage(int damageValue)
    {
        if (!isAlive) return;
        if (zombieData != null)
        {
            currentHealth -= damageValue;
            if (currentHealth <= 0)
            {
                Die();           
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        currentHealth = 0;
        //Debug.Log("死亡！");

        /* 切换状态机 */
        if (fsm != null)
        {
            fsm.EnterDeadState(isAlive);
        }

        /* 对象池回收 */
        ZombieManager spawnManager = FindObjectOfType<ZombieManager>();
        if (spawnManager != null)
        {
            spawnManager.OnZombieDied(gameObject);
        }
    }

    #region 设置来源
    /// <summary>
    /// 设置僵尸来源
    /// </summary>
    public void SetSpawnOrigin(ZombieOrigin origin)
    {
        spawnOrigin = origin;
    }

    /// <summary>
    /// 获取僵尸来源
    /// </summary>
    public ZombieOrigin GetSpawnOrigin()
    {
        return spawnOrigin;
    }

    /// <summary>
    /// 检查是否为白天生成的僵尸
    /// </summary>
    public bool IsDaySpawnedZombie()
    {
        return spawnOrigin == ZombieOrigin.DaySpawn;
    }

    /// <summary>
    /// 检查是否为夜晚生成的僵尸
    /// </summary>
    public bool IsNightSpawnedZombie()
    {
        return spawnOrigin == ZombieOrigin.NightSpawn;
    }
    #endregion
}
