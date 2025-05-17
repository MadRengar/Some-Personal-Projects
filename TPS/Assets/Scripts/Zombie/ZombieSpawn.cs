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
            SpawnDayZombies();

        //if (enableNightSpawn)
        //    StartCoroutine(SpawnNightZombies());
    }

    private void SpawnDayZombies()
    {
        List<Vector3> placedPositions = new List<Vector3>();
        int tries = 0;
        int maxTries = 500;

        while (placedPositions.Count < dayGroupCount && tries < maxTries)
        {
            tries++;
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * daySpawnRadius;
            randomPoint.y = 100f;

            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshCheckRadius, NavMesh.AllAreas))
                {
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
                            Vector3 offset = Random.insideUnitSphere * 3f;
                            offset.y = 0f;
                            SpawnZombie(navHit.position + offset);
                        }

                        placedPositions.Add(navHit.position);
                    }
                }
            }
        }

        Debug.Log($"白天生成了 {aliveZombies.Count} 个僵尸");
    }

    private void SpawnZombie(Vector3 position)
    {
        if (currentZombiesAlive >= maxZombiesAlive) return;

        GameObject zombie = ZombiePool.Instance.TrySpawnZombie(position, Quaternion.identity);
        aliveZombies.Add(zombie);
        currentZombiesAlive++;

        // 监听死亡时回收
        ZombieStats stats = zombie.GetComponent<ZombieStats>();
        if (stats != null)
        {
            // 你可以在 ZombieStats 的 Die() 中触发事件或调接口回收
        }
    }
}
