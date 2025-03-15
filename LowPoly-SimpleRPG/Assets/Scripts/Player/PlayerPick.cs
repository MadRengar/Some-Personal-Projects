using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPick : MonoBehaviour
{
    //使用OnCollisionEnter()方法 使得玩家可以和物体发生碰撞
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == Tag.INTERACTABLE)
        {
            PickableObject po = collision.gameObject.GetComponent<PickableObject>();
            //如果PickableObject组件存在 说明该物体可以捡起(即PickableObject组件有点像一个判断物品是否能捡起的标志 && 并传递itemSO)
            if (po != null) 
            {
                InventoryManager.Instance.AddItem(po.itemSO);
                Destroy(po.gameObject);
            }
        }
    }
}
