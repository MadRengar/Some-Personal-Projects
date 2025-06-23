using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoUIController : MonoBehaviour
{
    [Header("UI Ref")]
    public TextMeshProUGUI currentAmmoText; // 当前弹药文本
    public TextMeshProUGUI totalAmmoText;   // 总弹药文本

    [Header("Weapon Ref")]
    public WeaponManager weaponManager; // 武器管理器引用

    private void Start()
    {
        // 如果没有手动分配，自动查找WeaponManager
        if (weaponManager == null)
        {
            weaponManager = FindObjectOfType<WeaponManager>();
        }

        if (weaponManager != null)
        {
            // 订阅弹药变化事件
            weaponManager.OnAmmoChanged += UpdateAmmoUI;

            // 初始化显示
            UpdateAmmoUI(weaponManager.GetCurrentAmmo(), weaponManager.GetReserveAmmo());
        }
        else
        {
            Debug.LogError("AmmoUIController: 未找到WeaponManager组件！");
        }
    }

    private void OnDestroy()
    {
        // 取消事件订阅
        if (weaponManager != null)
        {
            weaponManager.OnAmmoChanged -= UpdateAmmoUI;
        }
    }

    /// <summary>
    /// 更新弹药UI显示
    /// </summary>
    private void UpdateAmmoUI(int currentAmmo, int reserveAmmo)
    {
        // 更新当前弹药数量
        if (currentAmmoText != null)
        {
            currentAmmoText.text = currentAmmo.ToString();
        }

        // 更新总弹药数量（备用弹药）
        if (totalAmmoText != null)
        {
            totalAmmoText.text = reserveAmmo.ToString();
        }
    }
}
