using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObject : InteractableObject
{
    public ItemSO itemSO;
    protected override void Interact()
    {
        print("Interacting with PickableObject!");

        Destroy(this.gameObject);
        InventoryManager.Instance.AddItem(itemSO);
    }
}
