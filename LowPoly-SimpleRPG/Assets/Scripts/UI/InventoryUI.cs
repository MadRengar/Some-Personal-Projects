using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/*���Ʊ�������ʾ������*/
public class InventoryUI : MonoBehaviour
{
    private GameObject uiGameObject;
    private GameObject content;

    public GameObject itemPrefab;
    private bool isShow = false;//�жϱ�����ǰ�Ƿ�չʾ
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
        uiGameObject = transform.Find("UI").gameObject;//���UI���� Ϊ�˿��Ʊ�����ʾ&��ʧ
        content = transform.Find("UI/ListBG/Scroll View/Viewport/Content").gameObject;//��ù��������е�content Ϊ��ȷ��������Ʒ��λ��
        Hide();//Ĭ��Ϊ����״̬
    }

    /*B�� ���ر���*/
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            //������ǰ���ڴ�״̬
            if (isShow)
            {
                Hide();// �رձ���
                isShow = false;//���ı�־λ
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
        //itemGo.transform.parent = content.transform;//����Ʒ���ص�content��
        itemGo.transform.SetParent(content.transform);//�°��Ƽ�
        itemGo.transform.localScale = Vector3.one;//�ӣ��������ڱ����������ɵ���Ʒ��scale��Ԥ����һ��
        ItemUI itemUI = itemGo.GetComponent<ItemUI>();
        //itemUI.InitItem(itemSO.icon, itemSO.name, itemSO.type); �ᱨ���޷��ӡ�ItemType��ת��Ϊ��string��	
        itemUI.InitItem(itemSO);
    }


    /*���ã�����InventoryUI�ĸ�item�������*/
    public void OnItemClick(ItemSO itemSO, ItemUI itemUI)
    {
        itemDetailUI.UpdateItemDetailUI(itemSO, itemUI);
    }

    /*�������ʹ�ð�ť�߼�*/
    public void OnItemUse(ItemSO itemSO, ItemUI itemUI)
    {
        Destroy(itemUI.gameObject);//������Ϸ����  PS��itemUI��Ϊһ��item����� ��ʵ��������item
        InventoryManager.Instance.RemoveItem(itemSO);//������Ʒ�����ݴӱ����б����Ƴ�

        GameObject.FindGameObjectWithTag(Tag.PLAYER).GetComponent<Player>().UseItem(itemSO);
    }
}
