using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*��������*/
public class InventoryManager : MonoBehaviour
{
    //����ģʽ
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

    /*Э�� + �ӳ�1sִ�� ��ֹ��ָ��*/
    //IEnumerator Start()
    //{
    //    yield return new WaitForSeconds(1);
    //    AddItem(defaultWeapon);//�����һ��Ĭ������
    //}

    //����������ӵ�������
    public void AddItem(ItemSO itemSO)
    {
        itemList.Add(itemSO);//��������
        InventoryUI.Instance.AddItem(itemSO);//�ڱ�����������Ʒ

        MessageUI.Instance.Show("������" + itemSO.name);//������Ʒ����ʾ
    }

    public void RemoveItem(ItemSO itemSO)
    {
        itemList.Remove(itemSO);
    }
}
