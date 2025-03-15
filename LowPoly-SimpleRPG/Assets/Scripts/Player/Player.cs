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
        playerProperty = GetComponent<PlayerProperty>();//�޸����� ԭ���ټ�����һ����룬û�л�ȡ��PlayerProperty���
    }                                                   //���±���Object reference not set to an instance of an object
    public void UseItem(ItemSO itemSO)
    {
        /*�ж���Ʒ����*/
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                playerAttack.LoadWeapon(itemSO);//װ������
                break;
            case ItemType.Consumable:
                playerProperty.UseDrug(itemSO);
                break;
        }
    }
}
