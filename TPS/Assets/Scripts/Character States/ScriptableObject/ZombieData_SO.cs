using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Zombie Data")]
public class ZombieData_SO : ScriptableObject
{
    [Header("State Info")]
    public int maxHealth;
    public int currentHealth;
    public bool isAlive;
    public bool isBerserk; // °×Ìì&Ò¹ÍíµÄÇÐ»»£¿
}
