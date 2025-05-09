using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStats : MonoBehaviour
{
    public ZombieData_SO zombieData;
    public ZombieAttackData_SO zombieAttackData;

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

    public int CurrentHealth
    {
        get
        {
            if (zombieData != null)
            {
                return zombieData.currentHealth;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            zombieData.currentHealth = value;
        }
    }

    public bool IsAlive
    {
        get
        {
            if(zombieData != null)
            {
                return zombieData.isAlive;
            }
            else
            {
                return false;
            }
        }
        set
        {
            zombieData.isAlive = value;
        }
    }

    public bool IsBerserk
    {
        get
        {
            if(zombieData != null)
            {
                return zombieData.isBerserk;
            }
            else
            {
                return false;
            }
        }
        set
        {
            zombieData.isBerserk = value;
        }
    }
    #endregion
}
