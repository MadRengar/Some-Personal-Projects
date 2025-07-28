using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingStateUIController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public GameObject lowPowerTip;

    [Header("References")]
    public MonoBehaviour controller; // 引用 GeneratorController、TurretController、StorageController

    private IBuildingController buildingController;
    private Transform cam;

    private int currentHealth;
    private int maxHealth;
    private bool hasPower = true;

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

        maxHealth = buildingController.GetBuildingData().maxHealth;
    }

    void LateUpdate()
    {
        // 面向相机
        if (cam != null)
        {
            transform.forward = cam.forward;
        }

        UpdateHealth();
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
            hasPower = false; // 发电机始终有电
        }
        else if (controller is StorageController storage)
        {
            currentHealth = storage.GetCurrentHealth();
            hasPower = false; // 仓库不涉及供电系统
        }
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            float percent = Mathf.Clamp01((float)currentHealth / maxHealth);
            healthSlider.value = percent;
        }

        if (lowPowerTip != null)
        {
            lowPowerTip.SetActive(!hasPower);
        }
    }
}
