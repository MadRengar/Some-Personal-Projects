using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*背包管理*/
public class InventoryManager : MonoBehaviour
{
    //单例模式
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
    }

    public List<ItemSO> itemList;
    //public ItemSO defaultWeapon;

    /*协程 + 延迟1s执行 防止空指针*/
    //IEnumerator Start()
    //{
    //    yield return new WaitForSeconds(1);
    //    AddItem(defaultWeapon);//给玩家一件默认武器
    //}

    //将掉落物添加到背包里
    public void AddItem(ItemSO itemSO)
    {
        itemList.Add(itemSO);//保存数据
        InventoryUI.Instance.AddItem(itemSO);//在背包中生成物品

        MessageUI.Instance.Show("你获得了" + itemSO.name);//捡起物品的提示
    }

    public void RemoveItem(ItemSO itemSO)
    {
        itemList.Remove(itemSO);
    }
}
