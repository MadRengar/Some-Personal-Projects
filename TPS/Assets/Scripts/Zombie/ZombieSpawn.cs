using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawn : MonoBehaviour
{
    [Header("Basic Setting")]
    public LayerMask groundMask;
    public float navMeshCheckRadius = 2f;

    [Header("Day Spawn Setting")]
    public bool enableDaySpawn = true;
    public float daySpawnRadius = 80f;
    public int dayGroupCount = 5;
    public int minZombiesPerGroup = 3;
    public int maxZombiesPerGroup = 6;
    public float minDistanceBetweenGroups = 10f;

    [Header("Night Spawn Setting")]
    public bool enableNightSpawn = true;
    public List<Transform> nightSpawnPoints; // 地图四个固定点
    public float spawnIntervalAtNight = 1f; // 每秒刷一个
    public int nightZombiesPerWave = 20;

    [Header("Total ZombieCount Limit")]
    public int maxZombiesAlive = 60;
    private int currentZombiesAlive = 0;

    private List<GameObject> aliveZombies = new List<GameObject>();

    private void Start()
    {
        if (enableDaySpawn)
        {
            SpawnDayZombies();
        }
        //if (enableNightSpawn)
        //    StartCoroutine(SpawnNightZombies());
    }

    private void SpawnDayZombies()
    {
        List<Vector3> placedPositions = new List<Vector3>(); // 存储每个成功放置的僵尸群中心点
        int tries = 0;
        int maxTries = 500;

        while (placedPositions.Count < dayGroupCount && tries < maxTries)
        {
            tries++;
            // 生成一个随机点（以当前 GameObject 为中心）作为候选群体生成点
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * daySpawnRadius;
            randomPoint.y = 100f; // 从高处向下做 Raycast（避免地下或遮挡）

            // 向下射线投射，判断是否命中了地面（groundMask 层）
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {   // 判断命中的地面点是否位于可走的 NavMesh 区域
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshCheckRadius, NavMesh.AllAreas))
                {
                    // 检查与已有群体中心的距离是否过近
                    bool tooClose = false;
                    foreach (var pos in placedPositions)
                    {
                        if (Vector3.Distance(pos, navHit.position) < minDistanceBetweenGroups)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        int zombiesInGroup = Random.Range(minZombiesPerGroup, maxZombiesPerGroup + 1);
                        for (int i = 0; i < zombiesInGroup; i++)
                        {
                            Vector3 offset = Random.insideUnitSphere * 3f; // 每只僵尸做一点随机偏移（避免重叠）
                            offset.y = 0f; // 保持平面偏移
                            SpawnZombie(navHit.position + offset); // 实际生成僵尸
                        }
                        // 记录这个生成点，避免下次太近
                        placedPositions.Add(navHit.position);
                    }
                }
            }
        }
        //Debug.Log($"白天生成了 {aliveZombies.Count} 个僵尸");
    }

    private void SpawnZombie(Vector3 position)
    {
        if (currentZombiesAlive >= maxZombiesAlive) return;
        // 从对象池中取一个僵尸，放置到目标位置
        GameObject zombie = ZombiePool.Instance.TrySpawnZombie(position, Quaternion.identity);
        aliveZombies.Add(zombie); // 加入活着的僵尸列表
        currentZombiesAlive++; // 活着的数量加一
    }

    public void OnZombieDied(GameObject zombie)
    {
        if (aliveZombies.Contains(zombie))
        {
            aliveZombies.Remove(zombie);
            currentZombiesAlive--;

            StartCoroutine(DelayedDespawn(zombie, 2.5f));

            // 当全部僵尸死亡时，重新生成新一波
            if (currentZombiesAlive == 0 && enableDaySpawn)
            {
                Debug.Log("白天僵尸清除完毕，正在刷新新一波...");
                StartCoroutine(RespawnAfterDelay(2f));
            }
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnDayZombies();
    }

    private IEnumerator DelayedDespawn(GameObject zombie, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (zombie.activeInHierarchy)
        {
            ZombiePool.Instance.DespawnZombie(zombie);
        }
    }
}
