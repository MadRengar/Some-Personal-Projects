using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStats : MonoBehaviour
{
    public ZombieData_SO zombieData; // 只读配置引用
    public ZombieAttackData_SO zombieAttackData;

    [Header("运行时状态")]
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isAlive;
    [SerializeField] private bool isBerserk;

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
        set
        {
            zombieData.maxHealth = value;
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

    private void OnEnable()
    {
        ResetZombie(); // 对象池激活时，重置状态
    }

    /// <summary>
    /// 初始化/重置僵尸状态（用于对象池复用）
    /// </summary>
    public void ResetZombie()
    {
        currentHealth = MaxHealth;
        isAlive = true;
        isBerserk = false;// 与时间相关
        // TODO：重置动画状态、AI状态、特效等
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
        Debug.Log("死亡！");
        // TODO: 回收、播放动画等
    }
}
