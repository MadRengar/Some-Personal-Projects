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
            Debug.Log("玩家进入治疗区域！");
            playerInTreatmentArea = true;
        }

        if (other.CompareTag("AIPlayer"))
        {
            Debug.Log("ai进入治疗区域！");
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
