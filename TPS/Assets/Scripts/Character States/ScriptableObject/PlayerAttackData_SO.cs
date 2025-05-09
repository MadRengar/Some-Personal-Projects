using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Attack/Player AttackData")]
public class PlayerAttackData_SO : ScriptableObject
{
    [Header("Attack Info")]
    public int minAttackDamage;
    public int maxAttackDamage;
    // 暴击
    public float criticalMultiplier; // 加成百分比
    public float criticalChance; // 加成暴击率
}
