using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Inventory inventory;
    public ResourceData_SO resourceData;
    public int amount = 1;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            if (inventory != null)
            {
                bool success = inventory.TryAdd(resourceData, amount);
                if (success)
                {
                    Destroy(gameObject);
                    Debug.Log($"拾取成功：{resourceData.resourceName} x{amount} 当前重量：{inventory.CurrentWeight}");
                }
                else
                {
                    Debug.Log("背包超重，无法拾取");
                }
            }
        }
    }
}
