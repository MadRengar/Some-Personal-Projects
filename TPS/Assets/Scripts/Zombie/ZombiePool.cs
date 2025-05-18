using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{

    [System.Serializable]
    public class ZombieType
    {
        public string name;
        public GameObject prefab;
        public ZombieData_SO zombieData;
    }

    [Header("Zombie Pool Settings")]
    public GameObject zombiePrefab; // 僵尸预制体
    public int poolSize = 10; // 初始池容量
    public Transform zombieContainer; // 用来收纳僵尸实例

    private Queue<GameObject> zombieQueue = new Queue<GameObject>(); // 先进先出的数据结构

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
            zombieQueue.Enqueue(zombie); // 加入到队列末尾，等待调用。

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
            zombie = zombieQueue.Dequeue(); // 从队列中取出一个
        }
        else
        {
            Debug.LogWarning("超出可用僵尸对象数量！");
            /*动态扩容
             * 重大Bug：新扩容的僵尸NavMesh导航报错
             * 1."Resume" can only be called on an active agent that has been placed on a NavMesh.
             * 2."SetDestination" can only be called on an active agent that has been placed on a NavMesh.
             */
            zombie = Instantiate(zombiePrefab, zombieContainer);
        }

        zombie.transform.SetPositionAndRotation(position, rotation);
        zombie.SetActive(true);
        return zombie;
    }

    /// <summary>
    /// 回收僵尸回池
    /// </summary>
    public void DespawnZombie(GameObject zombie)
    {
        zombie.SetActive(false);
        zombieQueue.Enqueue(zombie); // 放回对象池
    } 
}
