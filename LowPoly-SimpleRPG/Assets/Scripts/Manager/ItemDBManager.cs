using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*脚本作用：管理ItemDB*/
public class ItemDBManager : MonoBehaviour
{
    //用单例模式管理item信息 
    public static ItemDBManager Instance { get; private set; }

    //持有对ItemDBSO中列表的引用
    public ItemDBSO itemDB;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    //通过随机生成列表索引下标 随机生成凋落物
    public ItemSO GetRandomItem()
    {
        int randomIndex = Random.Range(0, itemDB.itemList.Count);
        return itemDB.itemList[randomIndex]; //返回的是ItemSO类型
    }

}
