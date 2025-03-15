using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/*控制背包的显示与隐藏*/
public class InventoryUI : MonoBehaviour
{
    private GameObject uiGameObject;
    private GameObject content;

    public GameObject itemPrefab;
    private bool isShow = false;//判断背包当前是否展示
    public ItemDetailUI itemDetailUI;

    public static InventoryUI Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this; 
    }

    void Start()
    {
        uiGameObject = transform.Find("UI").gameObject;//获得UI对象 为了控制背包显示&消失
        content = transform.Find("UI/ListBG/Scroll View/Viewport/Content").gameObject;//获得滚动对象中的content 为了确定增加物品的位置
        Hide();//默认为隐藏状态
    }

    /*B键 开关背包*/
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            //背包当前处于打开状态
            if (isShow)
            {
                Hide();// 关闭背包
                isShow = false;//更改标志位
            }
            else 
            { 
                Show();
                isShow = true;
            }
        }     
    }

    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    public void AddItem(ItemSO itemSO)
    {
        GameObject itemGo =  GameObject.Instantiate(itemPrefab);
        //itemGo.transform.parent = content.transform;//将物品挂载到content下
        itemGo.transform.SetParent(content.transform);//新版推荐
        itemGo.transform.localScale = Vector3.one;//坑！：设置在背包中新生成的物品的scale和预制体一样
        ItemUI itemUI = itemGo.GetComponent<ItemUI>();
        //itemUI.InitItem(itemSO.icon, itemSO.name, itemSO.type); 会报错：无法从“ItemType”转换为“string”	
        itemUI.InitItem(itemSO);
    }


    /*作用：告诉InventoryUI哪个item被点击了*/
    public void OnItemClick(ItemSO itemSO, ItemUI itemUI)
    {
        itemDetailUI.UpdateItemDetailUI(itemSO, itemUI);
    }

    /*详情界面使用按钮逻辑*/
    public void OnItemUse(ItemSO itemSO, ItemUI itemUI)
    {
        Destroy(itemUI.gameObject);//消除游戏对象  PS：itemUI作为一个item的组件 其实例化就是item
        InventoryManager.Instance.RemoveItem(itemSO);//将该物品的数据从背包列表中移除

        GameObject.FindGameObjectWithTag(Tag.PLAYER).GetComponent<Player>().UseItem(itemSO);
    }
}
