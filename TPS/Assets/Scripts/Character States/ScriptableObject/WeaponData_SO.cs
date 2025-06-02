using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon System/Weapon Data")]
public class WeaponData_SO : ScriptableObject
{
    [Header("基础信息")]
    public string weaponName = "默认武器";
    public string description = "武器描述";
    public Sprite weaponIcon;
    public GameObject weaponPrefab; // 武器模型预制体

    [Header("射击设置")]
    public bool isAutomatic = false; // 是否为全自动
    public float fireRate = 1f; // 射击间隔（秒）
    public float cooldown = 0.5f; // 冷却时间
    public int bulletsPerShot = 1; // 每次射击的子弹数
    public float bulletVelocity = 500f; // 子弹速度
    public int damage = 10; // 伤害值
    public float range = 100f; // 射程
    public float accuracy = 0.95f; // 精度（0-1）

    [Header("弹药系统")]
    public int magazineSize = 30; // 弹夹容量
    public int maxReserveAmmo = 300; // 最大备用子弹数
    public float reloadTime = 2.0f; // 换弹时间
    public bool autoReloadWhenEmpty = true; // 弹夹空时自动换弹

    [Header("音效配置")]
    public AudioClip fireSound; // 射击音效
    public AudioClip autoFireSound; // 全自动射击音效
    public AudioClip reloadSound; // 换弹音效
    public AudioClip emptyClipSound; // 空弹夹音效
    public AudioClip drawSound; // 拔枪音效
    public AudioClip holsterSound; // 收枪音效
    [Range(0f, 1f)]
    public float soundVolume = 1.0f; // 音效音量

    [Header("视觉效果")]
    public ParticleSystem muzzleFlashPrefab; // 枪口火花预制体
    public ParticleSystem shellEjectPrefab; // 弹壳抛射预制体
    public GameObject bulletTrailPrefab; // 子弹轨迹预制体
    public GameObject bulletImpactPrefab; // 子弹撞击效果预制体

    [Header("动画设置")]
    public RuntimeAnimatorController weaponAnimatorController; // 武器动画控制器
    public float drawTime = 0.5f; // 拔枪时间
    public float holsterTime = 0.3f; // 收枪时间

    [Header("UI设置")]
    public Color crosshairColor = Color.white; // 准星颜色
    public float crosshairSize = 1.0f; // 准星大小
    public Sprite ammoIcon; // 弹药图标

    // 运行时数据验证
    private void OnValidate()
    {
        // 确保数值在合理范围内
        fireRate = Mathf.Max(0.01f, fireRate);
        bulletsPerShot = Mathf.Max(1, bulletsPerShot);
        damage = Mathf.Max(1, damage);
        magazineSize = Mathf.Max(1, magazineSize);
        maxReserveAmmo = Mathf.Max(0, maxReserveAmmo);
        reloadTime = Mathf.Max(0.1f, reloadTime);
        accuracy = Mathf.Clamp01(accuracy);
        soundVolume = Mathf.Clamp01(soundVolume);
    }
    // 获取每分钟射速（RPM）
    public float GetRPM()
    {
        return 60f / fireRate;
    }

    // 获取理论DPS
    public float GetTheoreticalDPS()
    {
        return (damage * bulletsPerShot) / fireRate;
    }

    // 获取弹夹持续时间
    public float GetMagazineDuration()
    {
        return magazineSize * fireRate / bulletsPerShot;
    }
}
