using UnityEngine;

[CreateAssetMenu(fileName = "StorageData", menuName = "Building/Storage Data")]
public class StorageData_SO : BuildingData_SO
{
    [Header("Storage Info")]
    public int storageCapacity = 100;   // 存储容量

    // 构造函数中设置建筑类型
    private void OnEnable()
    {
        buildingType = BuildingType.Storage;
    }
}