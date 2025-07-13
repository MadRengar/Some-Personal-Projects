using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("ZombieManager ref")]
    public ZombieManager zombieManager;

    [Header("Spawner Settings")]
    public float spawnInterval = 2f; // 生成间隔（秒）
    public float spawnRadius = 2f; // 生成范围半径

    [Header("Zombie State")]
    [SerializeField] private int berserkHealth = 60;
    [Header("Debug")]
    public bool showGizmos = true;

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    /// <summary>
    /// 开始生成僵尸
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        //Debug.Log($"僵尸生成器 {gameObject.name} 开始生成僵尸");

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnZombiesCoroutine());
    }

    /// <summary>
    /// 停止生成僵尸
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning) return;

        isSpawning = false;
        //Debug.Log($"僵尸生成器 {gameObject.name} 停止生成僵尸");

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 生成僵尸协程
    /// </summary>
    private IEnumerator SpawnZombiesCoroutine()
    {
        while (isSpawning)
        {
            // 持续生成僵尸，直到黎明
            SpawnZombie();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 生成单个僵尸
    /// </summary>
    private void SpawnZombie()
    {
        // 游戏结束检查
        if (GameManager.Instance.IsGameOver())
        {
            return; // 游戏结束时不生成僵尸
        }

        // 数量检查
        if (zombieManager == null || !zombieManager.CanSpawnMoreZombies())
        {
            return; // 达到数量限制，不生成
        }

        if (ZombiePool.Instance == null)
        {
            Debug.LogError("ZombiePool.Instance 为空，无法生成僵尸");
            return;
        }

        // 随机生成位置
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // 从对象池获取僵尸
        GameObject zombie = ZombiePool.Instance.TrySpawnZombie(spawnPosition, Quaternion.identity);

        if (zombie != null)
        {
            // 设置僵尸为狂暴状态
            SetZombieBerserkState(zombie, true);

            // 通知管理器注册这个僵尸
            zombieManager.RegisterSpawnedZombie(zombie);

            //Debug.Log($"在 {gameObject.name} 生成了狂暴僵尸");
        }
    }

    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // 简单的地面检测
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            spawnPosition.y = hit.point.y;
        }

        return spawnPosition;
    }

    /// <summary>
    /// 设置僵尸狂暴状态
    /// </summary>
    private void SetZombieBerserkState(GameObject zombie, bool isBerserk)
    {
        // 获取僵尸的数据组件
        var zombieStats = zombie.GetComponent<ZombieStats>();
        if (zombieStats != null)
        {
            zombieStats.isBerserk = isBerserk;
            zombieStats.currentHealth = berserkHealth;
        }

        // 重置僵尸FSM以应用新状态
        var zombieFSM = zombie.GetComponent<ZombieFSM>();
        if (zombieFSM != null)
        {
            zombieFSM.ResetZombieFSM();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // 绘制生成范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // 绘制生成器标识
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
