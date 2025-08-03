using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreatmentController : MonoBehaviour
{
    [Header("Treatment Value")]
    [SerializeField] private bool playerInTreatmentArea = false;
    [SerializeField] private bool aiInTreatmentArea = false;
    [SerializeField] private int playerRecoverRate = 3;
    [SerializeField] private int aiRecoverRate = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RadioPopController.Instance.ShowMessage(MessageKey.Player_EnterTreatmentArea, RadioPopController.MessageType.Info);
            playerInTreatmentArea = true;
        }

        if (other.CompareTag("AIPlayer"))
        {
            RadioPopController.Instance.ShowMessage(MessageKey.AI_EnterTreatmentArea, RadioPopController.MessageType.Info);
            aiInTreatmentArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTreatmentArea = false;
        }

        if (other.CompareTag("AIPlayer"))
        {
            aiInTreatmentArea = false;
        }
    }

    #region Getter
    public bool IsPlayerInTreatmentArea()
    {
        return playerInTreatmentArea;
    }

    public bool IsAIPlayerInTreatmentArea()
    {
        return aiInTreatmentArea;
    }

    public int GetPlayerRecoverRate()
    {
        return playerRecoverRate;
    }

    public int GetAIPlayerRecoverRate()
    {
        return aiRecoverRate;
    }
    #endregion
}
