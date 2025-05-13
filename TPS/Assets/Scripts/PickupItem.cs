using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryManager inventory;
    public ResourceData_SO resourceData;
    public int amount = 1;
    public PlayerInputSystem playerInputSystem;

    private void Awake()
    {
        playerInputSystem = GameManager.Instance.GetComponent<PlayerInputSystem>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInputSystem.pickUp)
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
                playerInputSystem.pickUp = false;
            }
        }
    }
}
