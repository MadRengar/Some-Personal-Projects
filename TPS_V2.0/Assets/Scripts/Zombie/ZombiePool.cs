using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    [System.Serializable]
    public class ZombieModelVariant
    {
        public string name;
        public GameObject modelPrefab;
        [Range(0f, 1f)]
        public float spawnWeight = 1f; // 生成权重，控制出现概率
    }

    [Header("Zombie Pool Settings")]
    public GameObject zombiePrefab; // 僵尸预制体
    public int poolSize = 10; // 初始池容量
    public Transform zombieContainer; // 用来收纳僵尸实例

    [Header("Model Variants")]
    public ZombieModelVariant[] modelVariants; // 不同的模型变体
    public bool useRandomModels = true; // 是否启用随机模型

    private Queue<GameObject> zombieQueue = new Queue<GameObject>(); // 先进先出的数据结构
    private List<float> cumulativeWeights = new List<float>(); // 累积权重，用于加权随机

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
        // 新增：初始化权重系统
        if (useRandomModels && modelVariants.Length > 0)
        {
            CalculateCumulativeWeights();
        }
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

        // 在激活前应用随机模型
        if (useRandomModels && modelVariants.Length > 0)
        {
            ApplyRandomModel(zombie);
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

    /// <summary>
    /// 计算累积权重
    /// </summary>
    private void CalculateCumulativeWeights()
    {
        cumulativeWeights.Clear();
        float totalWeight = 0f;

        foreach (var variant in modelVariants)
        {
            totalWeight += variant.spawnWeight;
            cumulativeWeights.Add(totalWeight);
        }
    }

    /// <summary>
    /// 应用随机模型
    /// </summary>
    private void ApplyRandomModel(GameObject zombie)
    {
        int selectedIndex = GetWeightedRandomIndex();
        var selectedVariant = modelVariants[selectedIndex];

        if (selectedVariant.modelPrefab != null)
        {
            ReplaceZombieModel(zombie, selectedVariant.modelPrefab);
        }
    }

    /// <summary>
    /// 基于权重的随机选择
    /// </summary>
    private int GetWeightedRandomIndex()
    {
        if (cumulativeWeights.Count == 0) return 0;

        float randomValue = Random.Range(0f, cumulativeWeights[cumulativeWeights.Count - 1]);

        for (int i = 0; i < cumulativeWeights.Count; i++)
        {
            if (randomValue <= cumulativeWeights[i])
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// 替换僵尸模型
    /// </summary>
    private void ReplaceZombieModel(GameObject zombie, GameObject newModelPrefab)
    {
        // 找到并删除Root节点
        Transform rootTransform = zombie.transform.Find("Root");
        if (rootTransform != null)
        {
            DestroyImmediate(rootTransform.gameObject);
        }

        // 找到并删除身体模型
        Transform bodyModel = null;
        foreach (Transform child in zombie.transform)
        {
            if (child.name.Contains("SM_Chr_"))
            {
                bodyModel = child;
                break;
            }
        }

        if (bodyModel != null)
        {
            DestroyImmediate(bodyModel.gameObject);
        }

        // 直接实例化新的完整模型
        GameObject newModel = Instantiate(newModelPrefab, zombie.transform);
        newModel.transform.localPosition = Vector3.zero;
        newModel.transform.localRotation = Quaternion.identity;
        newModel.transform.localScale = Vector3.one;

        // 保持层级一致
        SetLayerRecursively(newModel.transform, zombie.layer);
    }

    /// <summary>
    /// 递归设置层级
    /// </summary>
    private void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
