using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerDate_SO playerData;
    public PlayerAttackData_SO playerAttackData;
    /*Prompts 直接对CharacterDate_SO中的数值进行修改*/
    #region Read from Data_SO
    public int MaxHealth 
    {  
        get
        {
            if (playerData != null)
            {
                return playerData.maxHealth;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            playerData.maxHealth = value;
        }
    }

    public int CurrentHealth
    {
        get
        {
            return playerData.currentHealth;
        }
        set
        {
            playerData.currentHealth = value;
        }
    }

    public int Stamina
    {
        get
        {
            return playerData.stamina;
        }
        set
        {
            playerData.stamina = value;
        }
    }

    public bool IsAlive
    {
        get
        {
            return playerData.isAlive;
        }
        set 
        { 
            playerData.isAlive = value; 
        }
    }
    #endregion

    //TODO: FIX CurrentHealth 衰减问题
    public void TakeDamage(int damageValue)
    {
        CurrentHealth -= damageValue;
        Debug.Log($"玩家受到{damageValue}点伤害，当前生命值：{CurrentHealth}");
    }
}
