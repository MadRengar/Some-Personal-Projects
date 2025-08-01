using UnityEngine;
using System;

public enum WeaponType
{
    Rifle,
    Hammer
}

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponType currentWeapon = WeaponType.Rifle;

    [Header("Weapon Objects")]
    public GameObject rifleObject;      // 步枪游戏对象
    public GameObject hammerObject;     // 锤子游戏对象（暂时可以为空）

    // 武器切换事件
    private Animator animator;
    private WeaponManager weaponManager;
    public static event Action<WeaponType> OnWeaponChanged;

    void Start()
    {
        animator = GetComponent<Animator>();
        weaponManager = GameManager.Instance.GetPlayerWeaponManager(); 
        // 初始化武器状态
        SwitchToWeapon(currentWeapon);
    }

    /// <summary>
    /// 切换到下一个武器
    /// </summary>
    public void SwitchToNextWeapon()
    {
        WeaponType nextWeapon = currentWeapon == WeaponType.Rifle ? WeaponType.Hammer : WeaponType.Rifle;
        SwitchToWeapon(nextWeapon);
    }

    /// <summary>
    /// 切换到上一个武器
    /// </summary>
    public void SwitchToPreviousWeapon()
    {
        WeaponType prevWeapon = currentWeapon == WeaponType.Rifle ? WeaponType.Hammer : WeaponType.Rifle;
        SwitchToWeapon(prevWeapon);
    }

    /// <summary>
    /// 切换到指定武器
    /// </summary>
    public void SwitchToWeapon(WeaponType weaponType)
    {
        if (currentWeapon == weaponType) return; // 已经是当前武器
        if (weaponManager.IsReloading()) return;
        currentWeapon = weaponType;

        // 更新武器对象显示状态
        UpdateWeaponDisplay();

        // 触发武器切换事件
        OnWeaponChanged?.Invoke(currentWeapon);
    }

    /// <summary>
    /// 更新武器显示状态
    /// </summary>
    private void UpdateWeaponDisplay()
    {
        if (rifleObject != null)
        {
            rifleObject.SetActive(currentWeapon == WeaponType.Rifle);
        }

        if (hammerObject != null)
        {
            hammerObject.SetActive(currentWeapon == WeaponType.Hammer);
        }
    }

    /// <summary>
    /// 获取当前武器类型
    /// </summary>
    public WeaponType GetCurrentWeapon()
    {
        return currentWeapon;
    }

    /// <summary>
    /// 检查是否是步枪
    /// </summary>
    public bool IsRifle()
    {
        return currentWeapon == WeaponType.Rifle;
    }

    /// <summary>
    /// 检查是否是锤子
    /// </summary>
    public bool IsHammer()
    {
        return currentWeapon == WeaponType.Hammer;
    }
}