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
        /*����type�Ĵ���
         itemSO.typeΪö������
         ����������switch��һ���ж�
         */
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
        /*����һ��itemSO�Ķ���
         ���ã���һ��item������Ժ�Ϳ���֪�����������һ��
         */
        this.itemSO = itemSO;
    }

    //�ǵ���������ע��OnClick����
    public void OnClick()
    {
        InventoryUI.Instance.OnItemClick(itemSO, this);//���ݣ�1 itemSOΪ������ҳ����Ʒ��Ϣ����ʾ��2 this��Ϊitemʵ��
    }
}
