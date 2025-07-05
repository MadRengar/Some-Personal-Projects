using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using PlayerControl;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourcePrefabInfo
    {
        public ResourceData_SO resourceData;
        public GameObject resourcePrefab;
        public int maxAmountPerPile = 10; // 每堆包含资源个数的最大值
        [Header("Spawn Weight")]
        [Range(1, 100)]
        public int spawnWeight = 10; // 生成权重，数值越大生成概率越高
    }
    public ResourcePoolManager poolManager;
    public InventoryManager inventoryManagerRef; // 持有Inventory引用 手动为生成的资源Prefab注入Inventory
    public PlayerInputSystem playerInputSystemRef;
    public List<ResourcePrefabInfo> resourceTypes;

    /*一次刷新会随机生成的“资源堆”的数量区间*/
    public int minPileCount = 30;
    public int maxPileCount = 60;

    /*每两个资源堆之间的最小距离。*/
    public float minDistanceBetweenPiles = 5f;

    /*在生成过程中最多尝试几次去找合法位置。*/
    public int maxTries = 1000;

    public LayerMask groundMask;

    public float spawnRadius = 100f;

    void Start()
    {
        SpawnResources();
    }

    void SpawnResources()
    {
        int pileCount = Random.Range(minPileCount, maxPileCount + 1);
        List<Vector3> placedPositions = new List<Vector3>();

        int tries = 0;
        while (placedPositions.Count < pileCount && tries < maxTries)
        {
            tries++;

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = 100f; // 射线起点

            // 投射到地面
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {
                // 检查是否在 NavMesh 上
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
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
                        // 随机资源种类
                        if (res != null)
                        {
                            // 使用对象池取对象
                            var obj = poolManager.Get(res.resourceData.type);
                            obj.transform.position = navHit.position;
                            obj.transform.rotation = Quaternion.identity;
                            var pickup = obj.GetComponent<PickupItem>();
                            if (pickup != null)
                            {
                                // 为生成的资源属性赋值
                                pickup.resourceData = res.resourceData;
                                pickup.amount = Random.Range(1, res.maxAmountPerPile + 1);
                                pickup.poolType = res.resourceData.type;
                                pickup.isConsuming = res.resourceData.isConsuming;

                                // 获得Manager 的引用
                                pickup.inventoryManager = inventoryManagerRef;
                                pickup.playerInputSystem = playerInputSystemRef;
                                pickup.poolManager = poolManager;
                                pickup.playerStats = GameManager.Instance.GetPlayerStats();
                            }
                            placedPositions.Add(navHit.position);
                        }
                    }
                }
            }
        }

        //Debug.Log($"生成了 {placedPositions.Count} 堆资源");
        // 打印各种资源的统计信息
        //LogResourceStatistics(placedPositions.Count);
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

    /// <summary>
    /// 打印资源生成统计（可选，用于调试）
    /// </summary>
    void LogResourceStatistics(int totalGenerated)
    {
        Debug.Log("=== 资源生成统计 ===");
        foreach (var res in resourceTypes)
        {
            float expectedRatio = (float)res.spawnWeight / GetTotalWeight();
            int expectedCount = Mathf.RoundToInt(totalGenerated * expectedRatio);
            Debug.Log($"{res.resourceData.resourceName}: 权重 {res.spawnWeight}, 预期数量 ~{expectedCount}");
        }
    }

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
    }
}
