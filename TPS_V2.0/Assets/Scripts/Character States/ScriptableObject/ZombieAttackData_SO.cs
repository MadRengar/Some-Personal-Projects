using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Attack/Zombie AttackData")]
public class ZombieAttackData_SO : ScriptableObject
{
    [Header("Attack Info")]
    public int attackDamage;
    public float attackCD;
    public float attackRange;
}
