using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Player Data")]
public class PlayerDate_SO : ScriptableObject
{
    [Header("State Info")]
    public int maxHealth;
    public int maxStamina; // 体力
    public int maxSatiety; // 饱食度
    public int maxInfectivity; // 感染性
    public bool isAlive;
}
