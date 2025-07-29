using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingStateUIController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;        // 对应 Health_Bar
    public Slider capacitySlider;      // 对应 Capacity_Bar
    public GameObject BuildingType;    // 对应 Type

    [Header("Resource Display")]
    public TextMeshProUGUI woodAmountText;  // 对应 Wood/Amount
    public TextMeshProUGUI ironAmountText;  // 对应 Iron/Amount

    [Header("References")]
    public MonoBehaviour controller; // 引用 GeneratorController、TurretController、StorageController

    private IBuildingController buildingController;
    private StorageController storageController; // 新增：仓库控制器引用
    private Transform cam;

    private int currentHealth;
    private int maxHealth;
    private bool hasPower = true;

    // 仓库相关数据
    private float currentCapacity;
    private float maxCapacity;
    private int currentWood;
    private int currentIron;

    void Start()
    {
        cam = Camera.main.transform;

        buildingController = controller as IBuildingController;
        if (buildingController == null)
        {
            Debug.LogError("缺少 IBuildingController 实现！");
            enabled = false;
            return;
        }

        // 检查是否为仓库
        storageController = controller as StorageController;

        maxHealth = buildingController.GetBuildingData().maxHealth;

        maxCapacity = storageController.storageData.storageCapacity;
    

        // 初始化容量滑动条
        if (capacitySlider != null)
        {
            if (storageController != null)
            {
                capacitySlider.gameObject.SetActive(true);
                capacitySlider.maxValue = 1f; // 使用百分比显示
                capacitySlider.value = 0f;
            }
            else
            {
                // 非仓库建筑隐藏容量条
                capacitySlider.gameObject.SetActive(false);
            }
        }

        // 初始化资源文本显示
        if (storageController != null)
        {
            // 仓库建筑：显示资源文本
            if (woodAmountText != null)
                woodAmountText.gameObject.SetActive(true);
            if (ironAmountText != null)
                ironAmountText.gameObject.SetActive(true);
        }
        else
        {
            // 非仓库建筑：隐藏资源文本
            if (woodAmountText != null)
                woodAmountText.gameObject.SetActive(false);
            if (ironAmountText != null)
                ironAmountText.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        // 面向相机
        if (cam != null)
        {
            transform.forward = cam.forward;
        }

        UpdateHealth();
        UpdateStorageData();
        UpdateUI();
    }

    private void UpdateHealth()
    {
        // 类型判断，强类型调用
        if (controller is TurretController turret)
        {
            currentHealth = turret.GetCurrentHealth();
            hasPower = turret.IsPowered();
        }
        else if (controller is GeneratorController generator)
        {
            currentHealth = generator.GetCurrentHealth();
            hasPower = false; // 发电机自己有电
        }
        else if (controller is StorageController storage)
        {
            currentHealth = storage.GetCurrentHealth();
            hasPower = false; // 仓库不涉及供电系统
        }
    }

    private void UpdateStorageData()
    {
        // 仅在是仓库时更新容量和资源数据
        if (storageController != null)
        {
            currentCapacity = storageController.GetCurrentUsedWeight();
            currentWood = storageController.GetStoredWood();
            currentIron = storageController.GetStoredIron();
        }
    }

    private void UpdateUI()
    {
        // 更新生命值滑动条
        if (healthSlider != null)
        {
            float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);
            healthSlider.value = healthPercent;
        }

        // 更新容量滑动条（仅仓库）
        if (capacitySlider != null && storageController != null)
        {
            float capacityPercent = maxCapacity > 0 ? Mathf.Clamp01(currentCapacity / maxCapacity) : 0f;
            capacitySlider.value = capacityPercent;
        }

        // 更新资源数量显示（仅仓库）
        if (storageController != null)
        {
            if (woodAmountText != null)
            {
                woodAmountText.text = currentWood.ToString();
            }

            if (ironAmountText != null)
            {
                ironAmountText.text = currentIron.ToString();
            }
        }

        // 更新建筑类型显示（电力状态）
        if (BuildingType != null)
        {
            BuildingType.SetActive(!hasPower);
        }
    }

    /// <summary>
    /// 公共方法：获取当前建筑类型（用于调试）
    /// </summary>
    public string GetBuildingType()
    {
        if (controller is TurretController)
            return "Turret";
        else if (controller is GeneratorController)
            return "Generator";
        else if (controller is StorageController)
            return "Storage";
        else
            return "Unknown";
    }

    /// <summary>
    /// 公共方法：获取当前健康百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    /// <summary>
    /// 公共方法：获取当前容量百分比（仅仓库有效）
    /// </summary>
    public float GetCapacityPercentage()
    {
        if (storageController != null && maxCapacity > 0)
        {
            return currentCapacity / maxCapacity;
        }
        return 0f;
    }
}