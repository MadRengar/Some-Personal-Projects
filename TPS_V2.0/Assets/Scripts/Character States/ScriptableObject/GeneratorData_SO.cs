using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorData", menuName = "Building/Generator Data")]
public class GeneratorData_SO : BuildingData_SO
{
    [Header("Power Generation")]
    public int powerOutput = 10;        // 提供的电力

    // 构造函数中设置建筑类型
    private void OnEnable()
    {
        buildingType = BuildingType.Generator;
    }
}