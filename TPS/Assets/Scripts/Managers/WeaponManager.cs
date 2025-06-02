using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon DataSO")]
    public WeaponData_SO weaponData; // 武器数据引用

    [Header("Running State")]
    [SerializeField] private int currentAmmo; // 当前弹夹子弹数
    [SerializeField] private int reserveAmmo; // 备用子弹数
    [SerializeField] private bool isReloading = false; // 是否正在换弹
    [HideInInspector] public float cooldown = 0f; // 当前冷却时间
    [HideInInspector] public float fireRate; // 射击间隔

    [Header("References")]
    public Transform firePoint;
    [Header("WeaponAudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip singleShotClip;
    [SerializeField] private AudioClip autoLoopClip;
    [Header("Particle")]
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem bloodEffect;

    // 事件系统
    public System.Action<int, int> OnAmmoChanged; // 弹药变化事件 (当前弹药, 备用弹药)
    public System.Action<bool> OnReloadStateChanged; // 换弹状态变化事件
    public System.Action<WeaponData_SO> OnWeaponChanged; // 武器切换事件
    public System.Action OnWeaponFired; // 武器射击事件
    public System.Action OnWeaponEmpty; // 武器空弹事件

    private bool isEnemy;
    private ParticleSystem muzzleFlashInstance;
    private ParticleSystem shellEjectInstance;
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        InitializeWeapon();
    }

    private void Start()
    {
        // 触发初始弹药事件
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        OnWeaponChanged?.Invoke(weaponData);
    }


    #region 更新循环
    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
    }
    #endregion

    private void InitializeWeapon()
    {
        if (weaponData == null)
        {
            Debug.LogError($"WeaponManager on {gameObject.name}: 未分配武器数据！");
            return;
        }

        // 初始化弹药
        currentAmmo = weaponData.magazineSize;
        reserveAmmo = weaponData.maxReserveAmmo;

        // 同步武器数据到公共变量
        fireRate = weaponData.fireRate;
        cooldown = weaponData.cooldown;
    }
    //TODO：武器切换

    #region 射击系统
    /// <summary>
    /// 处理射击输入
    /// </summary>
    public void HandleShooting(bool shootPressed, bool shootHeld, bool shootReleased, RaycastHit raycastHit)
    {
        if (weaponData == null || isReloading) return;
        if (weaponData.isAutomatic)
        {
            if (shootHeld && CanFire())
            {
                TryFire(raycastHit);
            }

            if (shootReleased)
            {
                // 停止自动枪音效等逻辑
                StopAutoFireAudio();
            }
        }
        else
        {
            // 半自动射击
            if (shootPressed && CanFire())
            {
                TryFire(raycastHit);
            }
        }
    }

    /// <summary>
    /// 尝试射击
    /// </summary>
    private bool CanFire()
    {
        return currentAmmo > 0 && cooldown <= 0f && !isReloading;
    }

    public void TryFire(RaycastHit raycastHit)
    {
        if (!CanFire()) return;

        // 执行射击
        Fire(raycastHit);

        // 消耗弹药
        ConsumeAmmo();

        // 设置冷却时间
        cooldown = weaponData.fireRate;

        // 触发射击事件
        OnWeaponFired?.Invoke();
    }

    /// <summary>
    /// 执行射击逻辑
    /// </summary>
    private void Fire(RaycastHit raycastHit)
    {
        // 计算射击方向
        Vector3 shootDirection = CalculateShootDirection(raycastHit);
        Debug.DrawLine(firePoint.position, raycastHit.point, Color.red, 1.0f);
        // 调用对象池生成子弹
        // 生成子弹
        for (int i = 0; i < weaponData.bulletsPerShot; i++)
        {
            FireBullet(shootDirection);
        }

        // 播放音效
        PlayFireAudio();

        // 播放视觉效果
        //PlayMuzzleFlash();

        // 处理命中
        HandleHit(raycastHit);
    }

    private Vector3 CalculateShootDirection(RaycastHit raycastHit)
    {
        Vector3 direction = (raycastHit.point - firePoint.position).normalized;

        // 添加精度影响（随机偏移）
        float inaccuracy = 1f - weaponData.accuracy;
        direction += Random.insideUnitSphere * inaccuracy * 0.1f;

        return direction.normalized;
    }

    private void FireBullet(Vector3 direction)
    {
        // 使用对象池生成子弹
        GameObject bullet = BulletPool.Instance.TryGetBullet();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.AddForce(direction * weaponData.bulletVelocity, ForceMode.Impulse);
            }
        }
    }

    private void HandleHit(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null) return;

        if (raycastHit.collider.CompareTag("Enemy"))
        {
            var enemy = raycastHit.collider.GetComponent<ZombieStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }

        // 生成撞击效果
        if (weaponData.bulletImpactPrefab != null)
        {
            Instantiate(weaponData.bulletImpactPrefab, raycastHit.point,
                       Quaternion.LookRotation(raycastHit.normal));
        }
    }
    #endregion

    #region 弹药系统
    /// <summary>
    /// 消耗弹药
    /// </summary>
    private void ConsumeAmmo()
    {
        currentAmmo -= weaponData.bulletsPerShot;
        Debug.Log(currentAmmo);
        if (currentAmmo < 0) currentAmmo = 0;

        // 触发弹药变化事件
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);

        // 检查是否需要自动换弹
        if (currentAmmo <= 0)
        {
            OnWeaponEmpty?.Invoke();

            if (weaponData.autoReloadWhenEmpty && reserveAmmo > 0)
            {
                StartReload();
            }
            else
            {
                PlayEmptyClipSound();
            }
        }
    }

    /// <summary>
    /// 开始换弹
    /// </summary>
    public void StartReload()
    {
        if (!CanReload()) return;

        StartCoroutine(ReloadCoroutine());
    }

    /// <summary>
    /// 检查是否可以换弹
    /// </summary>
    public bool CanReload()
    {
        return !isReloading &&
               currentAmmo < weaponData.magazineSize &&
               reserveAmmo > 0;
    }

    /// <summary>
    /// 换弹协程
    /// </summary>
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        // 播放换弹音效
        PlayReloadAudio();

        Debug.Log($"开始换弹 {weaponData.weaponName}...");

        // 等待换弹时间
        yield return new WaitForSeconds(weaponData.reloadTime);

        // 计算实际装填的子弹数
        int ammoNeeded = weaponData.magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        // 执行换弹
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        isReloading = false;
        OnReloadStateChanged?.Invoke(false);

        // 触发弹药变化事件
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);

        Debug.Log($"换弹完成！当前: {currentAmmo}/{weaponData.magazineSize}, 备用: {reserveAmmo}");
    }

    /// <summary>
    /// 添加备用弹药
    /// </summary>
    public void AddReserveAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(reserveAmmo + amount, weaponData.maxReserveAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);

        Debug.Log($"获得 {amount} 发 {weaponData.weaponName} 弹药，当前备用: {reserveAmmo}");
    }
    #endregion

    #region 音效系统
    private void PlayFireAudio()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = weaponData.isAutomatic ?
                              weaponData.autoFireSound : weaponData.fireSound;

        if (clipToPlay != null)
        {
            if (weaponData.isAutomatic)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = clipToPlay;
                    audioSource.loop = true;
                    audioSource.volume = weaponData.soundVolume;
                    audioSource.Play();
                }
            }
            else
            {
                audioSource.PlayOneShot(clipToPlay, weaponData.soundVolume);
            }
        }
    }

    private void StopAutoFireAudio()
    {
        if (audioSource != null && weaponData.isAutomatic)
        {
            audioSource.Stop();
        }
    }

    private void PlayReloadAudio()
    {
        if (audioSource != null && weaponData.reloadSound != null)
        {
            audioSource.PlayOneShot(weaponData.reloadSound, weaponData.soundVolume);
        }
    }

    private void PlayEmptyClipSound()
    {
        if (audioSource != null && weaponData.emptyClipSound != null)
        {
            audioSource.PlayOneShot(weaponData.emptyClipSound, weaponData.soundVolume);
        }
    }
    #endregion

    #region 公共访问器
    public WeaponData_SO GetWeaponData() => weaponData;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => reserveAmmo;
    public int GetMagazineSize() => weaponData?.magazineSize ?? 0;
    public bool IsReloading() => isReloading;
    public bool HasAmmo() => currentAmmo > 0 || reserveAmmo > 0;
    public float GetReloadProgress()
    {
        // 可以通过协程或其他方式实现更精确的进度计算
        return isReloading ? 0.5f : 1f;
    }
    #endregion

}
