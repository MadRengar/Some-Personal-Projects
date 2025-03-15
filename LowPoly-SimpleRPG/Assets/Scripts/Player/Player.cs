using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private PlayerProperty playerProperty;
    private void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerProperty = GetComponent<PlayerProperty>();//修复报错 原因：少加了这一句代码，没有获取到PlayerProperty组件
    }                                                   //导致报错Object reference not set to an instance of an object
    public void UseItem(ItemSO itemSO)
    {
        /*判断物品类型*/
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                playerAttack.LoadWeapon(itemSO);//装备武器
                break;
            case ItemType.Consumable:
                playerProperty.UseDrug(itemSO);
                break;
        }
    }
}
