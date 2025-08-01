using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieManager : MonoBehaviour
{
    [Header("Basic Setting")]
    public LayerMask groundMask;
    public float navMeshCheckRadius = 2f;

    [Header("Dawn Damage Settings")]
    public int damagePerSecond = 20;

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

    [Header("Zombie Origin Management")]
    [SerializeField] private int dayZombiesCount = 0;    // 白天僵尸数量
    [SerializeField] private int nightZombiesCount = 0;  // 夜晚僵尸数量

    [Header("Camp Zone Detection")]
    public Transform campZoneTransform; // 营地区域的Transform
    public Vector3 campZoneSize = new Vector3(100f, 10f, 80f); // 营地大小

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

    #region Day and Night Switch
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

        // 将所有白天僵尸变为狂暴状态
        ConvertDayZombiesToBerserk();
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
        // 开始持续扣血所有狂暴状态的僵尸
        StartBerserkZombieHealthDecay();
    }

    /// <summary>
    /// 将所有白天生成的僵尸转换为狂暴状态
    /// </summary>
    private void ConvertDayZombiesToBerserk()
    {
        int convertedCount = 0;

        // 遍历所有活着的僵尸
        foreach (var zombie in aliveZombies)
        {
            if (zombie != null)
            {
                var zombieStats = zombie.GetComponent<ZombieStats>();
                if (zombieStats != null && zombieStats.IsDaySpawnedZombie())
                {
                    // 检查是否已经是狂暴状态
                    if (!zombieStats.isBerserk)
                    {
                        // 设置为狂暴状态
                        SetZombieBerserkState(zombie, true);
                        convertedCount++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 开始对狂暴僵尸进行持续扣血
    /// </summary>
    private void StartBerserkZombieHealthDecay()
    {
        int affectedCount = 0;

        foreach (var zombie in aliveZombies)
        {
            if (zombie != null)
            {
                var zombieStats = zombie.GetComponent<ZombieStats>();
                if (zombieStats != null && zombieStats.isBerserk)
                {
                    // 开始扣血协程
                    StartCoroutine(HealthDecayCoroutine(zombie));
                    affectedCount++;
                }
            }
        }

        Debug.Log($"[ZombieManager] 黎明到来：开始对 {affectedCount} 个狂暴僵尸进行持续扣血（{damagePerSecond}/秒）");
    }

    /// <summary>
    /// 持续扣血协程
    /// </summary>
    private IEnumerator HealthDecayCoroutine(GameObject zombie)
    {
        var zombieStats = zombie.GetComponent<ZombieStats>();

        while (zombie != null && zombie.activeInHierarchy && zombieStats != null && zombieStats.isBerserk)
        {
            // 扣血
            zombieStats.TakeDamage(damagePerSecond);

            // 检查是否死亡
            if (zombieStats.currentHealth <= 0)
            {
                //Debug.Log($"[ZombieManager] 狂暴僵尸 {zombie.name} 因黎明扣血死亡");
                yield break; // 僵尸死亡，退出协程
            }

            // 等待1秒
            yield return new WaitForSeconds(1f);
        }
    }

    #endregion

    private void SpawnDayZombies()
    {
        // 尝试自动获取营地信息
        if (campZoneTransform == null)
        {
            GameObject campZoneObj = GameObject.FindWithTag("CampZone");
            if (campZoneObj != null)
            {
                campZoneTransform = campZoneObj.transform;
            }
        }

        // 尝试从CampZone组件获取大小
        if (campZoneTransform != null)
        {
            CampZone campZone = campZoneTransform.GetComponent<CampZone>();
            if (campZone != null)
            {
                campZoneSize = campZone.campSize;
            }
        }

        List<Vector3> placedPositions = new List<Vector3>(); // 存储每个成功放置的僵尸群组的中心点
        int tries = 0;
        int maxTries = 500;
        int campZoneBlocked = 0; // 统计被营地阻挡的次数

        while (placedPositions.Count < dayGroupCount && tries < maxTries)
        {
            tries++;
            // 生成一个随机点（以当前 GameObject 为中心）作为候选群组生成点
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * daySpawnRadius;
            randomPoint.y = 100f; // 从高处向下 Raycast（避免地下或障碍）

            // 向下射线投射，判定是否命中了地面（groundMask 层）
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {   // 判定命中的地面点是否位于可走的 NavMesh 区内
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshCheckRadius, NavMesh.AllAreas))
                {
                    // *** 新增：检查是否在营地内 ***
                    if (IsPositionInCampZone(navHit.position))
                    {
                        campZoneBlocked++;
                        continue; // 如果在营地区内，跳过这个位置
                    }

                    // 检查与已有群组中心的距离是否过近
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
                            Vector3 offset = Random.insideUnitSphere * 3f; // 每个僵尸做一点随机偏移（避免重叠）
                            offset.y = 0f; // 保持平面偏移
                            SpawnZombie(navHit.position + offset); // 实际生成僵尸
                        }
                        // 记录这个生成点，避免下次太接近
                        placedPositions.Add(navHit.position);
                    }
                }
            }
        }
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
            var zombieStats = zombie.GetComponent<ZombieStats>();
            zombieStats.SetSpawnOrigin(ZombieStats.ZombieOrigin.DaySpawn);

            dayZombiesCount++; // 白天僵尸数量+1
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
    /// 由ZombieSpawner调用，注册晚上生成的僵尸
    /// </summary>
    public void RegisterSpawnedZombie(GameObject zombie)
    {
        if (zombie != null && !aliveZombies.Contains(zombie))
        {
            var zombieStats = zombie.GetComponent<ZombieStats>();
            zombieStats.SetSpawnOrigin(ZombieStats.ZombieOrigin.NightSpawn);

            nightZombiesCount++; // 夜晚僵尸数量+1
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
            var zombieStats = zombie.GetComponent<ZombieStats>();
            if (zombieStats != null)
            {
                if (zombieStats.IsDaySpawnedZombie())
                {
                    dayZombiesCount--;
                }
                else if (zombieStats.IsNightSpawnedZombie())
                {
                    nightZombiesCount--;
                }
            }

            aliveZombies.Remove(zombie);
            currentZombiesAlive--;

            StartCoroutine(DelayedDespawn(zombie, 1.5f));

            // 当全部僵尸死亡时，重新生成新一波
            if (currentZombiesAlive == 0 && enableDaySpawn)
            {
                StartCoroutine(RespawnAfterDelay(3f));
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
    /// 自动检查位置是否在营地内 - 使用坐标和大小计算
    /// </summary>
    private bool IsPositionInCampZone(Vector3 position)
    {
        if (campZoneTransform == null)
        {
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

        // Y轴范围（可选，通常地面资源不需要严格检测Y轴）
        bool inYRange = position.y >= (campCenter.y - halfSize.y) &&
                        position.y <= (campCenter.y + halfSize.y);

        bool isInCamp = inXRange && inZRange; // 不检测Y轴，只检测XZ平面

        if (isInCamp)
        {
            Debug.Log($"位置 {position} 在营地内");
            Debug.Log($"营地中心: {campCenter}, 营地大小: {campZoneSize}");
        }

        return isInCamp;
    }

    #region Getter
    /// <summary>
    /// 获取当前活跃的僵尸总数
    /// </summary>
    public int GetTotalActiveZombies()
    {
        return currentZombiesAlive;
    }

    /// <summary>
    /// 获取白天僵尸数量
    /// </summary>
    public int GetDayZombiesCount()
    {
        return dayZombiesCount;
    }

    /// <summary>
    /// 获取夜晚僵尸数量
    /// </summary>
    public int GetNightZombiesCount()
    {
        return nightZombiesCount;
    }
    #endregion

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
