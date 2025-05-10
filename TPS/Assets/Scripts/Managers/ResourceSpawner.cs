using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourcePrefabInfo
    {
        public ResourceData_SO resourceData;
        public GameObject resourcePrefab;
        public int maxAmountPerPile = 10; // 每堆包含资源个数的最大值
    }
    public Inventory inventoryReference; // 持有Inventory引用 手动为生成的资源Prefab注入Inventory

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
        Debug.Log("开始生成资源");
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
                        // 随机资源种类
                        var res = resourceTypes[Random.Range(0, resourceTypes.Count)];
                        var obj = Instantiate(res.resourcePrefab, navHit.position, Quaternion.identity);
                        var pickup = obj.GetComponent<PickupItem>();
                        if (pickup != null)
                        {
                            pickup.resourceData = res.resourceData;
                            pickup.amount = Random.Range(1, res.maxAmountPerPile + 1);
                            pickup.inventory = inventoryReference;
                        }

                        placedPositions.Add(navHit.position);
                    }
                }
            }
        }

        Debug.Log($"生成了 {placedPositions.Count} 堆资源");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
