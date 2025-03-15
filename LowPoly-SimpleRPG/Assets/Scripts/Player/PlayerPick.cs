using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPick : MonoBehaviour
{
    //ʹ��OnCollisionEnter()���� ʹ����ҿ��Ժ����巢����ײ
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == Tag.INTERACTABLE)
        {
            PickableObject po = collision.gameObject.GetComponent<PickableObject>();
            //���PickableObject������� ˵����������Լ���(��PickableObject����е���һ���ж���Ʒ�Ƿ��ܼ���ı�־ && ������itemSO)
            if (po != null) 
            {
                InventoryManager.Instance.AddItem(po.itemSO);
                Destroy(po.gameObject);
            }
        }
    }
}
