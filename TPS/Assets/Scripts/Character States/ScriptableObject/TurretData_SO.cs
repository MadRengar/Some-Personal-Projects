using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

[CreateAssetMenu(fileName = "New Data", menuName = "Turret/Turret Data")]
public class TurretData_SO : ScriptableObject
{
    [Header("Turret Building Info")]
    public int requiredWoodNum;
    public int requiredIronNum;
    public float requiredBuidlingTime;

    [Header("Turret Attack Info")]
    public int attackDamage;
    public float firerate;
    public float attackRange;

    [Header("Fire Pattern")]
    public float continuousFireDuration = 3f;  // 连续开火持续时间
    public float restDuration = 2f;            // 休息时间
}
