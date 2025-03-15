using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;

    private ItemSO itemSO;

    public void InitItem(ItemSO itemSO)
    {
        string type = "";
        /*关于type的处理：
         itemSO.type为枚举类型
         所以这里用switch做一下判断
         */
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
        /*保存一个itemSO的对象
         作用：当一个item被点击以后就可以知道点击的是哪一个
         */
        this.itemSO = itemSO;
    }

    //记得在引擎中注册OnClick方法
    public void OnClick()
    {
        InventoryUI.Instance.OnItemClick(itemSO, this);//传递：1 itemSO为了详情页面物品信息的显示；2 this即为item实例
    }
}
