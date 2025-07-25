using UnityEngine;

public class AIHammerController : MonoBehaviour
{
    [Header("Hammer Settings")]
    [SerializeField] private float hammerRange = 3f;
    [SerializeField] private int repairPerHit = 10; // 每次挥击恢复的耐久度

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private PingMarkerManager pingManager;
    private GameObject targetBuilding;

    void Start()
    {
        pingManager = GameManager.Instance.GetPingMarkerManager();
    }

    /// <summary>
    /// 执行一次锤子挥击（由Animation Event调用）
    /// </summary>
    public void OnHammerHit()
    {
        if (enableDebugLog)
        {
            Debug.Log("[AIHammerController] 执行锤子挥击");
        }

        targetBuilding = pingManager.GetCurrentMarkedBuilding();

        // 执行维修
        PerformRepair();
    }

    /// <summary>
    /// 执行维修逻辑
    /// </summary>
    private void PerformRepair()
    {
        if (targetBuilding == null) return;

        if (enableDebugLog)
        {
            Debug.Log($"[AIHammerController] 修理建筑物: {targetBuilding.name}，恢复 {repairPerHit} 点耐久");
        }

        // 检查建筑是否有有效的控制器
        IBuildingController buildingController = targetBuilding.GetComponent<IBuildingController>();
        if (buildingController != null)
        {
            // 检查建筑是否被摧毁
            if (buildingController.IsDestroyed())
            {
                if (enableDebugLog)
                {
                    Debug.Log("[AIHammerController] 建筑已被摧毁，无法修理");
                }
                return;
            }

            // 根据建筑类型进行修理（使用负伤害来修理）
            TurretController turret = targetBuilding.GetComponent<TurretController>();
            if (turret != null)
            {
                turret.TakeDamage(-repairPerHit);
                if (enableDebugLog)
                {
                    Debug.Log($"[AIHammerController] 成功修理防御塔");
                }
                return;
            }

            GeneratorController generator = targetBuilding.GetComponent<GeneratorController>();
            if (generator != null)
            {
                generator.TakeDamage(-repairPerHit);
                if (enableDebugLog)
                {
                    Debug.Log($"[AIHammerController] 发电机修理成功");
                }
                return;
            }

            StorageController storage = targetBuilding.GetComponent<StorageController>();
            if (storage != null)
            {
                storage.TakeDamage(-repairPerHit);
                if (enableDebugLog)
                {
                    Debug.Log($"[AIHammerController] 仓库修理成功");
                }
                return;
            }
        }

        if (enableDebugLog)
        {
            Debug.LogWarning($"[AIHammerController] 无法修理建筑物 {targetBuilding.name}：未找到有效的建筑控制器");
        }
    }

    /// <summary>
    /// 检查目标建筑是否已满血
    /// </summary>
    public bool IsTargetBuildingFullHealth()
    {
        if (targetBuilding == null)
        {
            // 获取当前标记的建筑物
            if (pingManager != null)
            {
                targetBuilding = pingManager.GetCurrentMarkedBuilding();
            }
        }

        if (targetBuilding == null) return true;

        // 检查不同类型建筑的血量
        TurretController turret = targetBuilding.GetComponent<TurretController>();
        if (turret != null)
        {
            return turret.IsFullHealth();
        }

        GeneratorController generator = targetBuilding.GetComponent<GeneratorController>();
        if (generator != null)
        {
            return generator.IsFullHealth();
        }

        StorageController storage = targetBuilding.GetComponent<StorageController>();
        if (storage != null)
        {
            return storage.IsFullHealth();
        }

        return true; // 默认认为已满血
    }
}