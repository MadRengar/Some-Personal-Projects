using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*负责更新物品点击后的详情*/
public class ItemDetailUI : MonoBehaviour
{
    public Image iconImage;// - 物品的图标
    public TextMeshProUGUI nameText;// - 物品的名字
    public TextMeshProUGUI descriptionText;// - 物品的描述
    public TextMeshProUGUI typeText;// - 物品的类型
    public GameObject propertyGrid;// - 背包网格
    public GameObject propertyTemplate;// - 属性模板 
    /*使用按钮需要传递的两个参数*/
    private ItemSO itemSO;//确定使用的是哪一个物品
    private ItemUI itemUI;

    private void Start()
    {
        propertyTemplate.SetActive(false);//后续会通过复制来生成其他属性并完成赋值
        this.gameObject.SetActive(false);//详情业面默认不显示
    }

    public void UpdateItemDetailUI(ItemSO itemSO, ItemUI itemUI)
    {
        /*保存数据*/
        this.itemSO = itemSO;
        this.itemUI = itemUI;

        this.gameObject.SetActive(true);
        string type = "";
        iconImage.sprite = itemSO.icon;
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                type = "武器"; break;
            case ItemType.Consumable:
                type = "可消耗品"; break;
        }
        iconImage.sprite = itemSO.icon;//初始化物品 - 图标
        nameText.text = itemSO.name;//初始化物品 - 名称
        typeText.text = type;//初始化物品 - 类型
        descriptionText.text = itemSO.description;//初始化物品 - 描述

        /*添加新的属性之前清空所有属性 不然显示会重叠*/
         foreach(Transform child in propertyGrid.transform)
        {
            //判断自身是否处于激活状态
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
        
        /*遍历属性列表 获取数值*/
        foreach(Property property in itemSO.propertyList)
        {
            //名称
            string PropertyStr = "";
            string PropertyName = "";
            switch(property.propertyType)
            {
                case PropertyType.HPValue:
                    PropertyName = "生命值:"; 
                    break;
                case PropertyType.EnergyValue:
                    PropertyName = "饥饿值:";
                    break;
                case PropertyType.MentalValue:
                    PropertyName = "精神值:";
                    break;
                case PropertyType.SpeedValue:
                    PropertyName = "速度:";
                    break;
                case PropertyType.AttackValue:
                    PropertyName = "攻击力:";
                    break;
            }
            PropertyStr += PropertyName;
            PropertyStr += property.value;
            GameObject go =  GameObject.Instantiate(propertyTemplate);//实例化一个propertyTemplate(属性模板)
            go.SetActive(true);
            //go.transform.parent = propertyGrid.transform;//将新初始化的属性模板挂载到PropertyGrid下
            go.transform.SetParent(propertyGrid.transform);
            go.transform.Find("Property").GetComponent<TextMeshProUGUI>().text = PropertyStr;//赋值
        }
    }

    public void OnUseButtonClick()
    {
        InventoryUI.Instance.OnItemUse(itemSO, itemUI);
        this.gameObject.SetActive(false);//详情业面默认不显示
    }
}
