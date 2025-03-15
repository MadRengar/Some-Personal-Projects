using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*�ű����ã�����ItemDB*/
public class ItemDBManager : MonoBehaviour
{
    //�õ���ģʽ����item��Ϣ 
    public static ItemDBManager Instance { get; private set; }

    //���ж�ItemDBSO���б������
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

    //ͨ����������б������±� ������ɵ�����
    public ItemSO GetRandomItem()
    {
        int randomIndex = Random.Range(0, itemDB.itemList.Count);
        return itemDB.itemList[randomIndex]; //���ص���ItemSO����
    }

}
