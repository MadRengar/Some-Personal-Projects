using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolInfo
    {
        public ResourceData_SO resourceData;
        public GameObject resourcePrefab;
        public int initialPoolSize = 40; // 明确只用于池的初始容量
    }
    public List<PoolInfo> resourceTypes;
    private Dictionary<ResourceType, Queue<GameObject>> pools = new();

    void Awake()
    {
        // 初始化每种类型的池
        foreach (var info in resourceTypes)
        {
            if (!pools.ContainsKey(info.resourceData.type))
                pools.Add(info.resourceData.type, new Queue<GameObject>());
            for (int i = 0; i < info.initialPoolSize; i++)
            {
                var obj = Instantiate(info.resourcePrefab);
                obj.SetActive(false);
                pools[info.resourceData.type].Enqueue(obj);
            }
        }
    }

    public GameObject Get(ResourceType type)
    {
        if (pools.TryGetValue(type, out var pool) && pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        Debug.LogWarning($"池空了: {type}，动态创建！");
        // 池空则新建
        var prefabInfo = resourceTypes.Find(info => info.resourceData.type == type);
        if (prefabInfo != null)
        {
            var obj = Instantiate(prefabInfo.resourcePrefab);
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public void Return(ResourceType type, GameObject obj)
    {
        obj.SetActive(false);
        if (!pools.ContainsKey(type))
            pools[type] = new Queue<GameObject>();
        pools[type].Enqueue(obj);
    }
}
