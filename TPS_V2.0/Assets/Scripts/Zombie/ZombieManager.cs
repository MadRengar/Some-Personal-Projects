using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieManager : MonoBehaviour
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
    public List<ZombieSpawner> nightSpawners = new List<ZombieSpawner>(); // 夜晚生成器列表

    [Header("Total ZombieCount Limit")]
    public int maxZombiesAlive = 60;
    public int currentZombiesAlive = 0;

    private List<GameObject> aliveZombies = new List<GameObject>();

    private void Start()
    {
        if (enableDaySpawn)
        {
            SpawnDayZombies();
        }
        // 订阅游戏时间事件
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNightStarted += OnNightStarted;
            GameTimeManager.Instance.OnDawnStarted += OnDawnStarted;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNightStarted -= OnNightStarted;
            GameTimeManager.Instance.OnDawnStarted -= OnDawnStarted;
        }
    }

    /// <summary>
    /// 夜晚开始事件处理
    /// </summary>
    private void OnNightStarted()
    {
        //Debug.Log("夜晚降临，启动所有僵尸生成器");

        // 启动所有生成器
        foreach (var spawner in nightSpawners)
        {
            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }
    }

    /// <summary>
    /// 黎明开始事件处理
    /// </summary>
    private void OnDawnStarted()
    {
        //Debug.Log("黎明到来，停止所有僵尸生成器");

        // 停止所有生成器
        foreach (var spawner in nightSpawners)
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
            }
        }
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

    private void SpawnZombie(Vector3 position, bool isBerserk = false)
    {
        // 游戏结束检查
        if (GameManager.Instance.IsGameOver())
        {
            return;
        }

        if (currentZombiesAlive >= maxZombiesAlive) return;

        // 从对象池中取一个僵尸，放置到目标位置
        GameObject zombie = ZombiePool.Instance.TrySpawnZombie(position, Quaternion.identity);

        if (zombie != null)
        {
            aliveZombies.Add(zombie); // 加入活着的僵尸列表
            currentZombiesAlive++; // 活着的数量加一

            // 如果是夜晚僵尸，设置为狂暴状态
            if (isBerserk)
            {
                SetZombieBerserkState(zombie, true);
            }
        }
    }

    /// <summary>
    /// 由ZombieSpawner调用，注册生成的僵尸
    /// </summary>
    public void RegisterSpawnedZombie(GameObject zombie)
    {
        if (zombie != null && !aliveZombies.Contains(zombie))
        {
            aliveZombies.Add(zombie);
            currentZombiesAlive++;
        }
    }

    /// <summary>
    /// 检查是否可以生成更多僵尸
    /// </summary>
    public bool CanSpawnMoreZombies()
    {
        return currentZombiesAlive < maxZombiesAlive;
    }

    /// <summary>
    /// 设置僵尸狂暴状态
    /// </summary>
    private void SetZombieBerserkState(GameObject zombie, bool isBerserk)
    {
        // 获取僵尸的数据组件
        var zombieStats = zombie.GetComponent<ZombieStats>();
        if (zombieStats != null && zombieStats.zombieData != null)
        {
            zombieStats.isBerserk = isBerserk;
        }

        // 重置僵尸FSM以应用新状态
        var zombieFSM = zombie.GetComponent<ZombieFSM>();
        if (zombieFSM != null)
        {
            zombieFSM.ResetZombieFSM();
        }
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
        if(!GameTimeManager.Instance.IsNight())
        {
            SpawnDayZombies();
        }
    }

    private IEnumerator DelayedDespawn(GameObject zombie, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (zombie.activeInHierarchy)
        {
            ZombiePool.Instance.DespawnZombie(zombie);
        }
    }

    /// <summary>
    /// 获取当前活跃的僵尸总数
    /// </summary>
    public int GetTotalActiveZombies()
    {
        return currentZombiesAlive;
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制范围边界线
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, daySpawnRadius);

        // 绘制群体间最小距离示意
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 半透明红色
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenGroups);
    }
}
