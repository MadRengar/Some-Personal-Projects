using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourcePrefabInfo
    {
        public ResourceData_SO resourceData;
        public GameObject resourcePrefab;
        public int maxAmountPerPile = 10; // 每个小包含资源个数的最大值
        [Header("Spawn Weight")]
        [Range(1, 100)]
        public int spawnWeight = 10; // 生成权重，数值越大生成概率越高
    }
    public ResourcePoolManager poolManager;
    public InventoryManager inventoryManagerRef; // 传入Inventory引用 让动态生成的资源Prefab注册Inventory
    public PlayerInputSystem playerInputSystemRef;
    public List<ResourcePrefabInfo> resourceTypes;

    [Header("Daily Refresh Settings")]
    public bool enableDailyRefresh = true; // 是否启用每日刷新
    [Range(0.5f, 2.0f)]
    public float dailyRefreshMultiplier = 1.0f; // 每日刷新倍率（可以调整资源数量）

    /*一次性最多会随机生成的"资源堆"的数量区间*/
    public int minPileCount = 30;
    public int maxPileCount = 60;

    /*每两个资源堆之间的最小距离。*/
    public float minDistanceBetweenPiles = 5f;

    /*在生成过程中最多尝试几次寻找合法位置。*/
    public int maxTries = 1000;

    public LayerMask groundMask;
    public float spawnRadius = 100f;

    [Header("Camp Zone Detection")]
    public Transform campZoneTransform; // 手动拖拽营地的Transform
    public Vector3 campZoneSize = new Vector3(100f, 10f, 80f); // 营地大小，如果没有自动获取就手动设置

    [Header("Debug Settings")]
    public bool enableDebugLog = false;

    private List<GameObject> activeResources = new List<GameObject>(); // 管理激活的资源对象

    void Start()
    {
        // 尝试自动获取营地信息
        if (campZoneTransform == null)
        {
            GameObject campZoneObj = GameObject.FindWithTag("CampZone");
            if (campZoneObj != null)
            {
                campZoneTransform = campZoneObj.transform;
                Debug.Log($"自动找到营地: {campZoneObj.name}");
            }
        }

        // 尝试从CampZone脚本获取大小
        if (campZoneTransform != null)
        {
            CampZone campZone = campZoneTransform.GetComponent<CampZone>();
            if (campZone != null)
            {
                campZoneSize = campZone.campSize;
                Debug.Log($"从CampZone获取营地大小: {campZoneSize}");
            }
        }

        SpawnResources();
    }

    /// <summary>
    /// 手动检查位置是否在营地内 - 使用坐标和大小计算
    /// </summary>
    private bool IsPositionInCampZone(Vector3 position)
    {
        if (campZoneTransform == null)
        {
            if (enableDebugLog)
                Debug.LogWarning("没有设置营地Transform，跳过营地检测");
            return false;
        }

        Vector3 campCenter = campZoneTransform.position;
        Vector3 halfSize = campZoneSize * 0.5f;

        // 检查X轴范围
        bool inXRange = position.x >= (campCenter.x - halfSize.x) &&
                        position.x <= (campCenter.x + halfSize.x);

        // 检查Z轴范围  
        bool inZRange = position.z >= (campCenter.z - halfSize.z) &&
                        position.z <= (campCenter.z + halfSize.z);

        // Y轴范围（可选，通常地面资源不需要严格检查Y轴）
        bool inYRange = position.y >= (campCenter.y - halfSize.y) &&
                        position.y <= (campCenter.y + halfSize.y);

        bool isInCamp = inXRange && inZRange; // 不检查Y轴，只检查XZ平面

        if (isInCamp && enableDebugLog)
        {
            Debug.Log($"位置 {position} 在营地内");
            Debug.Log($"营地中心: {campCenter}, 营地大小: {campZoneSize}");
        }

        return isInCamp;
    }

    void SpawnResources()
    {
        int pileCount = Random.Range(minPileCount, maxPileCount + 1);
        List<Vector3> placedPositions = new List<Vector3>();

        int tries = 0;
        int campZoneBlocked = 0; // 统计被营地阻挡的次数

        while (placedPositions.Count < pileCount && tries < maxTries)
        {
            tries++;

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = 100f; // 射线起点

            // 投射到地面层
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {
                // 检查是否在 NavMesh 上
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    // 检查是否在CampZone内
                    if (IsPositionInCampZone(navHit.position))
                    {
                        campZoneBlocked++;

                        Debug.Log($"位置在营地内，跳过: {navHit.position}");
                        continue; // 如果在营地区域内，跳过这个位置
                    }

                    bool tooClose = false;
                    foreach (var pos in placedPositions)
                    {
                        if (Vector3.Distance(pos, navHit.position) < minDistanceBetweenPiles)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        // 根据权重随机选择资源类型
                        var res = GetWeightedRandomResource();
                        // 随机资源数量
                        if (res != null)
                        {
                            // 使用对象池获取对象
                            var obj = poolManager.Get(res.resourceData.type);
                            obj.transform.position = navHit.position;
                            obj.transform.rotation = Quaternion.identity;
                            var pickup = obj.GetComponent<PickupItem>();
                            if (pickup != null)
                            {
                                // 为生成的资源配置数值
                                pickup.resourceData = res.resourceData;
                                pickup.amount = Random.Range(1, res.maxAmountPerPile + 1);
                                pickup.poolType = res.resourceData.type;
                                pickup.isConsuming = res.resourceData.isConsuming;

                                // 获得Manager 的引用
                                pickup.inventoryManager = inventoryManagerRef;
                                pickup.playerInputSystem = playerInputSystemRef;
                                pickup.poolManager = poolManager;
                                pickup.playerStats = GameManager.Instance.GetPlayerStats();
                                pickup.resourceSpawner = this; // 设置引用
                                RegisterActiveResource(obj);   // 注册到管理列表
                            }
                            placedPositions.Add(navHit.position);
                            if (enableDebugLog)
                                Debug.Log($"成功生成资源在: {navHit.position}, 类型: {res.resourceData.resourceName}");
                        }
                    }
                }
            }
        }

        // 输出最终统计信息
        if (enableDebugLog)
        {
            Debug.Log($"=== 资源生成完成 ===");
            Debug.Log($"成功生成: {placedPositions.Count}/{pileCount} 个资源堆");
            Debug.Log($"总尝试次数: {tries}");
            Debug.Log($"被营地阻挡次数: {campZoneBlocked}");
        }
    }

    /// <summary>
    /// 根据权重随机选择资源
    /// </summary>
    ResourcePrefabInfo GetWeightedRandomResource()
    {
        if (resourceTypes.Count == 0) return null;

        // 计算总权重
        int totalWeight = 0;
        foreach (var res in resourceTypes)
        {
            totalWeight += res.spawnWeight;
        }

        // 生成随机数
        int randomValue = Random.Range(0, totalWeight);

        // 根据权重选择
        int currentWeight = 0;
        foreach (var res in resourceTypes)
        {
            currentWeight += res.spawnWeight;
            if (randomValue < currentWeight)
            {
                return res;
            }
        }

        // 备用返回第一个
        return resourceTypes[0];
    }

    #region Refresh Everyday
    public void ClearAllResources()
    {
        int clearedCount = 0;

        // 使用缓存的列表，避免 FindObjectsOfType 的性能消耗
        for (int i = activeResources.Count - 1; i >= 0; i--)
        {
            GameObject resource = activeResources[i];

            if (resource != null && resource.activeInHierarchy)
            {
                PickupItem pickup = resource.GetComponent<PickupItem>();
                if (pickup != null && poolManager != null)
                {
                    // 归还到对象池
                    poolManager.Return(pickup.poolType, resource);
                    clearedCount++;
                }
            }

            // 从列表中移除（倒序遍历，所以直接移除）
            activeResources.RemoveAt(i);
        }

        // 清空列表，确保同步
        activeResources.Clear();

        if (enableDebugLog)
            Debug.Log($"=== 每日资源刷新 ===\n清空了 {clearedCount} 个资源 (优化版本)");
    }

    /// <summary>
    /// 添加资源到管理列表
    /// </summary>
    public void RegisterActiveResource(GameObject resource)
    {
        if (resource != null && !activeResources.Contains(resource))
        {
            activeResources.Add(resource);

            if (enableDebugLog)
                Debug.Log($"注册资源到管理列表: {resource.name}, 当前总数: {activeResources.Count}");
        }
    }

    /// <summary>
    /// 从管理列表移除资源
    /// </summary>
    public void UnregisterActiveResource(GameObject resource)
    {
        if (resource != null && activeResources.Contains(resource))
        {
            activeResources.Remove(resource);

            if (enableDebugLog)
                Debug.Log($"从管理列表移除资源: {resource.name}, 剩余总数: {activeResources.Count}");
        }
    }

    /// <summary>
    /// 获取当前激活资源数量（用于调试）
    /// </summary>
    public int GetActiveResourceCount()
    {
        return activeResources.Count;
    }

    /// <summary>
    /// 每日资源刷新（清空+重新生成）
    /// </summary>
    public void DailyResourceRefresh()
    {
        if (!enableDailyRefresh)
        {
            Debug.Log("每日资源刷新已禁用");
            return;
        }

        // 先清空现有资源
        ClearAllResources();

        // 等待一帧确保清空完成，然后重新生成
        StartCoroutine(DelayedSpawn());
    }

    /// <summary>
    /// 延迟生成协程，确保清空操作完成
    /// </summary>
    private IEnumerator DelayedSpawn()
    {
        yield return null; // 等待一帧

        if (enableDebugLog)
            Debug.Log($"开始重新生成资源 - 目标数量: {minPileCount}-{maxPileCount}");

        // 重新生成资源
        SpawnResources();

        if (enableDebugLog)
            Debug.Log("每日资源刷新完成！");
    }
    #endregion

    int GetTotalWeight()
    {
        int total = 0;
        foreach (var res in resourceTypes)
        {
            total += res.spawnWeight;
        }
        return total;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // 绘制营地范围
        if (campZoneTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(campZoneTransform.position, campZoneSize);
        }
    }
}