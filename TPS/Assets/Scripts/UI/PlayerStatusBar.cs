using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider statusSlider;

    [Header("Status Type")]
    public StatusBarType statusType;

    [Header("Colors (Option)")]
    public Image fillImage; // Fill对象的Image组件
    public Color lowLevelColor = Color.blue;
    public Color dangerColor = Color.red;       // 危险时的颜色（红色）
    public Color normalColor = Color.white;       // 危险时的颜色（红色）
    [Header("Thresholds (Option)")]
    [Range(0f, 1f)]
    public float dangerThreshold = 0.40f;       // 红色阈值

    public enum StatusBarType
    {
        Health,
        Stamina,
        AIHealth,
    }

    private PlayerStats playerStats;
    private AITeammateState aITeammateStats;

    void Start()
    {
        SubscribeToEvents();
        InitializeStatusBar();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        switch (statusType)
        {
            case StatusBarType.Health: // 生命值
                PlayerStats.OnHealthChanged += UpdateStatusBar;
                break;
            case StatusBarType.Stamina: // 体力
                PlayerStats.OnStaminaChanged += UpdateStatusBar;
                break;
            case StatusBarType.AIHealth:
                // 如果有AI状态事件的话在这里订阅
                AITeammateState.AIOnHealthChanged += UpdateStatusBar;
                break;
        }
    }

    private void UnsubscribeFromEvents()
    {
        switch (statusType)
        {
            case StatusBarType.Health:
                PlayerStats.OnHealthChanged -= UpdateStatusBar;
                break;
            case StatusBarType.Stamina:
                PlayerStats.OnStaminaChanged -= UpdateStatusBar;
                break;
            case StatusBarType.AIHealth:
                AITeammateState.AIOnHealthChanged -= UpdateStatusBar;
                break;
        }
    }

    private void InitializeStatusBar()
    {
        // 根据状态类型决定需要获取哪些引用
        if (statusType == StatusBarType.Health || statusType == StatusBarType.Stamina)
        {
            playerStats = GameManager.Instance?.GetPlayerStats();
            if (playerStats == null)
            {
                playerStats = FindObjectOfType<PlayerStats>();
            }
            if (playerStats == null) return; // 只有需要playerStats时才检查
        }

        if (statusType == StatusBarType.AIHealth)
        {
            aITeammateStats = GameManager.Instance?.GetAIAgentStats();
            if (aITeammateStats == null)
            {
                aITeammateStats = FindObjectOfType<AITeammateState>();
            }
            if (aITeammateStats == null) return; // 只有需要AI状态时才检查
        }

        float current = 0;
        float max = 0;
        switch (statusType)
        {
            case StatusBarType.Health:
                current = playerStats.GetCurrentHealth();
                max = playerStats.GetMaxHealth();
                break;
            case StatusBarType.Stamina:
                current = playerStats.GetCurrentStamina();
                max = playerStats.GetMaxStamina();
                break;
            case StatusBarType.AIHealth:
                if (aITeammateStats != null)
                {
                    current = aITeammateStats.currentHealth;
                    max = aITeammateStats.playerData.maxHealth;
                    //Debug.Log($"ai当前生命值：{current}！！！！！");
                }
                break;
        }
        UpdateStatusBar(current, max);
    }

    private void UpdateStatusBar(int current, int max)
    {
        if (statusSlider != null)
        {
            statusSlider.value = (float)current / max;
            UpdateHealthBarColor(statusSlider.value, max);
        }

    }

    private void UpdateStatusBar(float current, float max)
    {
        if (statusSlider != null)
        {
            statusSlider.value = (float)current / max;
            UpdateHealthBarColor(statusSlider.value, max);
        }
    }

    private void UpdateHealthBarColor(float healthRatio, int max)
    {
        if (fillImage == null) return;

        Color targetColor = normalColor;

        if(healthRatio <= dangerThreshold)
        {
            targetColor = dangerColor;
        }
        else if(healthRatio < max)
        {
            float t = (healthRatio - dangerThreshold) / (1f - dangerThreshold);
            targetColor = Color.Lerp(dangerColor, normalColor, t);
        }

        fillImage.color = targetColor;
    }

    private void UpdateHealthBarColor(float healthRatio, float max)
    {
        if (fillImage == null) return;

        Color targetColor = normalColor;

        if (healthRatio <= dangerThreshold)
        {
            targetColor = dangerColor;
        }
        else if (healthRatio < max)
        {
            float t = (healthRatio - dangerThreshold) / (1f - dangerThreshold);
            targetColor = Color.Lerp(dangerColor, normalColor, t);
        }

        fillImage.color = targetColor;
    }
}
