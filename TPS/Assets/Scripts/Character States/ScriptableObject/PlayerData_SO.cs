using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Player Data")]
public class PlayerData_SO : ScriptableObject
{
    [Header("State Info")]
    public int maxHealth;
    public float maxStamina; // 体力
    public float maxSatiety; // 饱食度
    public float maxInfectivity; // 感染性
    public bool isAlive;
}
