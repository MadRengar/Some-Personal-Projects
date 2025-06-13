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
    }

    private PlayerStats playerStats;
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
        }
    }

    private void InitializeStatusBar()
    {
        // 获取PlayerStats引用
        playerStats = GameManager.Instance?.GetPlayerStats();

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null) return;

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
