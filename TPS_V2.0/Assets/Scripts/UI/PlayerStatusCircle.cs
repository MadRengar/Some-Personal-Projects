using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusCircle : MonoBehaviour
{
    public enum StatusCircleType
    {
        Satiety,      // 饱食度
        Infectivity,  // 感染度
    }

    [Header("CircleUI Settings")]
    public string parameterName = "";
    public StatusCircleType statusCircleType;

    private PlayerStats playerStats;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();

        PlayerStats.OnSatietyChanged += UpdateUI;
    }

    private void Start()
    {
        SubscribeEvents();
        InitializeUI();
    }

    private void SubscribeEvents()
    {
        void SubscribeEvents()
        {
            switch (statusCircleType)
            {
                case StatusCircleType.Satiety:
                    PlayerStats.OnSatietyChanged += UpdateUI;
                    break;
            }
        }
    }

    private void InitializeUI()
    {
        if (playerStats == null) return;

        switch (statusCircleType)
        {
            case StatusCircleType.Satiety:
                UpdateUI(playerStats.GetCurrentSatiety(), playerStats.GetMaxSatiety());
                break;
        }
    }

    private void UpdateUI(float currentValue, float maxValue)
    {
        if (animator == null || maxValue <= 0) return;

        float ratio = currentValue / maxValue;
        animator.SetFloat(parameterName, ratio);
    }

    void OnDestroy()
    {
        // 取消订阅
        UnsubscribeEvents();
    }
    private void UnsubscribeEvents()
    {
        switch (statusCircleType)
        {
            case StatusCircleType.Satiety:
                PlayerStats.OnSatietyChanged -= UpdateUI;
                break;
        }
    }
}
