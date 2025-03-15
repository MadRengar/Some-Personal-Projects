using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*���������Ʒ����������*/
public class ItemDetailUI : MonoBehaviour
{
    public Image iconImage;// - ��Ʒ��ͼ��
    public TextMeshProUGUI nameText;// - ��Ʒ������
    public TextMeshProUGUI descriptionText;// - ��Ʒ������
    public TextMeshProUGUI typeText;// - ��Ʒ������
    public GameObject propertyGrid;// - ��������
    public GameObject propertyTemplate;// - ����ģ�� 
    /*ʹ�ð�ť��Ҫ���ݵ���������*/
    private ItemSO itemSO;//ȷ��ʹ�õ�����һ����Ʒ
    private ItemUI itemUI;

    private void Start()
    {
        propertyTemplate.SetActive(false);//������ͨ�������������������Բ���ɸ�ֵ
        this.gameObject.SetActive(false);//����ҵ��Ĭ�ϲ���ʾ
    }

    public void UpdateItemDetailUI(ItemSO itemSO, ItemUI itemUI)
    {
        /*��������*/
        this.itemSO = itemSO;
        this.itemUI = itemUI;

        this.gameObject.SetActive(true);
        string type = "";
        iconImage.sprite = itemSO.icon;
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                type = "����"; break;
            case ItemType.Consumable:
                type = "������Ʒ"; break;
        }
        iconImage.sprite = itemSO.icon;//��ʼ����Ʒ - ͼ��
        nameText.text = itemSO.name;//��ʼ����Ʒ - ����
        typeText.text = type;//��ʼ����Ʒ - ����
        descriptionText.text = itemSO.description;//��ʼ����Ʒ - ����

        /*����µ�����֮ǰ����������� ��Ȼ��ʾ���ص�*/
         foreach(Transform child in propertyGrid.transform)
        {
            //�ж������Ƿ��ڼ���״̬
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
        
        /*���������б� ��ȡ��ֵ*/
        foreach(Property property in itemSO.propertyList)
        {
            //����
            string PropertyStr = "";
            string PropertyName = "";
            switch(property.propertyType)
            {
                case PropertyType.HPValue:
                    PropertyName = "����ֵ:"; 
                    break;
                case PropertyType.EnergyValue:
                    PropertyName = "����ֵ:";
                    break;
                case PropertyType.MentalValue:
                    PropertyName = "����ֵ:";
                    break;
                case PropertyType.SpeedValue:
                    PropertyName = "�ٶ�:";
                    break;
                case PropertyType.AttackValue:
                    PropertyName = "������:";
                    break;
            }
            PropertyStr += PropertyName;
            PropertyStr += property.value;
            GameObject go =  GameObject.Instantiate(propertyTemplate);//ʵ����һ��propertyTemplate(����ģ��)
            go.SetActive(true);
            //go.transform.parent = propertyGrid.transform;//���³�ʼ��������ģ����ص�PropertyGrid��
            go.transform.SetParent(propertyGrid.transform);
            go.transform.Find("Property").GetComponent<TextMeshProUGUI>().text = PropertyStr;//��ֵ
        }
    }

    public void OnUseButtonClick()
    {
        InventoryUI.Instance.OnItemUse(itemSO, itemUI);
        this.gameObject.SetActive(false);//����ҵ��Ĭ�ϲ���ʾ
    }
}
