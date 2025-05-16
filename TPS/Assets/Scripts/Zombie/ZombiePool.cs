using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    [Header("Zombie Pool Settings")]
    public GameObject zombiePrefab; // 僵尸预制体
    public int poolSize = 10; // 初始池容量
    public Transform zombieContainer; // 用来收纳僵尸实例

    private Queue<GameObject> zombieQueue = new Queue<GameObject>();

    public static ZombiePool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePool();
    }

    /*初始化对象池*/
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject zombie = Instantiate(zombiePrefab, zombieContainer);
            zombie.SetActive(false);
            zombieQueue.Enqueue(zombie);

        }
    }

    /// <summary>
    /// 从池中获取一个僵尸并激活
    /// </summary>
    public GameObject TrySpawnZombie(Vector3 position, Quaternion rotation)
    {
        GameObject zombie;

        if (zombieQueue.Count > 0)
        {
            zombie = zombieQueue.Dequeue();
        }
        else
        {
            Debug.LogWarning("超出可用僵尸对象数量！");
            zombie = Instantiate(zombiePrefab, zombieContainer); // 动态扩容
        }

        zombie.transform.SetPositionAndRotation(position, rotation);
        zombie.SetActive(true);

        ZombieStats stats = zombie.GetComponent<ZombieStats>();
        if (stats != null)
        {
            stats.ResetZombie(); // 确保状态被重置
        }

        return zombie;
    }

    /// <summary>
    /// 回收僵尸回池
    /// </summary>
    public void DespawnZombie(GameObject zombie)
    {
        zombie.SetActive(false);
        zombieQueue.Enqueue(zombie);
    }
}
