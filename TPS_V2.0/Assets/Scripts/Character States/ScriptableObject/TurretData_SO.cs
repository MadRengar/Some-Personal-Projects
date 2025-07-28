using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

[CreateAssetMenu(fileName = "TurretData", menuName = "Turret/Turret Data")]
public class TurretData_SO : BuildingData_SO
{
    [Header("Turret Basic Info")]
    public int turretLevel;

    [Header("Turret Attack Info")]
    public int attackDamage;
    public float firerate;
    public float attackRange;
    public int power;

    [Header("Fire Pattern")]
    public float continuousFireDuration = 3f;  // 连续开火持续时间
    public float restDuration = 2f;            // 休息时间

    // 构造函数中设置建筑类型
    private void OnEnable()
    {
        buildingType = BuildingType.Turret;
    }
}
