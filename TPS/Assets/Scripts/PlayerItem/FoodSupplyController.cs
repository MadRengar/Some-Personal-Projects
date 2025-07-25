using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSupplyController : MonoBehaviour
{
    [Header("Supply Value")]
    [SerializeField] private float supplySatietyPerSec = 1f;

    private Coroutine satietyDecayCoroutine;
    private bool playerInFoodRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInFoodRange = true;
            RadioPopController.Instance.ShowMessage(MessageKey.Interact_foodSupply, RadioPopController.MessageType.Info);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInFoodRange = false;
        }
    }

    public bool isPlayerInRange()
    {
        return playerInFoodRange;
    }

    public float GetSupplySatietyPerSec()
    {
        return supplySatietyPerSec;
    }
}
