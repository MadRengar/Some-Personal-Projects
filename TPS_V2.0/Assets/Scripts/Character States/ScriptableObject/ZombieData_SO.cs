using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZombieType { type1, type2, type3, type4 }

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Zombie Data")]
public class ZombieData_SO : ScriptableObject
{
    [Header("State Info")]
    public int maxHealth;
    public bool isAlive;
    public bool isGuard;
    public ZombieType type;
}
