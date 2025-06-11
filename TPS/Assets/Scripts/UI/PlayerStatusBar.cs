using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider statusSlider;
    public Text statusText;

    [Header("Status Type")]
    public StatusType statusType;

    [Header("Colors (Option)")]
    public Image fillImage; // Fill对象的Image组件
    public Color dangerColor = Color.red;       // 危险时的颜色（红色）
    public Color normalColor = Color.white;       // 危险时的颜色（红色）
    [Header("Thresholds (Option)")]
    [Range(0f, 1f)]
    public float dangerThreshold = 0.40f;       // 红色阈值

    public enum StatusType
    {
        Health,
        Stamina,
        Satiety,
        Infectivity
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
            case StatusType.Health:
                PlayerStats.OnHealthChanged += UpdateStatusBar;
                break;
        }
    }

    private void UnsubscribeFromEvents()
    {
        switch (statusType)
        {
            case StatusType.Health:
                PlayerStats.OnHealthChanged -= UpdateStatusBar;
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

        int current = 0, max = 0;

        switch (statusType)
        {
            case StatusType.Health:
                current = playerStats.GetCurrentHealth();
                max = playerStats.GetMaxHealth();
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

        if (statusText != null)
        {
            // 暂时用不着
            statusText.text = $"{current}/{max}";
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
}
